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
        public IList<TransactionModel> TransactionList { get; set; }

        [BindProperty] 
        public string ExcludeList { get; set; }

        public void OnGetAsync(string? forWhom)
        {
            DanLogger.LogView(HttpContext, ContextName);
            TransactionList = _service.GetTransactionModels();
        }
    }
}
