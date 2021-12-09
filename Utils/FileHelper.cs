namespace ChoreMgr.Utils
{
    static public class FileHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="dir"></param>
        /// <returns>returns true if it was really created, false if it already existed</returns>
        static public string CreateDatedFolder(string root, bool monthOnly = true)
        {
            Directory.CreateDirectory(root);
            root = Path.Combine(root, $"{DateTime.Today.ToString("yyyy")}");
            Directory.CreateDirectory(root);
            root = Path.Combine(root, $"{DateTime.Today.ToString("yyyyMM")}");
            Directory.CreateDirectory(root);
            if (!monthOnly)
            {
                root = Path.Combine(root, $"{DateTime.Today.ToString("yyyyMMdd")}");
                Directory.CreateDirectory(root);
            }
            return root;
        }
        static public string CreateDatedFilename(string root, string name, string ext)
        {
            var dir = FileHelper.CreateDatedFolder(root);
            return Path.Combine(dir, $"{name} {DateTime.Now.ToString("yyyyMMdd HHmmss")}.{ext.Replace(".", "")}");

        }
    }
}
