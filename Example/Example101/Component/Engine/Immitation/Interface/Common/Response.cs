namespace Example101.Engine.Immitation.Interface
{
    public class Response<T>
    {
        public T? Result { get; internal set; }
        public IEnumerable<ErrorInfo> Errors { get; internal set; } = Enumerable.Empty<ErrorInfo>();
    }
}