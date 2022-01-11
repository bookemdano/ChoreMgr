using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ChoreMgr.Models
{
    public class Job
    {
        public Job()
        {

        }
        public Job(Job? job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));
            Id = job.Id;
            Name = job.Name;
            IntervalDays = job.IntervalDays;
            LastDone = job.LastDone;
            Category = job.Category;
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Name { get; set; }
        [Display(Name = "Days")]
        public int? IntervalDays { get; set; }

        [Display(Name = "Last")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:M/d/yy}")]
        public DateTime? LastDone { get; set; }

        [Display(Name = "Cat")]
        public string? Category { get; set; }

        internal void Update(Job other)
        {
            Name = other.Name;
            IntervalDays = other.IntervalDays;
            LastDone = other.LastDone;
            Category = other.Category;
        }

        public override string ToString()
        {
            var rv = $"{Name}({Id})";
            if (Category != null)
                rv += $"[{Category}]";
            return rv;
        }

        internal static string CsvHeader()
        {
            return $"Id,Name,Category,IntervalDays,LastDone,NextDo";
        }
        internal string ToCsv()
        {
            return $"{Id},{Name},{Category},{IntervalDays},{LastDone?.ToShortDateString()},{JobModel.CalcNextDo(this)?.ToShortDateString()}";
        }

    }
    public class JobModel : Job
    {
        public JobModel()
        {
        }

        public JobModel(Job? job) : base(job)
        {
        }

        [Display(Name = "Next")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:M/d/yy}")]
        public DateTime? NextDo
        {
            get
            {
                return CalcNextDo(this);
            }
        }

        internal bool IsExcluded(char[] excludeList)
        {
            if (string.IsNullOrWhiteSpace(Category))
                return false;
            foreach (var exclude in excludeList)
                if (Category.Contains(exclude))
                    return true;
            return false;
        }
        internal bool ChildOnly()
        {
            if (Category == null)
                return false;
            return Category.Contains('C', StringComparison.OrdinalIgnoreCase);
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

        public string WebStyle
        {
            get
            {
                var rv = "text-align:center;";
                if (IntervalDays == null)
                    rv = rv + "background-color:lightblue;";
                else if (IntervalDays == 1)
                    rv = rv + "background-color:lightgreen;";
                else if (IntervalDays > 14)
                    rv = rv + "background-color:peachpuff;";
                return rv;
            }
        }
        // only set when viewing details
        public List<JobLog>? Logs { get; internal set; }

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
            var oldDone = oldJob?.LastDone?.Date;
            var newDone = newJob?.LastDone?.Date;
            if (newDone != oldDone)
                deltas.Add($"LastDone {OldNew(oldDone?.ToShortDateString(), newDone?.ToShortDateString())}");
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

        internal static DateTime? CalcNextDo(Job job)
        {
            if (job.LastDone.HasValue && !job.IntervalDays.HasValue)
                return null;    // it is not repeated and it has been done
            else if (!job.LastDone.HasValue)
                return DateTime.Today.AddDays(.999);
            else
                return job.LastDone.Value.Date.AddDays(job.IntervalDays.Value + .999);
        }
    }
}
