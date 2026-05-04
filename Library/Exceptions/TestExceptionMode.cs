namespace SoEx.Exceptions;

public class TestExceptionMode
{
    public ExceptionMode Mode { get; init; }
}

public enum ExceptionMode
{
    Production,
    Wrapped,
    Bare
}

