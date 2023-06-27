using ChoreMgr.Models;
using ChoreMgr.Utils;
using Finder2020Win;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PrivateStash;

namespace ChoreMgr.Pages.Shared
{
    public class UnauthorizedModel : BasePageModel
    {
        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPostAsync()
        {
            DanLogger.LogView(HttpContext, "Unauthorized check- " + DauthCode);
            var pinOnly = false;
            var leaveTwilioAlone = false;
            var pinMode = string.IsNullOrEmpty(_expectedCode);
            if (pinMode)
            {
                if (DauthCode == Mustache.DefaultPin)
                {
                    if (pinOnly)
                    {
                        TempAllow();
                        return RedirectToPage("/Chores/ChoreIndex");
                    }
                    else
                    {
                        Instruction = "Enter code sent to phone";
                        DauthCode = "";
                        if (leaveTwilioAlone)
                            _expectedCode = "123456";
                        else
                            _expectedCode = Sms.SendCodeToMe("WBHT");
                        return Page();

                    }
                }
            }
            else
            {
                if (DauthCode == _expectedCode)
                {
                    _expectedCode = "";
                    TempAllow();
                    return RedirectToPage("/Chores/ChoreIndex");
                }
                else
                {
                    _expectedCode = null;
                }

            }
            DauthCode = "";
            Instruction = "Enter PIN again " + UserName;
            return Page();
        }
        static string? _expectedCode;    // yeah- a bad place to store this
        public string Instruction { get; set; } = "Enter PIN";

        [BindProperty]
        public string DauthCode { get; set; }
    }
}

