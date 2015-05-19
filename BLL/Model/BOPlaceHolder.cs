using System;
using System.Collections.Generic;

namespace One.Net.BLL
{
    [Serializable]
    public class BOPlaceHolder
    {
        List<BOModuleInstance> instances = new List<BOModuleInstance>();


        public string Name { get; set; }

        public int? Id { get; set; }

        public List<BOModuleInstance> ModuleInstances
        {
            get { return instances;}
            set { this.instances = value; }
        }
    }
}
