using System.Collections.Generic;

namespace One.Net.BLL
{
    public class BOPlaceHolder
    {
        int? id;
        string name;

        List<BOModuleInstance> instances = new List<BOModuleInstance>();


        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int? Id
        {
            get { return this.id; }
            set { this.id = value;}
        }

        public List<BOModuleInstance> ModuleInstances
        {
            get { return instances;}
            set { this.instances = value; }
        }
    }
}
