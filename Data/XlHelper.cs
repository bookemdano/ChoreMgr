﻿using OfficeOpenXml;

namespace ChoreMgr.Data
{
    class XlHelper : IDisposable
    {
        private ExcelPackage _package;
        private ExcelWorkbook _book;

        public XlHelper()
        {
            _package = GetPackage();
            _book = _package.Workbook;
        }
        public string Name
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(Filename);
            }
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
        internal void RemoveRow(SheetEnum sheetEnum, int row)
        {
            GetSheet(sheetEnum).DeleteRow(row);
        }
    }
}
