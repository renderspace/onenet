using System;
using System.Collections.Generic;

namespace One.Net.BLL
{
    [Serializable]
    public class BOModuleInstance
    {

        private Dictionary<string, BOSetting> settings = new Dictionary<string, BOSetting>();

        int id;
        int moduleId;
        int order, persistTo, persistFrom;
        int pageId;
        bool pendingDelete, changed, publishFlag, isInherited;

        string name;
        int placeHolderId;

        public BOModuleInstance()
        { }


        public BOModuleInstance(int moduleInstanceID, int moduleID, int order, int pageID, string name)
        {
            this.id = moduleInstanceID;
            this.moduleId = moduleID;
            this.order = order;
            this.pageId = pageID;
            this.name = name;
        }

        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public string ExpandedName
        {
            get { return this.name + " [" + Id + "]"; }
        }

        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public int ModuleId
        {
            get { return this.moduleId; }
            set { this.moduleId = value; }
        }

        public bool IsInherited
        {
            get { return isInherited; }
            set { isInherited = value; }
        }

        public int Order
        {
            get { return this.order; }
            set { this.order = value; }
        }

        public int PageId
        {
            get { return this.pageId; }
            set { this.pageId = value; }
        }

        public int PlaceHolderId
        {
            set { this.placeHolderId = value; }
            get { return placeHolderId; }
        }

        public string ModuleSource
        {
            get
            {
                string moduleSource = Settings.ContainsKey("ModuleSource") && Settings["ModuleSource"].Value.Length > 0 ? Settings["ModuleSource"].Value : name;
                return moduleSource + ".ascx";
            }
        }

        public Dictionary<string, BOSetting> Settings
        {
            get { return settings; }
            set { this.settings = value; }
        }

        public int PersistFrom
        {
            get { return persistFrom; }
            set { this.persistFrom = value; }
        }

        public int PersistTo
        {
            get { return persistTo; }
            set { this.persistTo = value; }
        }

        public bool PendingDelete
        {
            get { return pendingDelete; }
            set { this.pendingDelete = value; }
        }

        public bool Changed
        {
            get { return changed; }
            set { this.changed = value; }
        }

        public bool PublishFlag
        {
            get { return publishFlag; }
            set { publishFlag = value; }
        }

        public bool Persists
        {
            get { return this.PersistFrom != this.PersistTo; }
        }
    }
}
