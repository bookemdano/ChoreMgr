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
    public class IndexModel : PageModel
    {
        private readonly ChoreMgr.Data.ChoreMgrContext _context;

        public IndexModel(ChoreMgr.Data.ChoreMgrContext context)
        {
            _context = context;
        }

        public IList<Chore> ChoreList { get;set; }

        public async Task OnGetAsync()
        {
            ChoreList = await _context.Chores.ToListAsync();
            if (!ChoreList.Any())
            {
                for (int i = 0; i < 10; i++)
                {
                    ChoreList.Add(Chore.Fake());
                }
            }
        }
    }
}
