using ChoreMgr.Data;
using ChoreMgr.Models;
using ChoreMgr.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChoreMgr.Pages.Chores
{
    public class ChoreEditModel : BasePageModel
    {
        private readonly ChoreJsonDb _service;

        public ChoreEditModel(ChoreJsonDb choreService)
        {
            _service = choreService;
        }

        [BindProperty]
        public JobModel JobModel { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            DanLogger.LogView(HttpContext);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");

            if (id == null)
                return NotFound();

            var jobModel = _service.GetJobModel(id, includeLogs: true);
            if (jobModel == null)
                return NotFound();
            JobModel = jobModel;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            DanLogger.LogChange(HttpContext, JobModel);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");

            if (!ModelState.IsValid)
            {
                return Page();
            }
            _service.UpdateJob(JobModel, UserName);
            return RedirectToPage("./ChoreIndex");
        }
        public IActionResult OnGetDelete(string id)
        {
            DanLogger.LogChange(HttpContext);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");

            _service.RemoveJob(id, UserName);
            return RedirectToPage("./ChoreIndex");
        }
    }
}
