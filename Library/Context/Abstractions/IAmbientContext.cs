namespace SoEx.Context
{
    public interface IAmbientContext
    {
        public void SetIfNotExists<T>(Func<T> contextFactory) where T : notnull;
        public T Get<T>() where T : notnull;
        public bool Contains<T>() where T : notnull;
        public void SetOrReplace<T>(T context) where T : notnull;
    }
}

