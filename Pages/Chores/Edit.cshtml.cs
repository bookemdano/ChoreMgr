using ChoreMgr.Data;
using ChoreMgr.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChoreMgr.Pages.Chores
{
    public class EditModel : PageModel
    {
        private readonly ChoreService _service;

        public EditModel(ChoreService choreService)
        {
            _service = choreService;
        }

        [BindProperty]
        public Job Job { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
                return NotFound();

            Job = _service.GetJob(id);
            if (Job == null)
                return NotFound();
            Job.Logs = _service.GetJobLogs().Where(j => j.JobId == id).OrderByDescending(j => j.Updated).ToList();
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            _service.UpdateJob(Job.Id, Job);
            return RedirectToPage("./Index");
        }
        public IActionResult OnGetDelete(string id)
        {
            _service.RemoveJob(id);
            return RedirectToPage("./Index");
        }
    }
}
