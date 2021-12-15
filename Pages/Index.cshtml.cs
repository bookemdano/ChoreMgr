using ChoreMgr.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChoreMgr.Pages
{
    public class IndexModel : PageModel
    {
        
        public void OnGet()
        {
        }
        static public void GoogleSignin(object context = null)
        {
            DanLogger.Log("WooHoo!");
        }
    }
}
