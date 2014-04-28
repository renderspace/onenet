using System;
using System.Reflection;
using System.Web;

namespace CKFinder.Utils
{
    /// <summary> 
    ///     Disable AppDomain restarts when folder is deleted.
    ///     https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=240686
    /// </summary> 
    public class StopAppDomainRestartModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            PropertyInfo p = typeof(HttpRuntime).GetProperty("FileChangesMonitor",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            object o = p.GetValue(null, null);

            FieldInfo f = o.GetType().GetField("_dirMonSubdirs",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase);

            object monitor = f.GetValue(o);

            MethodInfo m = monitor.GetType().GetMethod("StopMonitoring",
                BindingFlags.Instance | BindingFlags.NonPublic);

            m.Invoke(monitor, new object[] { });
        }

        public void Dispose() { }
    }
}
