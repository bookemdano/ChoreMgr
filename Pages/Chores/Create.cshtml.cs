using ChoreMgr.Data;
using ChoreMgr.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChoreMgr.Pages.Chores
{
    public class CreateModel : PageModel
    {
        private readonly XlChoreMgrContext _context;

        public CreateModel(XlChoreMgrContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Chore Chore { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.AddChore(Chore);
        
            return RedirectToPage("./Index");
        }
    }
}
