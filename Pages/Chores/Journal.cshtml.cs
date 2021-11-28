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
    public class JournalModel : PageModel
    {
        private readonly XlChoreMgrContext _context;

        public JournalModel(XlChoreMgrContext context)
        {
            _context = context;
        }

        public IList<Journal> JournalList{ get;set; }
        public List<Daily> DailyList { get; private set; }

        public string Summary
        {
            get
            {
                var today = DateTime.Today;
                var yesterday = today.AddDays(-1);
                var todayCount = _context.Journals.Count(j => j.DoneDate >= today);
                var yesterdayCount = _context.Journals.Count(j => j.DoneDate >= yesterday) - todayCount;
                return $"Done Today: {todayCount} " +
                        $"Yesterday: {yesterdayCount}";
            }
        }
        public string Pending
        {
            get
            {
                var today = DateTime.Today;
                return $"Due today: {_context.Chores.Count(j => j.NextDo?.Date == today)} " +
                        $"Past due: {_context.Chores.Count(j => j.NextDo?.Date < today)}";
            }
        }
        public void OnGetAsync()
        {
            JournalList = _context.Journals.OrderByDescending(j => j.Updated).ToList();
            var dailyList = new List<Daily>();
            var byDay = JournalList.GroupBy(j => j.DoneDate);
            foreach (var day in byDay)
            {
                if (day.Key == null)
                    continue;
                dailyList.Add(new Daily(day.Key.Value, day.Count()));
            }
            DailyList = dailyList.OrderByDescending(d => d.DoneDate).ToList();
        }
    }
}
