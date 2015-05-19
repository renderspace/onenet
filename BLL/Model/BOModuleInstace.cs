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

        // private Dictionary<string, BOSetting> settings = new Dictionary<string, BOSetting>();

        public string Name { get; set; }

        public string ExpandedName
        {
            get { return Name + " [" + Id + "]"; }
        }

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
                string moduleSource = Settings.ContainsKey("ModuleSource") && Settings["ModuleSource"].Value.Length > 0 ? Settings["ModuleSource"].Value : Name;
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
    }
}
