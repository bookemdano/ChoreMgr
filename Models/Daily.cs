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
        [DisplayFormat(DataFormatString = "{0:M/d/yy}")]
        public DateTime DoneDate { get; }
        public int Count { get; }
    }

}
