namespace ChoreMgr
{
    static public class DanLogger
    {
        static public void Log(object o)
        {
            File.AppendAllText($"endless {DateTime.Now.ToString("yyyy-MM")}.log", $"{DateTime.Now} {o} {Environment.NewLine}");
        }

        internal static void Error(object o, Exception ex)
        {
            Log($"{o} {ex.Message} {ex}");
        }
    }
}

