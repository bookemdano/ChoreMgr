using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChoreMgr.Data;
using ChoreMgr.Models;

namespace ChoreMgr.Pages.Chores
{
    public class EditModel : PageModel
    {
        private readonly XclChoreMgrContext _context;

        public EditModel(XclChoreMgrContext context)
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
            _context.SaveChore(Chore, null);
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

        private bool ChoreExists(int id)
        {
            return _context.Chores.Any(e => e.Id == id);
        }
    }
}
