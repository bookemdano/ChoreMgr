using System.ComponentModel.DataAnnotations;

namespace ChoreMgr.Models
{
    public class Chore
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime LastDone { get; set; }

        public int IntervalDays { get; set; }

        internal static Chore Fake()
        {
            var rnd = new Random();
            var rv = new Chore();
            rv.Name = Guid.NewGuid().ToString();
            rv.Id = rnd.Next();
            rv.LastDone = DateTime.Today.AddDays(rnd.Next(-10, 3));
            rv.IntervalDays = rnd.Next(7);
            return rv;
        }
    }
}
