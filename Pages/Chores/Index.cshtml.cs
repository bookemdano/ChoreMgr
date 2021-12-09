using ChoreMgr.Data;
using ChoreMgr.Models;
using ChoreMgr.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace ChoreMgr.Pages.Chores
{
    public class IndexModel : PageModel
    {
        private readonly ChoreJsonDb _service;

        public IndexModel(ChoreJsonDb choreService)
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
        public IList<JobModel> JobList { get;set; }

        public string Summary
        {
            get
            {
                var today = DateTime.Today;
                var yesterday = today.AddDays(-1);
                var recentJobLogs = _service.GetJobLogs().Where(j => j.DoneDate >= yesterday);
                var todayCount = recentJobLogs.Where(j => j.DoneDate >= today).DistinctBy(j => j.JobName).Count();
                var yesterdayCount = recentJobLogs.Where(j => j.DoneDate >= yesterday && j.DoneDate < today).DistinctBy(j => j.JobName).Count();
                return $"Done Today: {todayCount} " +
                        $"Yesterday: {yesterdayCount}";
            }
        }
        public string Pending
        {
            get
            {
                var today = DateTime.Today;
                return $"Due today: {JobList.Count(j => j.NextDo?.Date == today)} " +
                        $"Past due: {JobList.Count(j => j.NextDo?.Date < today)}";
            }
        }
        public void OnGetAsync()
        {
            DanLogger.Log($"{HttpContext.Request.Path} {ContextName}");

            JobList = _service.GetJobModels().OrderBy(j => j.NextDo).ToList();
        }

        public IActionResult OnGetToday(string id)
        {
            return UpdateChore(id, DateTime.Today);
        }
        public IActionResult OnGetProdSync()
        {
            DanLogger.Log("OnGetProdSync");
            _service.ProdSync();

            return RedirectToPage("./Index");
        }
        public IActionResult OnGetBackup()
        {
            DanLogger.Log("OnGetBackup");
            _service.Backup();
            return RedirectToPage("./Index");
        }
        public IActionResult OnGetYesterday(string id)
        {
            return UpdateChore(id, DateTime.Today.AddDays(-1));
        }
        IActionResult UpdateChore(string id, DateTime date)
        {
            var jobModel = _service.GetJobModel(id);
            if (jobModel == null)
                return Page();

            jobModel.LastDone = date;
            _service.UpdateJob(jobModel);

            return RedirectToPage("./Index");
        }
    }
}
