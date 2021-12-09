using ChoreMgr.Data;
using ChoreMgr.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChoreMgr.Pages.Chores
{
    public class EditModel : PageModel
    {
        private readonly ChoreJsonDb _service;

        public EditModel(ChoreJsonDb choreService)
        {
            _service = choreService;
        }

        [BindProperty]
        public Job Job { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
                return NotFound();

            var job = _service.GetJob(id, includeLogs: true);
            if (job == null)
                return NotFound();
            Job = job;
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
            _service.UpdateJob(Job);
            return RedirectToPage("./Index");
        }
        public IActionResult OnGetDelete(string id)
        {
            _service.RemoveJob(id, true);
            return RedirectToPage("./Index");
        }
    }
}
