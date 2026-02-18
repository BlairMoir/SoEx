namespace SoEx.Context
{
    public static class ContextExtensions
    {
        public static void CopyIfExists<T>(this IAmbientContext source, IAmbientContext destination) where T : struct
        {
            if (!source.Contains<T>())
            {
                return;
            }
            destination.SetOrReplace(source.Get<T>());
        }
    }
}
