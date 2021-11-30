using ChoreMgr.Data;
using ChoreMgr.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChoreMgr.Pages.Chores
{
    public class IndexModel : PageModel
    {
        private readonly ChoreService _service;

        public IndexModel(ChoreService choreService)
        {
            _service = choreService;
        }

        public string ContextName
        {
            get
            {
                if (_service.UseDevTables)
                    return "DEV";
                else
                    return "PROD";
            }
        }
        public bool IsDebug
        {
            get
            {
                return _service.UseDevTables;
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
                var todayCount = recentJobLogs.Count(j => j.DoneDate >= today);
                var yesterdayCount = recentJobLogs.Count() - todayCount;
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
            var prodService = _service.CloneProd();
            var prodJobs = prodService.GetJobs();
            foreach (var job in prodJobs)
                _service.CreateJob(job, false);
            var prodJobLogs = prodService.GetJobLogs();
            foreach (var jobLog in prodJobLogs)
                _service.CreateJobLog(jobLog);

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
            _service.UpdateJob(id, job);

            return RedirectToPage("./Index");
        }
    }
}
