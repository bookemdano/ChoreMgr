using System.Security.Cryptography;
using System.Text;

namespace ChoreMgr.Utils
{
    static public class OpSec
    {
        // https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.hashalgorithm.computehash?view=net-7.0
        public static string GetHash(string input)
        {
            using (var hashAlgorithm = SHA256.Create())
            {
                var data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
                return string.Join("", data.Select(b => b.ToString("x2")));
            }
        }
    }
}
