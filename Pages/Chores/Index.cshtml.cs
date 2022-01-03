using ChoreMgr.Data;
using ChoreMgr.Models;
using ChoreMgr.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace ChoreMgr.Pages.Chores
{
    // TODO Include user model
    // TODO Add size to jobs
    // TODONE allow to run in IIS not at root

    public class IndexModel : BasePageModel
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
        public IList<JobModel> JobList { get; set; }

        [BindProperty] 
        public string ExcludeList { get; set; }

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
        string GetFromSession(string key, string def)
        {
            if (!HttpContext.Session.Keys.Contains(key))
                return def;

            var rv = HttpContext.Session.GetString(key);
            if (string.IsNullOrEmpty(rv))
                return def;
            return rv;
        }
        void SetToSession(string key, string? val)
        {
            if (string.IsNullOrWhiteSpace(val))
                HttpContext.Session.Remove(key);
            else
                HttpContext.Session.SetString(key, val);
        }
        public void OnGetAsync(string? forWhom)
        {
            DanLogger.LogView(HttpContext, ContextName);
            ExcludeList = GetFromSession("ExcludeList", "");
            var excludeList = ExcludeList.ToCharArray();
            JobList = _service.GetJobModels().Where(j => !j.IsExcluded(excludeList)).OrderBy(j => j.NextDo).ToList();
        }
        public IActionResult OnPostAsync(object o)
        {
            DanLogger.LogView(HttpContext, "Change filter to-" + ExcludeList);
            SetToSession("ExcludeList", ExcludeList);

            return RedirectToPage("./Index");
        }

        public IActionResult OnGetProdSync()
        {
            DanLogger.LogChange(HttpContext);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");
            _service.ProdSync();

            return RedirectToPage("./Index");
        }
        public IActionResult OnGetBackup()
        {
            DanLogger.LogChange(HttpContext);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");
            _service.Backup();
            return RedirectToPage("./Index");
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
            DanLogger.LogChange(HttpContext, date);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");
            var jobModel = _service.GetJobModel(id);
            if (jobModel == null)
                return Page();

            jobModel.LastDone = date;
            _service.UpdateJob(jobModel, UserName);

            return RedirectToPage("./Index");
        }
    }
}
