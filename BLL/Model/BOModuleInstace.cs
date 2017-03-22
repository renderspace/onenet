using System;
using System.Collections.Generic;

namespace One.Net.BLL
{
    [Serializable]
    public class BOModuleInstance
    {
        public BOModuleInstance()
        {
            Settings = new Dictionary<string, BOSetting>();
        }

        public string ModuleName { get; set; }

        public int Id { get; set; }

        public int ModuleId { get; set; }

        public bool IsInherited { get; set; }

        public int Order { get; set; }

        public int PageId { get; set; }

        public int PlaceHolderId { get; set; }

        public string ModuleSource
        {
            get
            {
                string moduleSource = Settings.ContainsKey("ModuleSource") && Settings["ModuleSource"].Value.Length > 0 ? Settings["ModuleSource"].Value : ModuleName;
                return moduleSource + ".ascx";
            }
        }

        public Dictionary<string, BOSetting> Settings { get; set; }

        public int PersistFrom { get; set; }

        public int PersistTo { get; set; }

        public bool PendingDelete { get; set; }

        public bool Changed { get; set; }

        public bool PublishFlag { get; set; }

        public bool Persists
        {
            get { return this.PersistFrom != this.PersistTo; }
        }

        public DateTime DateCreated { get; set; }

        public bool IsVeryRecent
        {
            get
            {
                return DateTime.Now.Subtract(DateCreated).TotalMinutes < 2;
            }
        }

        public string Color
        {
            get
            {
                switch(PlaceHolderId % 10)
                {
                    case 0:
                        return "#d3adad";
                    case 1:
                        return "#d3ccad";
                    case 2:
                        return "#b9d3ad";
                    case 3:
                        return "#adcdd3";
                    case 4:
                        return "#d3adad";
                    case 5:
                        return "#adbbd3";
                    case 6:
                        return "#b9add3";
                    case 7:
                        return "#d3adbf";
                    case 8:
                        return "#8995a3";
                    case 9:
                        return "#a39e89";
                    default:
                        return "#89a39a";
                }
            }
        }
    }
}
