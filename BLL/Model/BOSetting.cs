using System;
using System.Collections.Generic;

namespace One.Net.BLL
{
	[Serializable]
	public class BOSetting
	{
        public const string USER_VISIBILITY_NORMAL = "NORMAL";
        public const string USER_VISIBILITY_COMMON = "COMMON";
        public const string USER_VISIBILITY_SPECIAL = "SPECIAL";
        public const string USER_VISIBILITY_MULTILINE = "MULTILINE";

        private string name;
        private string type;
        private string value;
        private string userVisibility;
	    private Dictionary<string, string> options;
		
		public BOSetting()
		{}

        public BOSetting(string settingName, string settingType, string settingValue, string userVisibility)
		{
			this.name = settingName;
			this.type = settingType;
            this.value = settingValue;
            this.userVisibility = userVisibility;
		}
		
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public string Type
		{
			get { return type; }
			set { type = value; }
		}

        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public string UserVisibility
        {
            get { return this.userVisibility.ToUpper(); }
            set { this.userVisibility = value; }
        }

        public bool IsVisible
        {
            get { return UserVisibility != BOSetting.USER_VISIBILITY_SPECIAL; }
        }

	    public Dictionary<string, string> Options
	    {
	        get { return options; }
	        set { options = value; }
	    }

	    public bool HasOptions
	    {
            get { return options != null; }
	    }
	}
}