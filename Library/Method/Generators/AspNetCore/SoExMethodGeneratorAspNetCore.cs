using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SoEx.Method.Generators.AspNetCore
{
    [Generator]
    public class SoExMethodGeneratorAspNetCore : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValueProvider<(AnalyzerConfigOptionsProvider Left, Compilation Right)> analyzerAndCompilationProviders =
                context.AnalyzerConfigOptionsProvider
                    .Combine(context.CompilationProvider);

            IncrementalValueProvider<StuffToGenerate> referencedSymbols = analyzerAndCompilationProviders.Select((combined, _) =>
            {
                AnalyzerConfigOptionsProvider config = combined.Left;
                Compilation compilation = combined.Right;
                config.GlobalOptions
                    .TryGetValue($"build_property.MethodGeneratorNamespace", out string? @namespace);
                var myAssemblySymbols = compilation.SourceModule.ReferencedAssemblySymbols
                    .Where(w => w.Name.Contains($"{@namespace}.Manager.") && w.Name.EndsWith(".Interface")).ToList();
                var stuff = new StuffToGenerate(
                    @namespace ?? "Company",
                    myAssemblySymbols.SelectMany(s => InterfacesFromNamespace(s.GlobalNamespace.GetMembers())),
                    myAssemblySymbols.Select(s => KnownTypesFromNamespace(s.GlobalNamespace.GetMembers())).ToList()
                );
                return stuff;
            });

            context.RegisterSourceOutput(referencedSymbols, static (spc, opts) =>
            {
                try
                {
                    StuffToGenerate symbols = opts;
                    InterfaceToGenerate[] interfaceSymbols = symbols.Interfaces.ToArray();
                    List<ImmutableDictionary<string, List<string>>> knownTypes = symbols.KnownTypes;

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($$"""
                                    namespace {{symbols.MethodNamespace}}.Generated;
                                    public static class SoExKnownTypes
                                    {
                                        public static bool TryGetType(Type baseType, out Type[] derivedTypes)
                                        {
                                            derivedTypes = baseType.FullName switch {
                                    """);
                    foreach (ImmutableDictionary<string, List<string>> set in knownTypes)
                    {
                        foreach (KeyValuePair<string, List<string>> pair in set)
                        {
                            sb.Append(
                                    $$"""
                                               "{{pair.Key}}" => [
                                   """);
                            foreach (string item in pair.Value)
                            {
                                sb.Append($@" typeof({item}), ");
                            }
                            sb.AppendLine("],");
                        }
                    }

                    sb.AppendLine("""
                                                _ => []
                                           };
                                           return derivedTypes.Length > 0;
                                       }
                                   }
                                   """);
                    spc.AddSource($"SoExKnownTypes.cs", sb.ToString());

                    foreach (InterfaceToGenerate interfaceSymbol in interfaceSymbols)
                    {
                        StringBuilder operationSource = new StringBuilder();
                        foreach (Operation operation in interfaceSymbol.Operations)
                        {
                            string operationText =
                                       $$"""


                                            [HttpPost]
                                            public async Task<{{(operation.ReturnType is null ? "IActionResult" : $"ActionResult<{operation.ReturnType}>")}}> {{operation.Name}}({{string.Join(", ", operation.Parameters.Select(s => $"[FromBody]{s}"))}})
                                            {
                                                var proxy = {{symbols.MethodNamespace}}.iFx.Proxy.Proxy.ForService<{{interfaceSymbol.Namespace}}.{{interfaceSymbol.Name}}>();
                                                {{(operation.ReturnType is null ? "" : "var result =")}} await proxy.{{operation.Name}}({{String.Join(", ", operation.Parameters.Select(s => s.Split(' ')[1]))}});
                                                return {{(operation.ReturnType is null ? "Ok()" : "result")}};
                                            }
                                    """;
                            operationSource.Append(operationText);
                        }

                        string source =
                                   $$"""
                                    //------------------------------------------------------------------------------
                                    // <auto-generated>
                                    //     This code was generated by a tool.
                                    //
                                    //     Changes to this file may cause incorrect behavior and will be lost if
                                    //     the code is regenerated.
                                    //     Generated at: {{DateTime.Now}}
                                    //     Generated From:: {{interfaceSymbol.Namespace}}
                                    // </auto-generated>
                                    //------------------------------------------------------------------------------

                                    using Microsoft.AspNetCore.Mvc;

                                    namespace {{symbols.MethodNamespace}}.Generated.Controllers;

                                    [Route("[controller]/[action]")]
                                    [ApiController]
                                    public partial class {{interfaceSymbol.Name}}Controller : ControllerBase
                                    { {{operationSource}}
                                    }
                                """;
                        spc.AddSource($"{interfaceSymbol.Name}Controller.g.cs", source);
                    }
                }
                catch (Exception ex)
                {
                    spc.AddSource($"error.txt", ex.ToString());
                }
            });
        }

        static IEnumerable<InterfaceToGenerate> InterfacesFromNamespace(IEnumerable<INamespaceOrTypeSymbol> symbols)
        {
            List<InterfaceToGenerate> typeSymbols = new List<InterfaceToGenerate>();
            foreach (INamespaceOrTypeSymbol symbol in symbols)
            {
                if (symbol is INamespaceSymbol namespaceSymbol)
                {
                    IEnumerable<INamespaceOrTypeSymbol> namespaceMembers = namespaceSymbol.GetMembers();
                    typeSymbols.AddRange(InterfacesFromNamespace(namespaceMembers));
                }
                else if (symbol is ITypeSymbol { TypeKind: TypeKind.Interface } typeSymbol)
                {
                    IEnumerable<IMethodSymbol> methodMembers = typeSymbol.GetMembers().OfType<IMethodSymbol>();
                    typeSymbols.Add(new InterfaceToGenerate(
                        typeSymbol.Name,
                        typeSymbol.ContainingNamespace.ToString(),
                        methodMembers.Select(s =>
                            new Operation(
                                PrepareReturnType(s.ReturnType),
                                s.Name,
                                s.Parameters.Select(s => $"{s.Type.ContainingNamespace}.{s.Type.Name} {s.Name}").ToList()
                                )).ToList()
                        )
                    );
                }
            }
            return typeSymbols;
        }

        static ImmutableDictionary<string, List<string>> KnownTypesFromNamespace(IEnumerable<INamespaceOrTypeSymbol> symbols)
        {
            Dictionary<string, List<string>> knownTypes = new Dictionary<string, List<string>>();
            foreach (INamespaceOrTypeSymbol symbol in symbols)
            {
                if (symbol is INamespaceSymbol namespaceSymbol)
                {
                    IEnumerable<INamespaceOrTypeSymbol> namespaceMembers = namespaceSymbol.GetMembers();

                    ImmutableDictionary<string, List<string>> returned = KnownTypesFromNamespace(namespaceMembers);
                    foreach (KeyValuePair<string, List<string>> set in returned)
                    {
                        if (!knownTypes.ContainsKey(set.Key))
                            knownTypes.Add(set.Key, new List<string>());

                        foreach (string val in set.Value)
                        {
                            knownTypes[set.Key].Add(val);
                        }
                    }
                }
                else if (symbol is ITypeSymbol { TypeKind: TypeKind.Class } typeSymbol)
                {
                    if (typeSymbol.BaseType?.SpecialType == SpecialType.None)
                    {
                        INamedTypeSymbol? baseType = typeSymbol.BaseType;
                        while (baseType?.BaseType != null && baseType.BaseType.SpecialType != SpecialType.System_Object)
                        {
                            baseType = baseType.BaseType;
                        }

                        if (baseType is not null)
                        {
                            string baseTypeFullName = $"{baseType.ContainingNamespace}.{baseType.Name}";
                            if (!knownTypes.ContainsKey(baseTypeFullName))
                                knownTypes.Add(baseTypeFullName, new List<string>());

                            string typeFullName = $"{symbol.ContainingNamespace}.{symbol.Name}";
                            knownTypes[baseTypeFullName].Add(typeFullName);
                        }
                    }
                }
            }
            return knownTypes.ToImmutableDictionary();
        }

        private static string? PrepareReturnType(ITypeSymbol argReturnType)
        {
            if (argReturnType is INamedTypeSymbol
                {
                    IsGenericType: true,
                    Name: "Task",
                    TypeArguments.Length: 1
                } namedTypeSymbol)
            {
                return namedTypeSymbol.TypeArguments[0].ToString();
            }

            if (argReturnType is INamedTypeSymbol
                {
                    IsGenericType: false,
                    Name: "Task",
                })
            {
                return null;
            }

            if (argReturnType.SpecialType == SpecialType.System_Void)
            {
                return null;
            }

            return argReturnType.ToString();
        }

        public readonly record struct StuffToGenerate
        {
            public readonly string MethodNamespace;
            public readonly EquatableArray<InterfaceToGenerate> Interfaces;
            public readonly List<ImmutableDictionary<string, List<string>>> KnownTypes;

            public StuffToGenerate(string methodNamespace,
                IEnumerable<InterfaceToGenerate> interfaces, List<ImmutableDictionary<string, List<string>>> knownTypes)
            {
                MethodNamespace = methodNamespace;
                Interfaces = new EquatableArray<InterfaceToGenerate>([.. interfaces]);
                KnownTypes = knownTypes;
            }

        }

        public readonly record struct InterfaceToGenerate
        {
            public readonly string Name;
            public readonly string Namespace;
            public readonly EquatableArray<Operation> Operations;
            public InterfaceToGenerate(string name, string @namespace, List<Operation> operations)
            {
                this.Name = name;
                Namespace = @namespace;
                Operations = new EquatableArray<Operation>([.. operations]);
            }
        }

        public readonly record struct Operation
        {
            public readonly string? ReturnType;
            public readonly string Name;
            public readonly EquatableArray<string> Parameters;
            public Operation(string? returnType, string name, List<string> parameters)
            {
                ReturnType = returnType;
                Name = name;
                Parameters = new EquatableArray<string>([.. parameters]);
            }
        }
    }
}
