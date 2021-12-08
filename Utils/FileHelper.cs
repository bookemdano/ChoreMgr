namespace ChoreMgr.Utils
{
    static public class FileHelper
    {
        static public string CreateDatedFolder(string root)
        {
            Directory.CreateDirectory(root);
            root = Path.Combine(root, $"{DateTime.Today.ToString("yyyy")}");
            Directory.CreateDirectory(root);
            root = Path.Combine(root, $"{DateTime.Today.ToString("yyyyMM")}");
            Directory.CreateDirectory(root);
            return root;
        }
    }
}
