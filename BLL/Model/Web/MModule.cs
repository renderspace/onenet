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
        public SiteMapNode CurrentSiteMapNode
        {
            get
            {
                return SiteMap.CurrentNode;
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

        protected string TranslateComplex(string keyword)
        {
            if (UniqueTranslation)
                return BContent.GetComplexMeaningRendered(keyword + "_" + InstanceId);
            else
                return BContent.GetComplexMeaningRendered(keyword);
        }

        public string ExtraCssClass 
        { 
            get 
            {
                return GetStringSetting("ExtraCssClass");
            } 
        }

        public string InstanceComment
        {
            get
            {
                return GetStringSetting("InstanceComment");
            }
        }

        protected static bool PublishFlag
        {
            get { return bool.Parse(ConfigurationManager.AppSettings["PublishFlag"]); }
        }

        public string CustomClientID { get; set; }

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

        public bool HasHumanReadableUrlParameter
        {
            get
            {
                return !string.IsNullOrWhiteSpace(HumanReadableUrlParameter);
            }
        }

        public string HumanReadableUrlParameter
        {
            get
            {
                if (Page.RouteData.Values["parameter"] != null)
                {
                    return Page.RouteData.Values["parameter"] as string;
                }
                return null;
            }
        }

        public string RelativePageUri { get; set; }

        public int InstanceId { get; set; }

        public int PageId { get; set; }

        public int WebSiteId {get; set; }

        public string WebSiteTitle { get; set; }

        public Dictionary<string, BOSetting> Settings { get; set; }

        public List<int> GetIntegerListSetting(string settingName)
        {
            if (Settings == null || !Settings.ContainsKey(settingName))
                return new List<int>();

            BOSetting setting = Settings[settingName];
            if (!(setting.Type.Equals("CSInteger")))
            {
                throw new ApplicationException("not a string/IntList setting; probably error in database");
            }
            return StringTool.SplitStringToIntegers(setting.Value);
        }

        public string[] GetStringListSetting(string settingName)
        {
            if (Settings == null || !Settings.ContainsKey(settingName))
                return new string[0];

            BOSetting setting = Settings[settingName];
            if (!(setting.Type.Equals("CSString")))
            {
                throw new ApplicationException("not a comma separated strings setting; probably error in database");
            }
            return (string.IsNullOrEmpty(setting.Value) ? null : setting.Value.Split(';')); 
        }


        public string GetStringSetting(string settingName, string defaultValue = "")
        {
            if (Settings == null || !Settings.ContainsKey(settingName))
                return defaultValue;

            BOSetting setting = Settings[settingName];
            if (!setting.Type.Equals("String") && !(setting.Type.Equals("CSInteger")) && !(setting.Type.Equals("Url")))
            {
                throw new ApplicationException("not a String setting; probably error in database " + settingName);
            }
            return setting.Value;
        }

        public BOImageTemplate GetImageTemplate(string settingName)
        {
            if (Settings == null || !Settings.ContainsKey(settingName))
                return null;

            BOSetting setting = Settings[settingName];
            if (!(setting.Type.Equals("ImageTemplate") || setting.Type.Equals("Int")))
            {
                throw new ApplicationException("not a Image template setting; probably error in database");
            }
            var templateId = Int32.Parse(setting.Value.ToString());

            object o = BWebsite.GetTemplate(templateId);
            if (o is BOImageTemplate)
                return (BOImageTemplate)o;
            else
                return null;
        }

        public int GetIntegerSetting(string settingName)
        {
            if (Settings == null)
                return -1;

            if (!Settings.ContainsKey(settingName))
                return -1;

            BOSetting setting = Settings[settingName];
            if (!setting.Type.Equals("Int") && !setting.Type.Equals("ImageTemplate"))
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

        public Dictionary<string, string> ExtraAttributes
        {
            get;
            set;
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
