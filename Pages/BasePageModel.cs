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
                SetToSession("CMUserName", rv);
                return rv;
            }
        }
        protected bool IsAuthed()
        {
            var userName = UserName;
            var rv = (userName?.StartsWith("192.168.86") == true || userName?.EndsWith("francis@gmail.com") == true || userName == "bookemdano@gmail.com" || userName == "::1");
            if (rv == false)
            {
                var headers = HttpContext.Request.Headers.Select(h => $"{h.Key}={h.Value}");
                DanLogger.Error($"Unauthorized access! u:{userName} h:{string.Join(",", headers)}", null);
            }
            return rv;
        }

        protected string GetFromSession(string key, string def)
        {
            if (!HttpContext.Session.Keys.Contains(key))
                return def;

            var rv = HttpContext.Session.GetString(key);
            if (string.IsNullOrEmpty(rv))
                return def;
            return rv;
        }
        protected void SetToSession(string key, string? val)
        {
            if (string.IsNullOrWhiteSpace(val))
                HttpContext.Session.Remove(key);
            else
                HttpContext.Session.SetString(key, val);
        }
    }
}
