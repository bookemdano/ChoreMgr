using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChoreMgr.Models;

namespace ChoreMgr.Data
{
    class BaseSheet
    {
        static internal string SheetName { get; set; }
    }
    class XlField
    {
        public XlField(XlField other)
        {
            SheetName = other.SheetName;
            Col = other.Col;
        }

        public XlField(string sheetName, int col)
        {
            SheetName = sheetName;
            Col = col;
        }
        public string SheetName { get; set; }
        public int Col { get; set; }
    }
    class XlCell : XlField
    {
        public XlCell(XlField col, int? row) : base(col)
        {
            Row = row.Value;
        }

        public int Row { get; set; }

    }
    internal class JournalSheet : BaseSheet
    {
        static JournalSheet()
        {
            SheetName = "Journal";
            Update = new XlField(SheetName, 1);
            Id = new XlField(SheetName, 2);
            Name = new XlField(SheetName, 3);
            Note = new XlField(SheetName, 4);
        }
        static public XlField Update { get; }
        static public XlField Id { get; }
        static public XlField Name { get; }
        static public XlField Note { get; }
    }
    internal class MainSheet : BaseSheet
    {
        static MainSheet()
        {
            SheetName = "Main";
            Id = new XlField(SheetName, 1);
            Name = new XlField(SheetName, 2);
            Interval = new XlField(SheetName, 3);
            Last = new XlField(SheetName, 7);
            Next = new XlField(SheetName, 8);
        }
        static public XlField Id { get; }
        public static XlField Name { get; }
        public static XlField Interval { get; }
        public static XlField Last { get; }
        public static XlField Next { get; }
    }
    public class XlChoreMgrContext : DbContext
    {
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
                {

#if DEBUG
                    //var filename = @"c:\temp\Chores Copy.xlsm";
                    var filename = "/Users/danielfrancis/OneDrive/dan/Chores Copy.xlsm";
#else
                    var filename = @"F:\OneDrive\Dan\Chores 2021.xlsm";
#endif
                    _xlHelper = new XlHelper(filename);
                }
                return _xlHelper;
            }
        }

        public bool AddChore(Chore chore)
        {
            var choreRow = Workbook.FirstBlank(MainSheet.Id);
            WriteChore(chore);
            AddToJournal(chore, null);
            Workbook.Save();
            return true;
        }
        public bool DeleteChore(Chore chore)
        {
            DeleteChoreRow(chore, ReadChoreById(chore.Id));
            Workbook.Save();
            return true;
        }

        public bool DeleteChoreRow(Chore chore, Chore? foundChore)
        {
            if (foundChore == null || foundChore.Id <= 0)
                throw new Exception($"Chore {chore} not found");
            if (foundChore.Name.CompareTo(chore.Name) != 0)
                throw new Exception($"Chore {chore} does not match found {foundChore}");

            AddToJournal(null, chore);
            var row = FindChoreRow(chore.Id);
            if (row == null)
                return false;
            Workbook.RemoveRow(new XlCell(MainSheet.Id, row));
            return true;
        }

        void WriteChore(Chore chore)
        {
            int? row;
            if (chore.Id == 0)
            {
                row = Workbook.FirstBlank(MainSheet.Id);
                chore.Id = Math.Max(Workbook.Max(JournalSheet.Id), Workbook.Max(MainSheet.Id)) + 1;
            }
            else
                row = FindChoreRow(chore.Id);
            if (row == null)
                throw new Exception($"WriteChore({chore}) No row found for chore.");
            Workbook.Set(new XlCell(MainSheet.Id, row), chore.Id);
            Workbook.Set(new XlCell(MainSheet.Name, row), chore.Name);
            Workbook.Set(new XlCell(MainSheet.Interval, row), chore.IntervalDays);
            Workbook.Set(new XlCell(MainSheet.Last, row), chore.LastDone);
            Workbook.Set(new XlCell(MainSheet.Next, row), chore.NextDo);
        }
        public bool SaveChore(Chore chore)
        {
            var foundChore = ReadChoreById(chore.Id);
            AddToJournal(chore, foundChore);
            if (chore.IntervalDays == null && chore.LastDone != null)
                DeleteChoreRow(chore, foundChore);
            else
                WriteChore(chore);

            Workbook.Save();
            return true;
        }

        int? FindChoreRow(int choreId)
        {
            return Workbook.FindRow(MainSheet.Id, choreId);
        }
        void AddToJournal(Chore? chore, Chore? oldChore)
        {
            var firstBlank = Workbook.FirstBlank(JournalSheet.Id);
            Workbook.Set(new XlCell(JournalSheet.Update, firstBlank), DateTime.Now);
            Workbook.Set(new XlCell(JournalSheet.Id, firstBlank), chore?.Id ?? oldChore?.Id);
            Workbook.Set(new XlCell(JournalSheet.Name, firstBlank), chore?.Name ?? oldChore?.Name);
            Workbook.Set(new XlCell(JournalSheet.Note, firstBlank), Chore.DeltaString(oldChore, chore));
        }

        IList<Chore> _chores;
        public IList<Chore> Chores
        {
            get
            {
                if (_chores == null)
                    _chores = ReadAllChores();

                return _chores;
            }
        }

        private IList<Chore> ReadAllChores()
        {
            try
            {
                var chores = new List<Chore>();
                for (int row = 2; row < 1000; row++)
                {
                    var chore = ReadChoreByRow(row);
                    if (chore == null)
                        break;
                    if (chore.IntervalDays == null && chore.LastDone != null)
                        continue;
                    chores.Add(chore);
                }
                return chores.OrderBy(c => c.NextDo).ToList();
            }
            catch (Exception ex)
            {
                DanLogger.Error("XlChoreMgrContext.Chores", ex);
                throw;
            }
        }

        private Chore? ReadChoreByRow(int row)
        {
            var id = Workbook.GetInt(new XlCell(MainSheet.Id, row));
            if (id == null)
                return null;

            var chore = new Chore(id.Value);
            chore.Name = Workbook.GetString(new XlCell(MainSheet.Name, row));
            chore.IntervalDays = Workbook.GetInt(new XlCell(MainSheet.Interval, row));
            chore.LastDone = Workbook.GetDate(new XlCell(MainSheet.Last, row));
            return chore;
        }
        private Chore? ReadChoreById(int id)
        {
            var row = FindChoreRow(id);
            if (row == null)
                return null;
            return ReadChoreByRow(row.Value);
        }

        public override void Dispose()
        {
            _xlHelper?.Dispose();
            base.Dispose();
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
