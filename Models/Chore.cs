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
    }
}
