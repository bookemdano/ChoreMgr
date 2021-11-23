using System.ComponentModel.DataAnnotations;

namespace ChoreMgr.Models
{
    public class Journal
    {
        [DisplayFormat(DataFormatString = "{0:dd-MMM HH:mm:ss}")]
        public DateTime Updated { get; set; }
        public int? ChoreId { get; set; }
        public string? ChoreName { get; set; }
        public string? Note { get; set; }

        DateTime? _doneDate;
        [Display(Name = "Last")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MMM}")]
        public DateTime? DoneDate
        {
            get 
            {
                if (_doneDate == null)
                    _doneDate = CalcDoneDate();
                return _doneDate;
            }
        }
        DateTime? CalcDoneDate()
        {
            if (Note == null)
                return null;
            var part = FindBetween(Note, "LastDone", "|");
            if (part == null)
            {
                // try to parse the whole thing
                if (DateTime.TryParse(Note, out DateTime rawDate))
                    return rawDate;
                return null;
            }
            var newDone = FindBetween(part, "New:", " ");
            if (newDone == null)
                return null;

            if (DateTime.TryParse(newDone, out DateTime rv))
                return rv;
            return null;
        }
        string? FindBetween(string src, string start, string end)
        {
            var first = src.IndexOf(start);
            if (first == -1)
                return null;
            first += start.Length;
            var last = src.IndexOf(end, first);
            if (last == -1)
                return src.Substring(first);
            
            return src.Substring(first, last - first);

        }
    }
}
