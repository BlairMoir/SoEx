using Example101.iFx.Proxy;
using Example101.Manager.Phenonmenon.Interface;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Example001.Host.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;                
    }

    public async Task OnGet()
    {                           
        var phenonmenonProxy = Proxy.ForService<IPhenonmenonManager>();
        await phenonmenonProxy.Observe(new ObservationRequest());        
        _logger.LogInformation("Hello!");
    }
}
