using ChoreMgr.Data;
using ChoreMgr.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChoreMgr.Pages.Chores
{
    public class EditModel : PageModel
    {
        private readonly XlChoreMgrContext _context;

        public EditModel(XlChoreMgrContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Chore Chore { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Chore = _context.Chores.FirstOrDefault(m => m.Id == id);
            if (Chore == null)
            {
                return NotFound();
            }
            Chore.Journals = _context.Journals.Where(j => j.ChoreId == id).OrderByDescending(j => j.Updated).ToList();
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
            _context.SaveChore(Chore);
            /*
             * old school
            _context.Attach(Chore).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChoreExists(Chore.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            */
            return RedirectToPage("./Index");
        }
        public IActionResult OnGetDelete(int id)
        {
            var chore = _context.Chores.FirstOrDefault(c => c.Id == id);
            if (chore == null)
                return Page();
            _context.DeleteChore(chore);
            return RedirectToPage("./Index");
        }
        private bool ChoreExists(int id)
        {
            return _context.Chores.Any(e => e.Id == id);
        }
    }
}
