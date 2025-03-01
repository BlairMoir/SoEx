using Example101.Common.Contract;
using Example101.iFx.Service;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.iFx
{
    public class LifetimeScopePageFilter : IAsyncPageFilter
    {
        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            using(SoEx.Container.BeginLocalLifetimeScope())
            {                
                await next.Invoke();
            }
        }
        public async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) 
        {
            await Task.CompletedTask;
        }
    }
}