using Newtonsoft.Json;

namespace ChoreMgr.Utils
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

        internal static void LogChange(HttpContext context, object? obj = null)
        {
            LogWithContext("CHANGE", context, obj);
        }
        internal static void LogView(HttpContext context, object? obj = null)
        {
            LogWithContext("VIEW", context, obj);
        }
        static void LogWithContext(string type, HttpContext context, object? obj = null)
        {
            var text = string.Empty;
            if (obj != null)
                text = " obj:" + JsonConvert.SerializeObject(obj);
            Log($"{type} from:{context.Request.Path} q:{context.Request.QueryString} from:{context.Request.HttpContext.Connection.RemoteIpAddress}{text}");
        }
    }
}

