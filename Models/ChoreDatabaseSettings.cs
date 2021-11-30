namespace ChoreMgr.Models
{
    public class ChoreDatabaseSettings : IChoreDatabaseSettings
    {
        public ChoreDatabaseSettings()
        {

        }

        public ChoreDatabaseSettings(IChoreDatabaseSettings other)
        {
            ConnectionString = other.ConnectionString;
            DatabaseName = other.DatabaseName;
            UseDevTables = other.UseDevTables;
        }

        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public bool UseDevTables { get; set; }
    }
    public interface IChoreDatabaseSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        bool UseDevTables { get; set; }
    }
}
