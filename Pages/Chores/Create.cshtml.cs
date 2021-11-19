using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ChoreMgr.Data;
using ChoreMgr.Models;

namespace ChoreMgr.Pages.Chores
{
    public class CreateModel : PageModel
    {
        private readonly ChoreMgr.Data.ChoreMgrContext _context;

        public CreateModel(ChoreMgr.Data.ChoreMgrContext context)
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

            _context.Chores.Add(Chore);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
