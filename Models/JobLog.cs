using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ChoreMgr.Models
{
    public class JobLog
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public DateTime Updated { get; set; }
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string JobId { get; set; }
        public string? JobName { get; set; }
        public string? Note { get; set; }

        internal static JobLog FromJournal(Journal journal, string jobId)
        {
            var rv = new JobLog();
            rv.JobId = jobId;
            rv.JobName = journal.ChoreName; 
            rv.Updated = journal.Updated;
            rv.Note = journal.Note;
            return rv;
        }

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
