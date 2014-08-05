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
        private Dictionary<string, BOSetting> siteSettings = new Dictionary<string, BOSetting>();
        private List<int> languages = new List<int>();

        public BOWebSite() { }

        public BOWebSite(int id, int contentID) 
            : base()
		{
			this.id = id;
			this.ContentId = contentID;
		}

        public string PreviewUrl { get; set; }

		public Dictionary<string, BOSetting> Settings
		{
			get { return siteSettings; }
			set { siteSettings = value; }
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
                if (siteSettings.ContainsKey("PrimaryLanguageId"))
                {
                    return int.Parse(siteSettings["PrimaryLanguageId"].Value);
                }
                else
                {
                    return -1;
                }
            }
            set 
            {
                BOSetting setting = new BOSetting("PrimaryLanguageId", "int", value.ToString(), BOSetting.USER_VISIBILITY_SPECIAL);
                siteSettings["PrimaryLanguageId"] = setting;
            }
        }

        public bool HasGoogleAnalytics 
        { 
            get 
            {
                if (Settings.ContainsKey("GoogleAnalyticsCode"))
                {
                    string code = Settings["GoogleAnalyticsCode"].Value;
                    if (!(code.Equals("UA-xxxx-x") || code.Length < 6))
                    {
                        return true;
                    }
                }
                return false;
            } 
        }
    }
}