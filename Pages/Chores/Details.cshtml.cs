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
    public class DetailsModel : PageModel
    {
        private readonly ChoreMgr.Data.ChoreMgrContext _context;

        public DetailsModel(ChoreMgr.Data.ChoreMgrContext context)
        {
            _context = context;
        }

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
    }
}
