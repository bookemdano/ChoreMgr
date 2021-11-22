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
        private readonly XlChoreMgrContext _context;

        public IndexModel(XlChoreMgrContext context)
        {
            _context = context;
        }

        public string ContextName
        {
            get
            {
                return _context.Name;
            }
        }
        public IList<Chore> ChoreList { get;set; }

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

            chore.LastDone = date;
            _context.SaveChore(chore);

            return RedirectToPage("./Index");
        }
    }
}
