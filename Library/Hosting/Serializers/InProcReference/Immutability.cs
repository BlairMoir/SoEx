namespace SoEx.Hosting.Serializers.InProcReference;

public static class Immutability
{
    public static object? Require(object? obj)
    {
        //TODO: Check immutability
        return obj;
    }
    public static object?[] Require(object?[] obj)
    {
        //TODO: Check immutability
        return obj;
    }

    public static object?[] Clone(object?[] obj)
    {
        return (object?[])obj.Clone();
    }
}
