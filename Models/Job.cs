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
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Name { get; set; }
        public int? IntervalDays { get; set; }

        [Display(Name = "Last")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MMM}")]
        public DateTime? LastDone { get; set; }

        internal void Update(Job other)
        {
            Name = other.Name;
            IntervalDays = other.IntervalDays;
            LastDone = other.LastDone;
        }

        public override string ToString()
        {
            return $"{Name}({Id})";
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
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM}")]
        public DateTime? NextDo
        {
            get
            {
                return CalcNextDo(this);
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
