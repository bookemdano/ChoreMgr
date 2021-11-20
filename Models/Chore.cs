using System.ComponentModel.DataAnnotations;

namespace ChoreMgr.Models
{
    public class Chore
    {
        public Chore()
        {

        }
        public Chore(string name)
        {
            Name = name;
            Id = name.GetHashCode();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MMM}")]
        public DateTime? LastDone { get; set; }

        public int? IntervalDays { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM}")]
        public DateTime? NextDo { get; internal set; }
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

        internal static Chore Fake()
        {
            var rnd = new Random();
            var rv = new Chore(Guid.NewGuid().ToString());
            rv.Id = rnd.Next();
            rv.LastDone = DateTime.Today.AddDays(rnd.Next(-10, 3));
            rv.IntervalDays = rnd.Next(7);
            return rv;
        }
    }
}
