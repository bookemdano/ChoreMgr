using System.Security.Authentication.ExtendedProtection;

namespace ChoreMgr.Models
{
    public class Quote
    {
        public Quote()
        {
        }
        public Quote(string id)
        {
            Id = id;
        }
        public string Id { get; set; }
        public List<string> Lines { get; set; } = new List<string>();

        public override string ToString()
        {
            return string.Join(Environment.NewLine, Lines);
        }
    }
}
