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
                var rv = HttpContext.Request.Cookies["userName"];
                if (string.IsNullOrEmpty(rv))
                    rv = HttpContext.Request.HttpContext.Connection.RemoteIpAddress?.ToString();
                return rv;
            }
        }
        protected bool IsAuthed()
        {
            var userName = UserName;
            var rv = (userName?.StartsWith("192.168.86") == true || userName?.EndsWith("francis@gmail.com") == true || userName == "bookemdano@gmail.com");
            if (rv == false)
            {
                var headers = HttpContext.Request.Headers.Select(h => $"{h.Key}={h.Value}");
                DanLogger.Error($"Unauthorized access! u:{userName} h:{string.Join(",", headers)}", null);
            }
            return rv;
        }
    }
}
