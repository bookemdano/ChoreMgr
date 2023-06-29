using ChoreMgr.Data;
using ChoreMgr.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChoreMgr.Pages.Mornings
{
    public class MorkModel : BasePageModel
    {
        private readonly ChoreJsonDb _service;

        public MorkModel(ChoreJsonDb choreService)
        {
            _service = choreService;
        }
        public string ContextName
        {
            get
            {
                return _service.ToString();
            }
        }
        public IActionResult OnGet()
        {
            DanLogger.LogView(HttpContext, ContextName);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");

            if (_service.GetRandQuote() == null)
                _service.PullQuotes();
            Daily = _service.GetRandQuote()?.ToString();
            return Page();
        }
        public IActionResult OnGetRepull()
        {
            DanLogger.LogChange(HttpContext);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");
            _service.PullQuotes();
            Daily = _service.GetRandQuote()?.ToString();
            return RedirectToPage("./Mork");
        }

        [BindProperty]
        public string Daily { get; set; }
    }
}
