
using System.Diagnostics;

namespace SoEx.Diagnostics
{
    public static class ActivitySources
    {
        public static readonly ActivitySource Client = new(ActivitySourceNames.Client, "1.0.0");
        public static readonly ActivitySource Host = new(ActivitySourceNames.Host, "1.0.0");
    }
}
