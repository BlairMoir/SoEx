using System.Reflection;
using SoEx.Context;

namespace SoExTemplate.iFx.Convention;

public static class Scan
{
    public static Type[] ServiceTypes(string company)
    {
        string[] serviceSuffixConventionKeywords = [Keywords.Manager, Keywords.Engine, Keywords.Access, Keywords.Utility];
        List<Type> foundTypes = [];

        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var assemblyFiles = Directory.GetFiles(path!, $"{company}.*.{Keywords.Service}.dll", SearchOption.TopDirectoryOnly);
        foreach (var assemblyFile in assemblyFiles)
        {
            var assembly = Assembly.LoadFrom(assemblyFile);
            var types = assembly.GetTypes().Where(t => serviceSuffixConventionKeywords.Any(s => t.Name.EndsWith(s)));
            foundTypes.AddRange(types);

        }
        return foundTypes.ToArray();
    }

    public static Type[] ProxyTypes(string company)
    {
        string[] serviceSuffixConventionKeywords = [Keywords.Manager, Keywords.Engine, Keywords.Access, Keywords.Utility, Keywords.Event];
        List<Type> foundTypes = [];

        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var assemblyFiles = Directory.GetFiles(path!, $"{company}.*.{Keywords.Interface}.dll", SearchOption.TopDirectoryOnly);
        foreach (var assemblyFile in assemblyFiles)
        {
            var assembly = Assembly.LoadFrom(assemblyFile);
            var types = assembly.GetTypes().Where(t => serviceSuffixConventionKeywords.Any(s => t.Name.EndsWith(s) && t.IsInterface));
            foundTypes.AddRange(types);
        }
        return foundTypes.ToArray();
    }

    public static Type[] ContextPolicyTypes(string company)
    {
        List<Type> foundTypes = [];

        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var assemblyFiles = Directory.GetFiles(path!, $"{company}.*.{Keywords.Policy}.dll", SearchOption.TopDirectoryOnly);
        foreach (var assemblyFile in assemblyFiles)
        {
            var assembly = Assembly.LoadFrom(assemblyFile);
            var types = assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IContextFlowPolicy)));
            foundTypes.AddRange(types);
        }
        return foundTypes.ToArray();
    }
}
