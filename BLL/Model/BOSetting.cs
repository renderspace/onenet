using System;
using System.Collections.Generic;

namespace One.Net.BLL
{
    public enum Visibility { NORMAL, COMMON, SPECIAL, MULTILINE };

	[Serializable]
	public class BOSetting
	{    
        private Dictionary<string, string> options;

        public BOSetting()
        { }

        public BOSetting(string settingName, string settingType, string settingValue, Visibility userVisibility)
        {
            Name = settingName;
            Type = settingType;
            Value = settingValue;
            UserVisibility = userVisibility;
        }
		
		public string Name { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public Visibility UserVisibility { get; set; }

        public bool IsVisible
        {
            get { return UserVisibility != Visibility.SPECIAL; }
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