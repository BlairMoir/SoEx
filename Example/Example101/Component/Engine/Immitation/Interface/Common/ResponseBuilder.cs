namespace Example101.Engine.Immitation.Interface
{
    public static class ResponseBuilder
    {
        public static Response<T> Response<T>(T result, IEnumerable<ErrorInfo> errors)
        {
            return new Response<T>() { 
                Result = result, 
                Errors = errors
            };
        }
        public static Response<T> Response<T>(T result)
        {
            return Response<T>(result, Enumerable.Empty<ErrorInfo>());            
        }

        public static ErrorInfo ErrorInfo(int code, string description)
        {
            return new ErrorInfo()
            {
                Code = code,
                Description = description
            };
        }
    }
}