namespace ChoreMgr.Models
{
    public class ChoreDatabaseSettings : IChoreDatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
    public interface IChoreDatabaseSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
