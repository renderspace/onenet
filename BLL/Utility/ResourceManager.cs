using System.Collections;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Xml;
using log4net;

namespace One.Net.BLL
{
	public class HtmlResource
	{
        private string name;
		private string title;
		private string body;

		public HtmlResource(XmlNode node)
		{
			name = node.Attributes["name"].Value;
			title = (node.SelectSingleNode("title") != null) ? node.SelectSingleNode("title").InnerText : node.InnerText;
			body = (node.SelectSingleNode("body") != null) ? node.SelectSingleNode("body").InnerText : "";
		}

		public HtmlResource(string name, string NoTranslationTag)
		{
			this.name = name;
			this.title = name;
			this.body = NoTranslationTag;
		}

		public string Name
		{
			get { return name; }
		}

		public string Title
		{
			get { return title; }
			set { title = value; }
		}

		public string Body
		{
			get { return body; }
			set { body = value; }
		}
	}

	public class ResourceManager
	{
        private const string DEFAULT_RESOURCE_IDENTIFIER = "Resource_";
		public const string DEFAULT_RESOURCE_FILE = "Resources.xml";
		private const string NoTranslationTag = "##{0}##";
		private const string RootNode = "root";

		public static HtmlResource GetHtmlResource(string name)
		{
			if (!name.StartsWith("$"))
				return new HtmlResource(name, NoTranslationTag);
			if (name.Length < 2)
				return new HtmlResource(name, NoTranslationTag);
			else
				name = name.TrimStart('$');
            return GetHtmlResource(name, DEFAULT_RESOURCE_IDENTIFIER, DEFAULT_RESOURCE_FILE);
		}

        public static string GetString(string name, string resourceIdentifier, string resourceFile)
        {
            if (!name.StartsWith("$"))
                return name;
            if (name.Length < 2)
                return "";
            else
                name = name.TrimStart('$');

            return GetHtmlResource(name, resourceIdentifier, resourceFile).Title;
        }

        public static string GetString(string name, string resourceFile)
        {
            if (!name.StartsWith("$"))
                return name;
            if (name.Length < 2)
                return "";
            else
                name = name.TrimStart('$');

            return GetHtmlResource(name, resourceFile, resourceFile).Title;
        }

		public static string GetString(string name)
		{
			if (!name.StartsWith("$"))
				return name;
			if (name.Length < 2)
				return "";
			else
				name = name.TrimStart('$');

            return GetHtmlResource(name, DEFAULT_RESOURCE_IDENTIFIER, DEFAULT_RESOURCE_FILE).Title;
		}

		public static HtmlResource GetHtmlResource(string name, string resourceIdentifier, string fileName)
		{
            string userLanguage = Thread.CurrentThread.CurrentUICulture.IetfLanguageTag;

            /*
            LogManager.GetLogger(typeof(HtmlResource)).Debug("GetHtmlResource " + name + " " + 
                resourceIdentifier + " " + fileName + " " + userLanguage);
            */
            Hashtable resources = GetResource(userLanguage, resourceIdentifier, fileName);
			HtmlResource htmlResource = resources[name] as HtmlResource;
		    return htmlResource ?? new HtmlResource(name, NoTranslationTag);
		}

        private static Hashtable GetResource(string userLanguage, string resourceIdentifier, string fileName)
		{
			Hashtable resources = HttpContext.Current.Cache[resourceIdentifier + userLanguage] as Hashtable;

			if (resources == null)
			{
				resources = LoadResource(userLanguage, fileName);
				string resourcePath = OContext.Current.PhysicalPath("Languages" + Path.DirectorySeparatorChar + userLanguage + Path.DirectorySeparatorChar + fileName);

                if (!File.Exists(resourcePath))
                    throw new IOException("There are no translations for selected UI language (" + userLanguage + ")");

                HttpContext.Current.Cache.Insert(resourceIdentifier + userLanguage, resources, new CacheDependency(resourcePath));
			}
			return resources;
		}

		private static Hashtable LoadResource(string language, string fileName)
		{
		    Hashtable target = new Hashtable();
			XmlDocument xmlDoc = new XmlDocument();

            try
			{
				string resourcePath = OContext.Current.PhysicalPath("Languages" + Path.DirectorySeparatorChar + language + Path.DirectorySeparatorChar + fileName);
				xmlDoc.Load(resourcePath);
			}
			catch
			{
				return target;
			}

			foreach (XmlNode node in xmlDoc.SelectSingleNode(RootNode).ChildNodes)
			{
				if (node.NodeType != XmlNodeType.Comment)
				{
					string name = node.Attributes["name"].Value;
					target[name] = new HtmlResource(node);
				}
			}

			return target;
		}
	}
}