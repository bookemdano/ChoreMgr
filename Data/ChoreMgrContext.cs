using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChoreMgr.Models;
using OfficeOpenXml;

namespace ChoreMgr.Data
{
    public class XlChoreMgrContext : DbContext
    {
        static int _colName = 1;
        static int _colInterval = 2;
        static int _colLast = 6;
        static int _colNext = 7;

        static int _colJournalDate = 1;
        static int _colJournalName = 2;
        static int _colJournalLast = 3;
        static int _colJournalUpdated = 4;

        public XlChoreMgrContext(DbContextOptions<XlChoreMgrContext> options)
            : base(options)
        {
        }

        private XlHelper _xlHelper;
        XlHelper Workbook
        {
            get
            {
                if (_xlHelper == null)
                    _xlHelper = new XlHelper();
                return _xlHelper;
            }
        }

        public bool AddChore(Chore chore)
        {
            var choreRow = Workbook.FirstBlank(SheetEnum.Main);
            WriteChore(choreRow, chore);
            AddToJournal(chore, null);
            Workbook.Save();
            return true;
        }
        void WriteChore(int row, Chore chore)
        {
            Workbook.Set(SheetEnum.Main, row, _colName, chore.Name);
            Workbook.Set(SheetEnum.Main, row, _colInterval, chore.IntervalDays);
            Workbook.Set(SheetEnum.Main, row, _colLast, chore.LastDone);
            Workbook.Set(SheetEnum.Main, row, _colNext, chore.NextDo);
        }
        public bool SaveChore(Chore chore, DateTime? oldLast)
        {
            var oldChoreRow = FindChoreRow(chore.Id);
            if (!oldChoreRow.HasValue)
                return false;
            WriteChore(oldChoreRow.Value, chore);
            AddToJournal(chore, oldLast);
            Workbook.Save();
            return true;
        }

        void AddToJournal(Chore chore, DateTime? oldLast)
        {
            var firstBlank = Workbook.FirstBlank(SheetEnum.Journal);
            Workbook.Set(SheetEnum.Journal, firstBlank, _colJournalDate, chore.LastDone);
            Workbook.Set(SheetEnum.Journal, firstBlank, _colJournalName, chore.Name);
            Workbook.Set(SheetEnum.Journal, firstBlank, _colJournalLast, oldLast);
            Workbook.Set(SheetEnum.Journal, firstBlank, _colJournalUpdated, DateTime.Now);
        }

        private int? FindChoreRow(int id)
        {
            return id;
            /*
            for (int iRow = 2; iRow < 1000; iRow++)
            {
                var name = Workbook.GetString(SheetEnum.Main, iRow, _colName);
                if (string.IsNullOrWhiteSpace(name))
                    break;

                if (name.GetHashCode() == id)
                    return iRow;
            }
            return null;
            */
        }


        IList<Chore> _chores;
        public IList<Chore> Chores
        {
            get
            {
                if (_chores == null)
                {
                    var chores = new List<Chore>();
                    for (int row = 2; row < 1000; row++)
                    {
                        var name = Workbook.GetString(SheetEnum.Main, row, _colName);
                        if (string.IsNullOrWhiteSpace(name))
                            break;

                        var chore = new Chore(row, name);
                        chore.IntervalDays = Workbook.GetInt(SheetEnum.Main, row, _colInterval);
                        chore.LastDone = Workbook.GetDate(SheetEnum.Main, row, _colLast);
                        if (chore.IntervalDays == null && chore.LastDone != null)
                            continue;
                        chores.Add(chore);
                    }
                    _chores = chores.OrderBy(c => c.NextDo).ToList();
                }
                return _chores;
            }
        }

        public override void Dispose()
        {
            _xlHelper?.Dispose();
            base.Dispose();
        }
    }
    public enum SheetEnum
    {
        NA,
        Main,
        Journal

    }
    class XlHelper : IDisposable
    {
        private ExcelPackage _package;
        private ExcelWorkbook _book;

        public XlHelper()
        {
            _package = GetPackage();
            _book = _package.Workbook;
        }
        private Dictionary<SheetEnum, ExcelWorksheet> _sheets = new Dictionary<SheetEnum, ExcelWorksheet>();
        ExcelWorksheet GetSheet(SheetEnum sheetEnum)
        {
            if (!_sheets.ContainsKey(sheetEnum))
                _sheets[sheetEnum] = _book.Worksheets[sheetEnum.ToString()];
            return _sheets[sheetEnum];
        }

#if DEBUG
        public string Filename { get; } = @"c:\temp\Chores Copy.xlsm";
#else
        public string Filename { get; } = @"F:\OneDrive\Dan\Chores 2021.xlsm";
#endif
        ExcelPackage GetPackage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            return new ExcelPackage(new FileInfo(Filename));
        }
        internal bool IsNull(SheetEnum sheetEnum, int row, int col)
        {
            return GetSheet(sheetEnum).Cells[row, col]?.Value == null;
        }

        internal int FirstBlank(SheetEnum sheetEnum)
        {
            for (int iRow = 2; iRow < 1000; iRow++)
            {
                if (IsNull(sheetEnum, iRow, 1))
                    return iRow;
            }
            return -1;
        }

        public void Dispose()
        {
            _package?.Dispose();
        }

        internal void Save()
        {
            _package.Save();
        }

        internal void Set(SheetEnum sheetEnum, int row, int col, object? val)
        {
            GetSheet(sheetEnum).Cells[row, col].Value = val;
        }
        internal string GetString(SheetEnum sheetEnum, int row, int col)
        {
            var r = GetSheet(sheetEnum).Cells[row, col];
            if (r?.Value == null)
                return null;
            return (string)r.Value;
        }
        internal DateTime? GetDate(SheetEnum sheetEnum, int row, int col)
        {
            var r = GetSheet(sheetEnum).Cells[row, col];
            if (r?.Value == null)
                return null;

            return Convert.ToDateTime(r.Value);
        }

        internal int? GetInt(SheetEnum sheetEnum, int row, int col)
        {
            var r = GetSheet(sheetEnum).Cells[row, col];
            if (r?.Value == null)
                return null;

            return Convert.ToInt32(r.Value);
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
