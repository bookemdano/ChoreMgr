using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChoreMgr.Pages
{
    public class BasePageModel : PageModel
    {
        protected string? UserName
        {
            get 
            { 
                return HttpContext.Request.HttpContext.Connection.RemoteIpAddress?.ToString(); 
            }
        }
    }
}
