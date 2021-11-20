using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChoreMgr.Data;
using ChoreMgr.Models;
using OfficeOpenXml;

namespace ChoreMgr.Pages.Chores
{
    public class IndexModel : PageModel
    {
        private readonly XclChoreMgrContext _context;

        public IndexModel(XclChoreMgrContext context)
        {
            _context = context;
        }

        public IList<Chore> ChoreList { get;set; }
        public string Filename
        {
            get
            {
                return _context.Filename;
            }
        }
        public void OnGetAsync()
        {
            ChoreList = _context.Chores.ToList();
        }
        public IActionResult OnGetToday(int id)
        {
            return UpdateChore(id, DateTime.Today);
        }
        public IActionResult OnGetYesterday(int id)
        {
            return UpdateChore(id, DateTime.Today.AddDays(-1));
        }
        IActionResult UpdateChore(int id, DateTime date)
        {
            var chore = _context.Chores.FirstOrDefault(c => c.Id == id);
            if (chore == null)
                return Page();

            var oldLast = chore.LastDone;
            chore.LastDone = date;
            _context.SaveChore(chore, oldLast);

            return RedirectToPage("./Index");
        }
    }
}
