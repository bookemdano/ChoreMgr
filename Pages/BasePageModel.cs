using ChoreMgr.Utils;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuGet.Protocol;
using System.Security.Cryptography;
using System.Text;
using Twilio.Rest.Api.V2010.Account.Usage.Record;

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
        private string TempCode()
        {
            return DateTime.Now.ToString("yyyyMM") + PrivateStash.TwilioStash.AuthToken;
        }
        private string TempHash()
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                return GetHash(sha256Hash, TempCode());
            }
        }

        // https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.hashalgorithm.computehash?view=net-7.0
        private static string GetHash(HashAlgorithm hashAlgorithm, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        private static bool VerifyHash(HashAlgorithm hashAlgorithm, string input, string hash)
        {
            // Hash the input.
            var hashOfInput = GetHash(hashAlgorithm, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return comparer.Compare(hashOfInput, hash) == 0;
        }
        protected void TempAllow()
        {
            // hash = month + code + nonce(later)
            // 
            SetToSession("CMUserName", TempHash());
        }
        protected bool IsAuthed()
        {
            var rv = false;
            var sess = GetFromSession("CMUserName", "");
            if (!string.IsNullOrWhiteSpace(sess))
            {
                using (SHA256 sha256Hash = SHA256.Create())
                {
                     rv = VerifyHash(sha256Hash, TempCode(), sess);
                }
            }
            var localPinBypass = true;
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
