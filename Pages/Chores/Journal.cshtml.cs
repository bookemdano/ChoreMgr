﻿using System;
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
        private readonly ChoreService _service;

        public JournalModel(ChoreService choreService)
        {
            _service = choreService;
        }

        public IList<JobLog> JobLogList{ get;set; }
        public List<Daily> DailyList { get; private set; }

        public void OnGetAsync()
        {
            JobLogList = _service.GetJobLog().OrderByDescending(j => j.Updated).ToList();
            var dailyList = new List<Daily>();
            var byDay = JobLogList.GroupBy(j => j.DoneDate);
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
