using ChoreMgr.Models;
using ChoreMgr.Utils;
using Finder2020Win;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChoreMgr.Pages.Shared
{
    public class UnauthorizedModel : BasePageModel
    {
        public void OnGet()
        {
            if (string.IsNullOrEmpty(Pin))
                Instruction = "Enter PIN";
            else
                Instruction = "Enter code sent to phone";
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var pinOnly = false;
            if (string.IsNullOrEmpty(Pin) && DauthCode == "1116")
            {
                if (pinOnly)
                {
                    TempAllow();
                    return RedirectToPage("/Chores/ChoreIndex");
                }
                Pin = DauthCode;
                Instruction = "Enter code sent to phone";
                DauthCode = "";
                _expectedCode = Sms.SendCodeToMe("WBHT");
                return Page();
            }
            else if (DauthCode == _expectedCode)
            {
                _expectedCode = "";
                TempAllow();
                return RedirectToPage("/Chores/ChoreIndex");
            }

            return Page();
        }
        static string _expectedCode;    // yeah- a bad place to store this
        public string Instruction { get; set; }

        [BindProperty]
        public string DauthCode { get; set; }
        [BindProperty]
        public string Pin { get; set; }
    }
}

