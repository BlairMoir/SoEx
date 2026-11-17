using System.Reflection;

namespace SoEx.Hosting;

internal static class ContractExtensions
{
    internal static MethodInfo[] Operations(this Type contract)
    {
        var operations = new List<MethodInfo>();
        operations.AddRange(contract.GetMethods());

        foreach (Type inherited in contract.GetInterfaces())
        {
            operations.AddRange(inherited.GetMethods());
        }

        return operations.ToArray();
    }

    internal static MethodInfo? Find(this Type contract, string methodName)
    {
        MethodInfo[] operations = Operations(contract);
        MethodInfo? operation = operations.FirstOrDefault(o => o.Name == methodName);
        return operation;
    }
}
