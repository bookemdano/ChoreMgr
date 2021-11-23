namespace ChoreMgr.Models
{
    public class Journal
    {
        public DateTime Updated { get; set; }
        public int? JobId { get; set; }
        public string? JobName { get; set; }
        public string? Note { get; set; }
    }
}
