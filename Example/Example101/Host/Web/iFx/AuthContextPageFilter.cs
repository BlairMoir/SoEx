using Example101.Common.Contract;
using Example101.iFx.Service;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.iFx
{
    public class AuthContextPageFilter : IAsyncPageFilter
    {
        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {            
            Context<AuthContext>.SetContext(new AuthContext(){  Principal = context.HttpContext.User.Clone() }); 
            await next.Invoke();
        }
        public async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) 
        {
            await Task.CompletedTask;
        }
    }
}