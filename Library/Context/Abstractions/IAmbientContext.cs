namespace SoEx.Context
{
    public interface IAmbientContext
    {
        public void SetIfNotExists<T>(Func<T> contextFactory) where T : class;
        public T Get<T>() where T : class;
        public bool Contains<T>() where T : class;
        public void SetOrReplace<T>(T context) where T : class;
    }
}

