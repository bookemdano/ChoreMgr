using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ChoreMgr.Models
{
    public class Job
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Name { get; set; }
        public int? IntervalDays { get; set; }
        [Display(Name = "Last")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MMM}")]
        public DateTime? LastDone { get; set; }

        internal static Job FromChore(Chore chore)
        {
            var rv = new Job();
            rv.Name = chore.Name;
            rv.IntervalDays = chore.IntervalDays;
            rv.LastDone = chore.LastDone;
            return rv;
        }

        [Display(Name = "Next")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM}")]
        public DateTime? NextDo
        {
            get
            {
                if (LastDone.HasValue && !IntervalDays.HasValue)
                    return null;    // it is not repeated and it has been done
                else if (!LastDone.HasValue)
                    return DateTime.Today.AddDays(.999);
                else
                    return LastDone.Value.Date.AddDays(IntervalDays.Value + .999);
            }
        }


        [Display(Name = "S")]
        public string Status
        {
            get
            {
                if (NextDo == null)
                    return "🤷‍";
                var delta = (NextDo - DateTime.Today).Value.TotalDays;
                if (delta > 1)
                    return "✅";
                else if (delta < 0)
                    return "❌";
                else
                    return "❕";
            }
        }
        // only set when viewing details
        public List<JobLog>? Logs { get; internal set; }

        public override string ToString()
        {
            return $"{Name}({Id})";
        }

        static internal string DeltaString(Job? oldJob, Job? newJob)
        {
            var deltas = new List<string>();
            if (oldJob == null)
                deltas.Add("New");
            if (newJob == null)
                deltas.Add("Delete");
            if (newJob?.Name != oldJob?.Name)
                deltas.Add($"Name {OldNew(oldJob?.Name, newJob?.Name)}");
            if (newJob?.IntervalDays != oldJob?.IntervalDays)
                deltas.Add($"Interval {OldNew(oldJob?.IntervalDays, newJob?.IntervalDays)}");
            if (newJob?.LastDone != oldJob?.LastDone)
                deltas.Add($"LastDone {OldNew(oldJob?.LastDone?.ToShortDateString(), newJob?.LastDone?.ToShortDateString())}");
            return String.Join("|", deltas);
        }
        static string OldNew(object? oldOne, object? newOne)
        {
            var parts = new List<string>();
            if (oldOne != null)
                parts.Add($"Old:{oldOne}");
            if (newOne != null)
                parts.Add($"New:{newOne}");
            return String.Join(" ", parts);
        }
    }
}
