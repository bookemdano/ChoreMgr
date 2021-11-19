using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChoreMgr.Data;
using ChoreMgr.Models;

namespace ChoreMgr.Pages.Chores
{
    public class DeleteModel : PageModel
    {
        private readonly ChoreMgr.Data.ChoreMgrContext _context;

        public DeleteModel(ChoreMgr.Data.ChoreMgrContext context)
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

            Chore = await _context.Chores.FirstOrDefaultAsync(m => m.Id == id);

            if (Chore == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Chore = await _context.Chores.FindAsync(id);

            if (Chore != null)
            {
                _context.Chores.Remove(Chore);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
