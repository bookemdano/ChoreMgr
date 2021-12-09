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
        public IList<Job> JobList { get;set; }

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

            JobList = _service.GetJobs().OrderBy(j => j.NextDo).ToList();
        }

        public IActionResult OnGetToday(string id)
        {
            return UpdateChore(id, DateTime.Today);
        }
        public IActionResult OnGetProdSync()
        {
            DanLogger.Log("OnGetProdSync");
            _service.GetJobs().ForEach(j => _service.RemoveJob(j, false));
            _service.GetJobLogs().ForEach(j => _service.RemoveJobLog(j.Id));
            // copy prod context to dev context for testing
            var prodService = _service.CloneFromProd();
            var prodJobs = prodService.GetJobs();
            foreach (var job in prodJobs)
                _service.CreateJob(job, false);
            var prodJobLogs = prodService.GetJobLogs();
            foreach (var jobLog in prodJobLogs)
                _service.CreateJobLog(jobLog);

            return RedirectToPage("./Index");
        }
        public IActionResult OnGetBackup()
        {
            DanLogger.Log("OnGetBackup");
            //_service.Backup();
            return RedirectToPage("./Index");
        }
        public IActionResult OnGetRestore()
        {
            DanLogger.Log("OnGetBackup");
            _service.Restore();
            return RedirectToPage("./Index");
        }
        public IActionResult OnGetYesterday(string id)
        {
            return UpdateChore(id, DateTime.Today.AddDays(-1));
        }
        IActionResult UpdateChore(string id, DateTime date)
        {
            var job = _service.GetJobs().FirstOrDefault(c => c.Id == id);
            if (job == null)
                return Page();

            job.LastDone = date;
            _service.UpdateJob(job);

            return RedirectToPage("./Index");
        }
    }
}
