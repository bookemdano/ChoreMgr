using ChoreMgr.Utils;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChoreMgr.Pages
{
    public class BasePageModel : PageModel
    {
        protected string? UserName
        {
            get
            {
                return HttpContext.Request.Cookies["userName"];

                //return HttpContext.Request.HttpContext.Connection.RemoteIpAddress?.ToString(); 
            }
        }
        protected bool IsAuthed()
        {
            var userName = UserName;
            if (userName == null)
                return false;
            var rv = (userName.EndsWith("francis@gmail.com") || userName == "bookemdano@gmail.com");
            if (rv == false)
                DanLogger.Log("Unauthorized access! u:" + userName);
            return rv;
        }
    }
}
