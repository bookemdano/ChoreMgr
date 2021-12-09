namespace ChoreMgr.Models
{
    public class ChoreJsonDbSettings : IChoreJsonDbSettings
    {
        public ChoreJsonDbSettings()
        {

        }

        public ChoreJsonDbSettings(IChoreJsonDbSettings other)
        {
            Directory = other.Directory;
            UseDevTables = other.UseDevTables;
        }
        public string Directory { get; set; }
        public bool UseDevTables { get; set; }
    }
    public interface IChoreJsonDbSettings
    {
        string Directory { get; set; }
        bool UseDevTables { get; set; }
    }
}
