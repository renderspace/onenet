using System;
using System.IO;
using System.Threading;
using System.Web;

namespace One.Net.BLL
{
    public sealed class OContext
    {
        private string _rootPath = null;
        HttpContext _httpContext = null;
        private const string DataKey = "OContextStore";

        public bool IsWebRequest
        {
            get { return this.Context != null; }
        }

        public HttpContext Context
        {
            get { return _httpContext; }
        }

        public static OContext Current
        {
            get
            {
                HttpContext httpContext = HttpContext.Current;
                OContext context = null;
                if (httpContext != null)
                {
					context = httpContext.Items[DataKey] as OContext;
                }
                else
                {
                    context = Thread.GetData(GetSlot()) as OContext;
                }

                if (context == null)
                {

                    if (httpContext == null)
                        throw new Exception("No OContext exists in the Current Application. AutoCreate fails since HttpContext.Current is not accessible.");

                    context = new OContext(httpContext);
                    SaveContextToStore(context);
                }
                return context;
            }
        }

        /// <summary>
		/// cnst called when HttpContext is avaiable
		/// </summary>
		/// <param name="context"></param>
		private OContext(HttpContext context)
		{
			this._httpContext = context;
		}

        public string MapPath(string path)
        {
            if (_httpContext != null)
                return _httpContext.Server.MapPath(path);
            else
                // Returns System\WOW for non web // return Directory.GetCurrentDirectory() + path.Replace("/", @"\").Replace("~", "");
                return PhysicalPath(path.Replace("/", Path.DirectorySeparatorChar.ToString()).Replace("~", ""));
        }

        

        public string PhysicalPath(string path)
        {
            return string.Concat(RootPath().TrimEnd(Path.DirectorySeparatorChar), Path.DirectorySeparatorChar.ToString(), path.TrimStart(Path.DirectorySeparatorChar)).Replace("\\bin","").Replace("\\Debug", "").Replace("\\Release", "");
        }

        private string RootPath()
        {
            if (_rootPath == null)
            {
                _rootPath = AppDomain.CurrentDomain.BaseDirectory;
                string dirSep = Path.DirectorySeparatorChar.ToString();

                _rootPath = _rootPath.Replace("/", dirSep);

                string filePath = "/";

                if (filePath != null)
                {
                    filePath = filePath.Replace("/", dirSep);

                    if (filePath.Length > 0 && filePath.StartsWith(dirSep) && _rootPath.EndsWith(dirSep))
                    {
                        _rootPath = _rootPath + filePath.Substring(1);
                    }
                    else
                    {
                        _rootPath = _rootPath + filePath;
                    }
                }
            }
            return _rootPath;
        }

        private static void SaveContextToStore(OContext context)
        {
            if (context.IsWebRequest)
            {
				context.Context.Items[DataKey] = context;
            }
            else
            {
                Thread.SetData(GetSlot(), context);
            }

        }

        private static LocalDataStoreSlot GetSlot()
        {
			return Thread.GetNamedDataSlot(DataKey);
        }
    }
}
