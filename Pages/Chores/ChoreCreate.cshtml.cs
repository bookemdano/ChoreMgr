﻿using ChoreMgr.Data;
using ChoreMgr.Models;
using ChoreMgr.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChoreMgr.Pages.Chores
{
    public class ChoreCreateModel : BasePageModel
    {
        private readonly ChoreJsonDb _service;

        public ChoreCreateModel(ChoreJsonDb choreService)
        {
            _service = choreService;
        }

        public IActionResult OnGet()
        {
            DanLogger.LogView(HttpContext);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");

            return Page();
        }

        [BindProperty]
        public Job Job { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            DanLogger.LogChange(HttpContext, Job);
            if (!IsAuthed())
                return RedirectToPage("/Shared/Unauthorized");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _service.CreateJob(Job, UserName);
        
            return RedirectToPage("./ChoreIndex");
        }
    }
}
