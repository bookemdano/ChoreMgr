using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChoreMgr.Models;

namespace ChoreMgr.Data
{
    public class XlChoreMgrContext : DbContext
    {
        static int _colName = 1;
        static int _colInterval = 2;
        static int _colLast = 6;
        static int _colNext = 7;

        public XlChoreMgrContext(DbContextOptions<XlChoreMgrContext> options)
            : base(options)
        {
        }

        public string Name
        {
            get
            {
                return Workbook.Name;
            }
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
        public bool DeleteChore(Chore chore)
        {
            var choreRow = FindChoreRow(chore.Id);
            if (choreRow <= 0)
                throw new Exception($"Chore {chore} not found");
            var foundName = Workbook.GetString(SheetEnum.Main, choreRow, _colName);
            if (foundName.CompareTo(chore.Name) != 0)
                throw new Exception($"Chore {chore} does not match row {choreRow}");

            AddToJournal(null, chore);
            Workbook.RemoveRow(SheetEnum.Main, choreRow);
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
            if (oldChoreRow <= 0)
                throw new Exception($"Chore {chore} not found");
            AddToJournal(chore, ReadChore(oldChoreRow));
            WriteChore(oldChoreRow, chore);
            Workbook.Save();
            return true;
        }

        void AddToJournal(Chore? chore, Chore? oldChore)
        {
            var colJournalUpdated = 1;
            var colJournalName = 2;
            var colJournalNote = 3;
            var firstBlank = Workbook.FirstBlank(SheetEnum.Journal);
            Workbook.Set(SheetEnum.Journal, firstBlank, colJournalUpdated, DateTime.Now);
            if (chore == null)
                Workbook.Set(SheetEnum.Journal, firstBlank, colJournalName, oldChore?.Name);
            else
                Workbook.Set(SheetEnum.Journal, firstBlank, colJournalName, chore?.Name);
            Workbook.Set(SheetEnum.Journal, firstBlank, colJournalNote, Chore.DeltaString(oldChore, chore));
        }

        private int FindChoreRow(int id)
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
                        var chore = ReadChore(row);
                        if (chore == null)
                            break;
                        if (chore.IntervalDays == null && chore.LastDone != null)
                            continue;
                        chores.Add(chore);
                    }
                    _chores = chores.OrderBy(c => c.NextDo).ToList();
                }
                return _chores;
            }
        }

        private Chore? ReadChore(int row)
        {
            var name = Workbook.GetString(SheetEnum.Main, row, _colName);
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var chore = new Chore(row, name);
            chore.IntervalDays = Workbook.GetInt(SheetEnum.Main, row, _colInterval);
            chore.LastDone = Workbook.GetDate(SheetEnum.Main, row, _colLast);
            return chore;
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
