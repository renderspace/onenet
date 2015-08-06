using System;
using System.Collections.Generic;

namespace One.Net.BLL
{
    public enum VisibilityEnum { NORMAL, COMMON, SPECIAL, MULTILINE };

    public enum SettingTypeEnum { Int, Bool, Image, ImageTemplate, CSInteger, CSString, String, Url };

	[Serializable]
	public class BOSetting
	{    
        private Dictionary<string, string> options;

        public BOSetting()
        { }

        public BOSetting(string settingName, SettingTypeEnum settingType, string settingValue, VisibilityEnum userVisibility)
        {
            Name = settingName;
            Type = settingType;
            Value = settingValue;
            UserVisibility = userVisibility;
        }
		
		public string Name { get; set; }

        public SettingTypeEnum Type { get; set; }

        public string Value { get; set; }

        public VisibilityEnum UserVisibility { get; set; }

        public bool IsVisible
        {
            get { return UserVisibility != VisibilityEnum.SPECIAL; }
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