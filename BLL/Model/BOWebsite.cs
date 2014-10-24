using System;
using System.Collections.Generic;
using System.Globalization;

namespace One.Net.BLL
{
	/// <summary>
	/// Summary description for BOWebsite.
	/// </summary>
    [Serializable]
	public class BOWebSite : BOInternalContent
	{
        private int id = -1;
        private int? rootPageId;
        private List<int> languages = new List<int>();

        public BOWebSite() { 
            Settings = new Dictionary<string, BOSetting>();
        }

        public BOWebSite(int id, int contentID) 
            : base()
		{
			this.id = id;
			this.ContentId = contentID;
		}

		public Dictionary<string, BOSetting> Settings { get; set;}

        public string GetSettingValue(string key)
        {
            if (Settings == null || Settings.Count == 0 || !Settings.ContainsKey(key))
                return "";
            return Settings[key].Value;
        }

        public int GetSettingValueInt(string key)
        {
            var result = -1;
            var v = GetSettingValue(key);
            int.TryParse(v, out result);
            return result;
        }

        public bool GetSettingValueBool(string key)
        {
            var result = FormatTool.GetBoolean(GetSettingValue(key));
            return result;
        }

        public bool IsNew
        {
            get { return (id <= 0); }
        }
		
		public int Id
		{
			get { return id; }
			set { id = value; }
		}

        public int? RootPageId
        {
            get { return rootPageId; }
            set { rootPageId = value; }
        }

        public string DisplayName
        {
            get
            {
                return Title + " [" + Culture.ThreeLetterISOLanguageName + "]";
            }
        }

        public CultureInfo Culture
        {
            get
            {
                return new CultureInfo(LanguageId);
            }
        }

     
        public override int LanguageId
        {
            get 
            {
                return GetSettingValueInt("PrimaryLanguageId");
            }
            set 
            {
                BOSetting setting = new BOSetting("PrimaryLanguageId", "int", value.ToString(), BOSetting.USER_VISIBILITY_SPECIAL);
                Settings["PrimaryLanguageId"] = setting;
            }
        }

        public CultureInfo Languge
        {
            get
            {
                if (LanguageId > 0)
                    return new CultureInfo(LanguageId);
                else
                    return null;
            }
        }

        public string DefaultOgImage
        {
            get
            {
                return SubTitle;
            }  
        }

        public string PreviewUrl
        {
            get
            {
                var url = GetSettingValue("PreviewUrl");
                if (url.StartsWith("http"))
                    return url;
                return "";
            }
            set
            {
                if (!value.StartsWith("http"))
                    throw new ArgumentException("URL must start with http");

                if (Settings.ContainsKey("PreviewUrl"))
                    Settings["PreviewUrl"].Value = value;
                else
                    Settings.Add("PreviewUrl", new BOSetting("PreviewUrl", "Url", value, BOSetting.USER_VISIBILITY_NORMAL));
            }
        }
        public string ProductionUrl
        {
            get
            {
                var url = GetSettingValue("ProductionUrl");
                if (url.StartsWith("http"))
                    return url;
                return "";
            }
            set
            {
                if (!value.StartsWith("http"))
                    throw new ArgumentException("URL must start with http");

                if (Settings.ContainsKey("ProductionUrl"))
                    Settings["ProductionUrl"].Value = value;
                else
                    Settings.Add("ProductionUrl", new BOSetting("ProductionUrl", "Url", value, BOSetting.USER_VISIBILITY_NORMAL));
            }
        }

        public long FacebookApplicationID
        {
            get
            {
                var appIdStr = GetSettingValue("FacebookApplicationID");
                long appId = 0;
                long.TryParse(appIdStr, out appId);
                return appId;
            }
        }

        public bool HasGoogleAnalytics 
        { 
            get 
            {
                var code = GetSettingValue("GoogleAnalyticsWebPropertyID");
                if (!(code.Equals("UA-xxxx-x") || code.Length < 6))
                    return true;
                return false;
            } 
        }

        public bool HasAdvertisingFeatures
        {
            get
            {
                return GetSettingValueBool("AdvertisingFeatures");
            }
        }
    }
}