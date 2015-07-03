using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace One.Net.BLL.Model.Attributes
{
    public enum SettingType {  String, Url }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class Setting : System.Attribute
    {
        public string Name { get; set; }
        public SettingType Type { get; set; }

        public string DefaultValue { get; set; }

        public Setting(string name, SettingType type)
        {
            Name = name;
            Type = type;
        }
    }
}
