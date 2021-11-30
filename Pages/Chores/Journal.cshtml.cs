using ChoreMgr.Data;
using ChoreMgr.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChoreMgr.Pages.Chores
{
    public class JournalModel : PageModel
    {
        private readonly ChoreService _service;

        public JournalModel(ChoreService choreService)
        {
            _service = choreService;
        }

        public IList<JobLog> JobLogList{ get;set; }
        public List<Daily> DailyList { get; private set; }

        public void OnGetAsync()
        {
            JobLogList = _service.GetJobLogs().OrderByDescending(j => j.Updated).ToList();
            var dailyList = new List<Daily>();
            var byDay = JobLogList.GroupBy(j => j.DoneDate);
            foreach (var day in byDay)
            {
                if (day.Key == null)
                    continue;
                dailyList.Add(new Daily(day.Key.Value, day.DistinctBy(d => d.JobName).Count()));
            }
            DailyList = dailyList.OrderByDescending(d => d.DoneDate).ToList();
        }
        public void OnGetByDay(DateTime? date)
        {
            JobLogList = _service.GetJobLogs().Where(j => j.DoneDate == date).OrderBy(j => j.JobName).ToList();
        }
    }
}
