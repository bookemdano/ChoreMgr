using ChoreMgr.Utils;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

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
        private string TempCode()
        {
            return DateTime.Now.ToString("yyyyMM") + PrivateStash.Mustache.HashMash;
        }

        protected void TempAllow()
        {
            SetToSession("TempUserHash", OpSec.GetHash(TempCode()));
        }
        protected bool IsAuthed()
        {
            var rv = false;
            var tempUserHash = GetFromSession("TempUserHash", "");
            if (!string.IsNullOrWhiteSpace(tempUserHash))
            {
                rv = (tempUserHash == OpSec.GetHash(TempCode()));
            }
            var localPinBypass = false;  // whether to greenlight everything from local network
            if (rv == false && localPinBypass)
            {
                var userName = UserName;
                rv = (userName.StartsWith("192.168.86") == true || userName.EndsWith("francis@gmail.com") == true || userName == "bookemdano@gmail.com" || userName == "::1");
            }
            if (rv == false)
            {
                var headers = HttpContext.Request.Headers.Select(h => $"{h.Key}={h.Value}");
                DanLogger.Error($"Unauthorized access! u:{UserName} h:{string.Join(",", headers)}", null);
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
