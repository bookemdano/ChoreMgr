using System.ComponentModel.DataAnnotations;

namespace ChoreMgr.Models
{
    public class Chore
    {
        public Chore()
        {

        }
        public Chore(int id, string name)
        {
            Name = name;
            Id = id;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Row
        {
            get
            {
                return Id;
            }
        }

        [Display(Name = "Last")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MMM}")]
        public DateTime? LastDone { get; set; }

        [Display(Name = "Int")]
        public int? IntervalDays { get; set; }
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
                    return LastDone.Value.AddDays(IntervalDays.Value + .999);
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
        public override string ToString()
        {
            return $"{Name}({Id})";
        }

        static internal string DeltaString(Chore? oldChore, Chore? newChore)
        {
            var deltas = new List<string>();
            if (oldChore == null)
                deltas.Add("New");
            if (newChore == null)
                deltas.Add("Delete");
            if (newChore?.Name != oldChore?.Name)
                deltas.Add($"Name {OldNew(oldChore?.Name, newChore?.Name)}");
            if (newChore?.IntervalDays != oldChore?.IntervalDays)
                deltas.Add($"Interval {OldNew(oldChore?.IntervalDays, newChore?.IntervalDays)}");
            if (newChore?.LastDone != oldChore?.LastDone)
                deltas.Add($"LastDone {OldNew(oldChore?.LastDone?.ToShortDateString(), newChore?.LastDone?.ToShortDateString())}");
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
