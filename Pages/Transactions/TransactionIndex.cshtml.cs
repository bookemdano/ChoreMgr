using ChoreMgr.Data;
using ChoreMgr.Models;
using ChoreMgr.Utils;
using Microsoft.AspNetCore.Mvc;

namespace ChoreMgr.Pages.Transactions
{
    // TODO Include user model
    // TODO Add size to jobs
    // TODONE allow to run in IIS not at root

    public class TransactionIndexModel : BasePageModel
    {
        private readonly ChoreJsonDb _service;

        public TransactionIndexModel(ChoreJsonDb choreService)
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
        public bool IsDebug
        {
            get
            {
                return System.Diagnostics.Debugger.IsAttached;
            }
        }
        public string Total
        {
            get
            {
                var total = TransactionList.Sum(s => s.Amount);
                return total.ToString("C0") + $"({TransactionList.Count()})";
            }
        }
        public TransactionModel[] TransactionList { get; set; }

        [BindProperty] 
        public string ExcludeList { get; set; }

        public IActionResult OnGetAsync(string? forWhom)
        {
            DanLogger.LogView(HttpContext, ContextName);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");
            TransactionList = _service.GetTransactionModels().OrderByDescending(t => t.Timestamp).ToArray();
            return Page();
        }
        public IActionResult OnGetDup(string id)
        {
            DanLogger.LogChange(HttpContext);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");
            var transaction = _service.GetTransaction(id);
            if (transaction == null)
                return NotFound();
            var createdTransaction = _service.CreateTransaction(Transaction.Duplicate(transaction), UserName);

            if (createdTransaction?.Id == null)
                return NotFound();
            //http://localhost:5165/Transactions/TransactionEdit?id=645bce6a-0d1b-427e-8142-f1f98dbdaea8
            var routeValues = new Dictionary<string, object>();
            routeValues["id"] = createdTransaction.Id;
            return RedirectToPage("./TransactionEdit", routeValues);
        }
        public IActionResult OnGetProdSync()
        {
            DanLogger.LogChange(HttpContext);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");
            _service.ProdSync();

            return RedirectToPage("./TransactionIndex");
        }
        public IActionResult OnGetBackup()
        {
            DanLogger.LogChange(HttpContext);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");
            _service.Backup();
            return RedirectToPage("./TransactionIndex");
        }
        public IActionResult OnGetDedup()
        {
            DanLogger.LogChange(HttpContext);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");
            _service.DedupTransactions();
            return RedirectToPage("./TransactionIndex");
        }

        // TODONE handle repeat/duplicate
    }
}
