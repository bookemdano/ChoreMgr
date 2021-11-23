using OfficeOpenXml;
using ChoreMgr;

namespace ChoreMgr.Data
{
    class XlHelper : IDisposable
    {
        private string _filename;
        private ExcelPackage _package;
        private ExcelWorkbook _book;

        public XlHelper(string filename)
        {
            _filename = filename;
            _package = GetPackage();
            _book = _package.Workbook;
        }
        public string Name
        {
            get
            {
                return Path.GetFileNameWithoutExtension(_filename);
            }
        }

        private Dictionary<string, ExcelWorksheet> _sheets = new Dictionary<string, ExcelWorksheet>();
        ExcelWorksheet GetSheet(string sheetName)
        {
            if (!_sheets.ContainsKey(sheetName))
                _sheets[sheetName] = _book.Worksheets[sheetName];
            return _sheets[sheetName];
        }

        ExcelPackage GetPackage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            return new ExcelPackage(new FileInfo(_filename));
        }
        internal bool IsNull(string sheetName, int row, int col)
        {
            return GetSheet(sheetName).Cells[row, col]?.Value == null;
        }

        internal int FirstBlank(XlField col)
        {
            for (int iRow = 2; iRow < 1000; iRow++)
            {
                if (IsNull(col.SheetName, iRow, col.Col))
                    return iRow;
            }
            return -1;
        }
        internal int Max(XlField field)
        {
            var max = 0;
            for (int iRow = 2; iRow < 1000; iRow++)
            {
                var v = GetInt(new XlCell(field, iRow));
                if (v == null)
                    break;
                if (v > max)
                    max = v.Value;
            }
            return max;
        }
        internal int? FindRow(XlField field, int id)
        {
            for (int iRow = 2; iRow < 1000; iRow++)
            {
                var foundId = GetInt(new XlCell(field, iRow));
                if (foundId == null)
                    break;
                if (foundId == id)
                    return iRow;
            }
            return null;
        }

        public void Dispose()
        {
            _package?.Dispose();
        }

        internal void Save()
        {
            try
            {
                _package.Save();

            }
            catch (Exception ex)
            {
                DanLogger.Error("XlHelper.Save()", ex);
                throw;
            }
        }

        internal void Set(XlCell cell, object? val)
        {
            GetSheet(cell.SheetName).Cells[cell.Row, cell.Col].Value = val;
        }
        internal string GetString(XlCell cell)
        {
            var r = GetSheet(cell.SheetName).Cells[cell.Row, cell.Col];
            if (r?.Value == null)
                return null;
            return r.Value.ToString();
        }
        internal DateTime? GetDate(XlCell cell)
        {
            var r = GetSheet(cell.SheetName).Cells[cell.Row, cell.Col];
            if (r?.Value == null)
                return null;

            if (r.Value is DateTime)
                return Convert.ToDateTime(r.Value);
            
            var d = Convert.ToDouble(r.Value);
            return DateTime.Parse("1/1/1900").AddDays(d - 2);
        }

        internal int? GetInt(XlCell cell)
        {
            var r = GetSheet(cell.SheetName).Cells[cell.Row, cell.Col];
            if (r?.Value == null)
                return null;

            return Convert.ToInt32(r.Value);
        }
        internal void RemoveRow(XlCell cell)
        {
            GetSheet(cell.SheetName).DeleteRow(cell.Row);
        }
    }
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
}
