using ChoreMgr.Models;
using ChoreMgr.Utils;
using Newtonsoft.Json;

namespace ChoreMgr.Data
{
    public class JsonDb
    {
        private IJsonDbSettings _settings;

        public JsonDb(IJsonDbSettings settings)
        {
            _settings = settings;
        }

        protected bool WriteJsonDb<T>(IList<T> objs)
        {
            // write backup
            Backup<T>(objs);

            return WriteJsonDb<T>(objs, GetFile<T>());
        }
        protected bool Backup<T>(IList<T> objs)
        {
            return WriteJsonDb<T>(objs, FileHelper.CreateDatedFilename(ArchiveDirectory, GetFilename<T>(), "json"));
        }

        #region IO methods

        protected IList<T> ReadJsonDb<T>()
        {
            if (!File.Exists(GetFile<T>()))
                return new List<T>();

            var str = File.ReadAllText(GetFile<T>());
            var rv = JsonConvert.DeserializeObject<IList<T>>(str);
            if (rv == null)
                rv = new List<T>();
            return rv;
        }
        bool WriteJsonDb<T>(IList<T> objs, string filename)
        {
            DanLogger.Log($"DATA WriteJsonDb {typeof(T)} {filename} {objs?.Count} items");
            var str = JsonConvert.SerializeObject(objs, Formatting.Indented);
            File.WriteAllText(filename, str);
            return true;
        }
        
        #endregion

        #region locations and naming
        
        string GetFile<T>()
        {
            return Path.Combine(_settings.Directory, GetFilename<T>() + ".json");
        }
        protected string GetFilename<T>()
        {
            var prefix = string.Empty;
            if (_settings.UseDevTables)
                prefix = "DEV_";
            return $"{prefix}{typeof(T).Name}";
        }

        protected string ArchiveDirectory
        {
            get
            {
                return Path.Combine(_settings.Directory, "archive");
            }
        }
        #endregion

        #region debug and diagnostic

        protected IJsonDbSettings CloneSettingsFromProd()
        {
            var prodSettings = new JsonDbSettings(_settings);
            prodSettings.UseDevTables = false;
            return prodSettings;
        }

        public override string ToString()
        {
            return $"Source:{_settings.Directory} UseDevTables:{_settings.UseDevTables}";
        }

        #endregion
    }
}
