using ChoreMgr.Data;
using ChoreMgr.Models;
using ChoreMgr.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChoreMgr.Pages.Chores
{
    public class TransactionEditModel : BasePageModel
    {
        private readonly ChoreJsonDb _service;

        public TransactionEditModel(ChoreJsonDb choreService)
        {
            _service = choreService;
        }

        [BindProperty]
        public Transaction Transaction { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            DanLogger.LogView(HttpContext);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");

            if (id == null)
                return NotFound();

            var transaction = _service.GetTransaction(id);
            if (transaction == null)
                return NotFound();
            Transaction = transaction;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            DanLogger.LogChange(HttpContext, Transaction);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");

            if (!ModelState.IsValid)
            {
                return Page();
            }
            _service.UpdateTransaction(Transaction, UserName);
            return RedirectToPage("./TransactionIndex");
        }
        public IActionResult OnGetDelete(string id)
        {
            DanLogger.LogChange(HttpContext);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");
            _service.RemoveTransaction(id, UserName);
            return RedirectToPage("./TransactionIndex");
        }
    }
}
