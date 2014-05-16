using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace One.Net.BLL.Web
{
    public abstract class MModule : UserControl
    {
        private string extracssclass;
        private int instanceId;
        private int pageId;
        private int webSiteId;
        private string webSiteTitle;
        private string relativePageUri;
        private Dictionary<string, BOSetting> settings;

        public SiteMapNode CurrentSiteMapNode
        {
            get
            {
                return SiteMap.CurrentNode;
            }
        }

        public int CurrentPageId
        {
            get
            {
                if (CurrentSiteMapNode == null)
                    return 0;
                var currentPageId = int.Parse(CurrentSiteMapNode["_pageID"]);
                return currentPageId;
            }
        }

        public string ParentPageUrl
        {
            get
            {
                if (CurrentSiteMapNode == null || CurrentSiteMapNode.ParentNode == null)
                    return "/";

                return CurrentSiteMapNode.ParentNode.Url;
            }
        }



        public int CurrentWebSiteId
        {
            get
            {
                var currentWebSiteId = int.Parse(ConfigurationManager.AppSettings["WebSiteId"]);
                return currentWebSiteId;
            }
        }

        protected string TranslateComplex(string keyword)
        {
            if (UniqueTranslation)
                return BContent.GetComplexMeaningRendered(keyword + "_" + InstanceId);
            else
                return BContent.GetComplexMeaningRendered(keyword);
        }

        public string ExtraCssClass
        {
            get { return extracssclass; }
            set { extracssclass = value; }
        }

        protected string TranslateText(string keyword)
        {
            if (UniqueTranslation)
                return BContent.GetMeaning(keyword + "_" + InstanceId, true);
            else
                return BContent.GetMeaning(keyword, true);
        }

        protected string Translate(string keyword)
        {
            if (UniqueTranslation)
                return BContent.GetMeaning(keyword + "_" + InstanceId);
            else
                return BContent.GetMeaning(keyword);
        }

        protected bool UniqueTranslation
        {
            get { return GetBooleanSetting("UniqueTranslation"); }
        }

        public string RelativePageUri
        {
            get { return relativePageUri; }
            set { relativePageUri = value; }
        }

        public bool SettingsLoaded
        {
            get { return settings != null; }
        }

        public int InstanceId
        {
            get { return instanceId; }
            set { instanceId = value; }
        }

        public int PageId
        {
            get { return pageId; }
            set { pageId = value; }
        }

        public int WebSiteId
        {
            get { return webSiteId; }
            set { webSiteId = value; }
        }

        public string WebSiteTitle
        {
            get { return webSiteTitle; }
            set { webSiteTitle = value; }
        }

        public bool IsViewableByCurrentPrincipal
        {
            get
            {
                if (FrontEndRequireGroupList.Length == 0)
                    return true;

                if (FrontEndRequireGroupList.Contains("UNAUTH"))
                {
                    return !Thread.CurrentPrincipal.Identity.IsAuthenticated;
                }

                string[] roles = FrontEndRequireGroupList.Split(new char[] { ',', ';' });
                bool isViewable = false;

                foreach (string s in roles)
                {
                    string roleName = s;
                    if (s.StartsWith("!"))
                    {
                        roleName = s.TrimStart(new char[] { '!' });
                        isViewable = !Thread.CurrentPrincipal.IsInRole(roleName);
                    }
                    else
                    {
                        isViewable = Thread.CurrentPrincipal.IsInRole(roleName);
                    }
                }
                return isViewable;
            }
        }

        public string FrontEndRequireGroupList
        {
            get { return GetStringSetting("RequireGroupList"); }
        }

        public Dictionary<string, BOSetting> Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        public List<int> GetIntegerListSetting(string settingName)
        {
            if (Settings == null || !Settings.ContainsKey(settingName))
                return new List<int>();

            BOSetting setting = Settings[settingName];
            if (!(setting.Type.Equals("String") || setting.Type.Equals("IntList")) )
            {
                throw new ApplicationException("not a string/IntList setting; probably error in database");
            }
            return StringTool.SplitStringToIntegers(setting.Value);
        }


        public string GetStringSetting(string settingName, string defaultValue = "")
        {
            if (Settings == null || !Settings.ContainsKey(settingName))
                return defaultValue;

            BOSetting setting = Settings[settingName];
            if (!setting.Type.Equals("String"))
            {
                throw new ApplicationException("not a String setting; probably error in database " + settingName);
            }
            return setting.Value;
        }

        public int GetIntegerSetting(string settingName)
        {
            if (Settings == null)
                return -1;

            if (!Settings.ContainsKey(settingName))
                return -1;

            BOSetting setting = Settings[settingName];
            if (!setting.Type.Equals("Int"))
            {
                throw new ApplicationException("not a Int setting; probably error in database");
            }
            return Int32.Parse(setting.Value.ToString());
        }


        public bool GetBooleanSetting(string settingName)
        {
            if (Settings == null || !Settings.ContainsKey(settingName))
            {
                return false;
            }

            BOSetting setting = Settings[settingName];
            if (!setting.Type.Equals("Bool"))
            {
                throw new ApplicationException("not a boolean setting; probably error in database");
            }
            if (setting.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            else if (setting.Value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            else 
            {
                throw new ApplicationException("");
            }
        }

        public static string RenderOrder(int idx)
        {
            int remainder2 = 0;
            int remainder3 = 0;
            int remainder4 = 0;
            string ret;
            Math.DivRem(idx, 2, out remainder2);
            Math.DivRem(idx, 3, out remainder3);
            Math.DivRem(idx, 4, out remainder4);
            ret = (remainder2 == 0) ? "odd " : "even ";
            switch (remainder3)
            {
                case 0: ret += "first "; break;
                case 1: ret += "second "; break;
                case 2: ret += "third "; break;
            }
            switch (remainder4)
            {
                case 0: ret += "divi1 "; break;
                case 1: ret += "divi2 "; break;
                case 2: ret += "divi3 "; break;
                case 3: ret += "divi4 "; break;
            }
            return ret.TrimEnd();
        }
    }
}
