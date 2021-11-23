using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChoreMgr.Models;
using System.Runtime.InteropServices;

namespace ChoreMgr.Data
{
    internal class JournalSheet : BaseSheet
    {
        static JournalSheet()
        {
            SheetName = "Journal";
            Updated = new XlField(SheetName, 1);
            ChoreId = new XlField(SheetName, 2);
            ChoreName = new XlField(SheetName, 3);
            Note = new XlField(SheetName, 4);
        }
        static public XlField Updated { get; }
        static public XlField ChoreId { get; }
        static public XlField ChoreName { get; }
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
        private XlHelper _workbook;
        XlHelper Workbook
        {
            get
            {
                if (_workbook == null)
                {
                    string filename;
#if DEBUG
                    if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        filename = @"c:\temp\Chores Copy.xlsm";
                    else
                        filename = "/Users/danielfrancis/OneDrive/dan/Chores Copy.xlsm";
#else
                    filename = @"F:\OneDrive\Dan\Chores 2021.xlsm";
#endif
                    _workbook = new XlHelper(filename);
                }
                return _workbook;
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
                chore.Id = Math.Max(Workbook.Max(JournalSheet.ChoreId), Workbook.Max(MainSheet.Id)) + 1;
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
            var firstBlank = Workbook.FirstBlank(JournalSheet.ChoreId);
            Workbook.Set(new XlCell(JournalSheet.Updated, firstBlank), DateTime.Now);
            Workbook.Set(new XlCell(JournalSheet.ChoreId, firstBlank), chore?.Id ?? oldChore?.Id);
            Workbook.Set(new XlCell(JournalSheet.ChoreName, firstBlank), chore?.Name ?? oldChore?.Name);
            Workbook.Set(new XlCell(JournalSheet.Note, firstBlank), Chore.DeltaString(oldChore, chore));
        }

        IList<Journal> _journals;
        public IList<Journal> Journals
        {
            get
            {
                if (_journals == null)
                    _journals = ReadAllJournals();

                return _journals;
            }
        }
        private IList<Journal> ReadAllJournals()
        {
            try
            {
                var rv = new List<Journal>();
                for (int row = 2; ; row++)
                {
                    var journal = ReadJournalByRow(row);
                    if (journal == null)
                        break;
                    rv.Add(journal);
                }
                return rv.OrderBy(c => c.Updated).ToList();
            }
            catch (Exception ex)
            {
                DanLogger.Error("XlChoreMgrContext.ReadAllJournals", ex);
                throw;
            }
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
                DanLogger.Error("XlChoreMgrContext.ReadAllChores", ex);
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
        private Journal? ReadJournalByRow(int row)
        {
            var updated = Workbook.GetDate(new XlCell(JournalSheet.Updated, row));
            if (updated == null)
                return null;

            var rv = new Journal();
            rv.Updated = updated.Value;
            rv.ChoreId = Workbook.GetInt(new XlCell(JournalSheet.ChoreId, row));
            rv.ChoreName = Workbook.GetString(new XlCell(JournalSheet.ChoreName, row));
            rv.Note = Workbook.GetString(new XlCell(JournalSheet.Note, row));
            return rv;
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
            _workbook?.Dispose();
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
