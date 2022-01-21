using ChoreMgr.Data;
using ChoreMgr.Models;
using ChoreMgr.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChoreMgr.Pages.Chores
{
    public class TransactionCreateModel : BasePageModel
    {
        private readonly ChoreJsonDb _service;

        public TransactionCreateModel(ChoreJsonDb choreService)
        {
            _service = choreService;
        }

        public IActionResult OnGet()
        {
            DanLogger.LogView(HttpContext);
            Transaction = new Transaction();
            return Page();
        }

        [BindProperty]
        public Transaction Transaction { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            DanLogger.LogChange(HttpContext, Transaction);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _service.CreateTransaction(Transaction, UserName);
        
            return RedirectToPage("./TransactionIndex");
        }
    }
}
