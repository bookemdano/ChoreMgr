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
#if DEBUG
                return "DEV";
#else
                return "PROD";
#endif
            }
        }
        public IList<Job> JobList { get;set; }

        public string Summary
        {
            get
            {
                var today = DateTime.Today;
                var yesterday = today.AddDays(-1);
                var jobLogs = _service.GetJobLog();
                var todayCount = jobLogs.Count(j => j.DoneDate >= today);
                var yesterdayCount = jobLogs.Count(j => j.DoneDate >= yesterday) - todayCount;
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
