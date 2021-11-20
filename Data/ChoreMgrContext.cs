using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChoreMgr.Models;
using OfficeOpenXml;

namespace ChoreMgr.Data
{
    public class XclChoreMgrContext : DbContext
    {
        static int _colName = 1;
        static int _colInterval = 2;
        static int _colLast = 6;
        static int _colNext = 7;

        static int _colJournalDate = 1;
        static int _colJournalName = 2;
        static int _colJournalLast = 3;
        static int _colJournalUpdated = 4;

        public XclChoreMgrContext(DbContextOptions<XclChoreMgrContext> options)
            : base(options)
        {
        }

        public bool SaveChore(Chore chore, DateTime? oldLast)
        {
            using (var package = GetPackage())
            {
                var book = package.Workbook;
                var sheet = book.Worksheets["Main"];
                var oldChoreRow = FindChore(sheet, chore.Id);
                if (oldChoreRow == null)
                    return false;
                sheet.Cells[oldChoreRow.Value, _colName].Value = chore.Name;
                sheet.Cells[oldChoreRow.Value, _colInterval].Value = chore.IntervalDays;
                sheet.Cells[oldChoreRow.Value, _colLast].Value = chore.LastDone;
                if (chore.LastDone != null && chore.IntervalDays != null)
                {
                    sheet.Cells[oldChoreRow.Value, _colNext].Value = chore.LastDone.Value.AddDays(chore.IntervalDays.Value + .999);
                }
                AddToJournal(book, chore, oldLast);
                package.Save();
            }
            return true;
        }

        void AddToJournal(ExcelWorkbook book, Chore chore, DateTime? oldLast)
        {
            var sheet = book.Worksheets["Journal"];
            var firstBlank = FindLast(sheet) + 1;
            sheet.Cells[firstBlank, _colJournalDate].Value = chore.LastDone;
            sheet.Cells[firstBlank, _colJournalName].Value = chore.Name;
            sheet.Cells[firstBlank, _colJournalLast].Value = oldLast;
            sheet.Cells[firstBlank, _colJournalUpdated].Value = DateTime.Now;
        }

        //public string Filename { get; } = @"c:\temp\Chores Copy.xlsm";
        public string Filename { get; } = @"F:\OneDrive\Dan\Chores 2021.xlsm";
        ExcelPackage GetPackage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            return new ExcelPackage(new FileInfo(Filename));
        }
        private static int? FindChore(ExcelWorksheet sheet, int id)
        {
            for (int iRow = 2; iRow < 1000; iRow++)
            {
                var name = GetString(sheet.Cells[iRow, _colName]);
                if (string.IsNullOrWhiteSpace(name))
                    break;

                if (name.GetHashCode() == id)
                    return iRow;
            }
            return null;
        }
        private static int FindLast(ExcelWorksheet sheet)
        {
            for (int iRow = 2; iRow < 1000; iRow++)
            {
                if (sheet.Cells[iRow, 1].Value == null)
                    return iRow - 1;
            }
            return -1;
        }

        IList<Chore> _chores;
        public IList<Chore> Chores
        {
            get
            {
                if (_chores == null)
                {
                    var chores = new List<Chore>();
                    using (var package = GetPackage())
                    {
                        var book = package.Workbook;
                        var sheet = book.Worksheets["Main"];
                        for (int iRow = 2; iRow < 1000; iRow++)
                        {
                            var name = GetString(sheet.Cells[iRow, _colName]);
                            if (string.IsNullOrWhiteSpace(name))
                                break;

                            var chore = new Chore(name);
                            chore.IntervalDays = GetInt(sheet.Cells[iRow, _colInterval]);
                            chore.LastDone = GetDate(sheet.Cells[iRow, _colLast]);
                            chore.NextDo = GetDate(sheet.Cells[iRow, _colNext]);
                            chores.Add(chore);
                        }

                    }
                    _chores = chores.OrderBy(c => c.NextDo).ToList();
                }
                return _chores;
            }
        }
        private DateTime? GetDate(ExcelRange r)
        {
            if (r?.Value == null)
                return null;

            return Convert.ToDateTime(r.Value);
        }

        private int? GetInt(ExcelRange r)
        {
            if (r?.Value == null)
                return null;

            return Convert.ToInt32(r.Value);
        }

        static string GetString(ExcelRange r)
        {
            if (r?.Value == null)
                return null;
            return (string)r.Value;
        }
    }

    public class ChoreMgrContext : DbContext
    {
        public ChoreMgrContext(DbContextOptions<ChoreMgrContext> options)
            : base(options)
        {
        }

        public DbSet<ChoreMgr.Models.Chore> Chores { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chore>().ToTable("Chore");
        }
    }
}
