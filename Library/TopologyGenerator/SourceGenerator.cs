using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SoEx.Test.TopologyGenerator
{
    public class SourceGenerator
    {
        [Generator]
        public class TopologyGenerator : IIncrementalGenerator
        {
            public void Initialize(IncrementalGeneratorInitializationContext context)
            {
                context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                    "TopologyAttribute.g.cs",
                    SourceText.From(Attribute, Encoding.UTF8)));

                IncrementalValuesProvider<TopologyToGenerate> stuff = context.SyntaxProvider
                    .ForAttributeWithMetadataName(
                        "SoEx.Test.HarnessGenerators.TopologyAttribute",
                        predicate: static (_, _) => true,
                        transform: (ctx, _) => GetSemanticTargetForGeneration(ctx)
                        );

                context.RegisterSourceOutput(stuff, (spc, o) =>
                {
                    try
                    {
                        StringBuilder fields = new StringBuilder();
                        StringBuilder components = new StringBuilder();
                        List<string> proxies = new List<string>();
                        Dictionary<string, string> names = MemberNames(new[] { o.SubsystemContract ?? "" }
                            .Concat(o.Components.Select(c => c.Contract)));
                        fields.AppendLine(
                            $$"""
                              public Microsoft.Extensions.DependencyInjection.ServiceCollection ServiceCollection_{{names[o.SubsystemContract ?? ""]}} = new();
                          """);

                        foreach (Component component in o.Components)
                        {
                            proxies.Add(
                                $$"""
                            new SoEx.Topology.Client<{{component.Contract}}>()
                            {
                                Service = new SoEx.Transport.InProc.InProcBinding<{{component.Contract}}>("Test"),
                                SubSystem = "Test"
                            },
                            """);

                            if (component.Implementation is null)
                            {
                                fields.AppendLine(
                                    $$"""
                                    public Moq.Mock<{{component.Contract}}> Mock_{{names[component.Contract]}} = new Moq.Mock<{{component.Contract}}>();
                                """);
                                components.AppendLine("new SoEx.Topology.HostMock()");
                            }
                            else
                            {
                                fields.AppendLine(
                                    $$"""
                                      public Microsoft.Extensions.DependencyInjection.ServiceCollection ServiceCollection_{{names[component.Contract]}} = new();
                                  """);
                                components.AppendLine("new SoEx.Topology.Host()");
                            }

                            components.AppendLine("    {");
                            if (component.Implementation is null)
                            {
                                components.AppendLine($"        Instance = Mock_{names[component.Contract]}.Object,");
                                components.AppendLine($"        Implementation = typeof(Moq.Mock),");
                            }
                            else
                            {
                                components.AppendLine($"        Implementation = typeof({component.Implementation}),");
                            }
                            components.AppendLine($"        Endpoints = [new SoEx.Transport.InProc.InProcBinding<{component.Contract}>(\"Test\")],");
                            components.AppendLine($"        Proxies = [");
                            foreach (string proxy in proxies)
                            {
                                if (!proxy.Contains(component.Contract))
                                {
                                    if (component.Contract.EndsWith("Access") && !proxy.Contains("Manager>") && !proxy.Contains("Engine>") && !proxy.Contains("Event>"))
                                    {
                                        components.AppendLine(proxy);
                                    }
                                    else if (component.Contract.EndsWith("Engine") && !proxy.Contains("Manager>") && !proxy.Contains("Event>"))
                                    {
                                        components.AppendLine(proxy);
                                    }
                                    else
                                    {
                                        components.AppendLine(proxy);
                                    }
                                }
                            }
                            components.AppendLine($"        ],");
                            if (component.Implementation is not null)
                            {
                                components.AppendLine(
                                    $"        ServiceCollection = ServiceCollection_{names[component.Contract] ?? ""}");
                            }
                            components.AppendLine("    },");
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
                        //
                        // </auto-generated>
                        //------------------------------------------------------------------------------
                        public partial class HarnessFor_{{o.MethodIdentifier}} : SoEx.Test.TestEnvironmentBase
                        {
                            {{fields}}

                            public Task TestService(Func<{{o.SubsystemContract}}, Task> callerFunc){
                                return TestService<{{o.SubsystemContract}}>(callerFunc, Topology);
                            }

                            public SoEx.Topology.System Topology =>
                                new SoEx.Topology.System {
                                    SubSystems = [
                                        new SoEx.Topology.SubSystem()
                                        {
                                            Name = "Test",
                                            EntryPoint = new SoEx.Topology.Host()
                                            {
                                                Implementation = typeof({{o.SubsystemImplementation}}),
                                                Endpoints = [new SoEx.Transport.InProc.InProcBinding<{{o.SubsystemContract}}>("Test")],
                                                Proxies = [
                                                    {{String.Join("\r\n", proxies)}}
                                                ],
                                                ServiceCollection = ServiceCollection_{{names[o.SubsystemContract ?? ""]}}
                                            },
                                            Components = [
                                                {{components.ToString()}}
                                            ]
                                        }
                                    ],
                                    Clients = [
                                        new SoEx.Topology.Client<{{o.SubsystemContract}}>()
                                        {
                                            Service = new SoEx.Transport.InProc.InProcBinding<{{o.SubsystemContract}}>("Test"),
                                            SubSystem = "Test"
                                        }
                                    ]
                            };
                        }
                        """;

                        spc.AddSource($"{o.MethodIdentifier}.g.cs", source);
                    }
                    catch (Exception ex)
                    {
                        spc.ReportDiagnostic(
                            Diagnostic.Create(
                                new DiagnosticDescriptor("SOEXTG001",
                                    "Topology generation failed",
                                    "Could not generate harness for {0}: {1}",
                                "SoEx.Test", DiagnosticSeverity.Error, true),
                            Location.None, o.MethodIdentifier, ex.Message
                            ));
                    }
                });
            }

            static Dictionary<string, string> MemberNames(IEnumerable<string> contracts)
            {
                var names = new Dictionary<string, string>();
                foreach (IGrouping<string, string>? group in contracts.Distinct().GroupBy(c => c.Split('.').Last()))
                {
                    string[] collision = group.ToArray();
                    if (collision.Length == 1)
                    {
                        names[collision[0]] = group.Key;
                        continue;
                    }

                    int longest = collision.Max(c => c.Split('.').Length);
                    for (int take = 2; ; take++)
                    {
                        Dictionary<string, string> qualified = collision.ToDictionary(c => c, c => Qualify(c, take));
                        if (qualified.Values.Distinct().Count() == collision.Length || take >= longest)
                        {
                            foreach (KeyValuePair<string, string> pair in qualified)
                            {
                                names[pair.Key] = pair.Value;
                            }

                            break;
                        }
                    }
                }

                return names;
            }

            static string Qualify(string contract, int segments)
            {
                string[] parts = contract.Split('.');
                int skip = Math.Max(0, parts.Length - segments);
                IEnumerable<string> qualifiedParts = parts.Skip(skip);
                return string.Join("_", qualifiedParts);
            }

            private static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode)
                => syntaxNode is MethodDeclarationSyntax methodSyntax && methodSyntax.AttributeLists.Count > 0;

            static TopologyToGenerate GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
            {
                string methodIdentifier = $"{context.TargetSymbol.ContainingSymbol.Name}_{context.TargetSymbol.Name}";

                var methodSymbol = context.TargetSymbol as IMethodSymbol;
                var attributes = methodSymbol?.GetAttributes().Where(w => w.AttributeClass?.Name == "TopologyAttribute").ToList();

                AttributeData? topologyAttribute = attributes?.FirstOrDefault();

                TypedConstant[]? constructorArguments = topologyAttribute?.ConstructorArguments.ToArray();
                if (constructorArguments is not null)
                {
                    var entrypoint = constructorArguments[0].Value as INamedTypeSymbol;
                    INamedTypeSymbol?[] componentsSymbols = constructorArguments[1].Values.Select(s => s.Value as INamedTypeSymbol).ToArray();

                    ITypeSymbol? entryPointContract = entrypoint?.TypeArguments[0];
                    ITypeSymbol? entryPointImplementation = entrypoint?.TypeArguments[1];

                    List<Component> components = [];
                    foreach (INamedTypeSymbol? componentsSymbol in componentsSymbols)
                    {
                        if (componentsSymbol?.TypeArguments.Length == 2)
                        {
                            components.Add(new Component(
                                $"{componentsSymbol.TypeArguments[0].ContainingNamespace}.{componentsSymbol.TypeArguments[0].Name}",
                                $"{componentsSymbol.TypeArguments[1].ContainingNamespace}.{componentsSymbol.TypeArguments[1].Name}"
                                ));
                        }
                        if (componentsSymbol?.TypeArguments.Length == 1)
                        {
                            components.Add(new Component(
                                $"{componentsSymbol.TypeArguments[0].ContainingNamespace}.{componentsSymbol.TypeArguments[0].Name}",
                                null
                                ));
                        }
                    }

                    var topologyToGenerate = new TopologyToGenerate(
                        methodIdentifier,
                        $"{entryPointContract?.ContainingNamespace}.{entryPointContract?.Name}",
                        $"{entryPointImplementation?.ContainingNamespace}.{entryPointImplementation?.Name}",
                        components.ToArray()
                        );

                    return topologyToGenerate;
                }

                var failedGenerate = new TopologyToGenerate(
                    methodIdentifier,
                    null,
                    null,
                    []
                    );
                return failedGenerate;
            }

            public readonly record struct TopologyToGenerate
            {
                public readonly string MethodIdentifier;
                public readonly string? SubsystemContract;
                public readonly string? SubsystemImplementation;
                public readonly Component[] Components;

                public TopologyToGenerate(string methodIdentifier, string? subsystemContract, string? subsystemImplementation, Component[] components)
                {
                    MethodIdentifier = methodIdentifier;
                    SubsystemContract = subsystemContract;
                    SubsystemImplementation = subsystemImplementation;
                    Components = components;
                }
            }

            public readonly record struct Component
            {
                public readonly string Contract;
                public readonly string? Implementation;

                public Component(string contract, string? implementation)
                {
                    Contract = contract;
                    Implementation = implementation;
                }
            }


            public const string Attribute = @"
namespace SoEx.Test.HarnessGenerators
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TopologyAttribute : Attribute
    {
        public TopologyAttribute(Type entryPoint, Type[] components)
        {
            EntryPoint = entryPoint;
            Components = components;
        }

        public virtual Type EntryPoint { get; set; }
        public virtual Type[] Components { get; set; }
    }

    public class Host<C, S> where C : class where S : class
    {

    }

    public class MockHost<C> where C : class
    {

    }
}";
        }
    }
}
