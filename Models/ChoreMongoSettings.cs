namespace ChoreMgr.Models
{
    public class ChoreMongoSettings : IChoreMongoSettings
    {
        public ChoreMongoSettings()
        {

        }

        public ChoreMongoSettings(IChoreMongoSettings other)
        {
            ConnectionString = other.ConnectionString;
            DatabaseName = other.DatabaseName;
            UseDevTables = other.UseDevTables;
        }

        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public bool UseDevTables { get; set; }
    }
    public interface IChoreMongoSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        bool UseDevTables { get; set; }
    }
}
