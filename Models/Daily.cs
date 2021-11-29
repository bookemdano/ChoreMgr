using System.ComponentModel.DataAnnotations;

namespace ChoreMgr.Models
{
    public class Daily
    {
        public Daily(DateTime done, int count)
        {
            DoneDate = done;
            Count = count;
        }
        [DisplayFormat(DataFormatString = "{0:dd-MMM}")]
        public DateTime DoneDate { get; }
        public int Count { get; }
    }

}
