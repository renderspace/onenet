﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace One.Net.BLL.Model.Attributes
{
    public enum SettingType { String, Int, Bool, ImageTemplate, Url, CSInteger, CSString }
    public enum SettingVisibility { NORMAL, SPECIAL }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class Setting : System.Attribute
    {
        public string Name { get; set; }
        public SettingType Type { get; set; }
        public SettingVisibility Visibility { get; set; }

        public string DefaultValue { get; set; }

        public Setting( SettingType type)
        {
            Type = type;
        }
    }
}
