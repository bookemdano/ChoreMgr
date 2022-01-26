namespace ChoreMgr.Models
{
    public class JsonDbSettings : IJsonDbSettings
    {
        public JsonDbSettings()
        {

        }

        public JsonDbSettings(IJsonDbSettings other)
        {
            Directory = other.Directory;
            UseDevTables = other.UseDevTables;
        }
        public string Directory { get; set; }
        public bool UseDevTables { get; set; }
    }
    public interface IJsonDbSettings
    {
        string Directory { get; set; }
        bool UseDevTables { get; set; }
    }
}
