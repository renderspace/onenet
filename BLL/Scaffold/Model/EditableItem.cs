using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using One.Net.BLL.Scaffold.Model;

namespace One.Net.BLL.Scaffold.Model
{
    [Serializable]
    public class EditableItem
    {
        public string StartingTable { get; set; }

        public List<string> PrimaryKeys { get; set; }
        public Dictionary<string, VirtualColumn> Columns { get; set; }

        public EditableItem()
        {
            Columns = new Dictionary<string, VirtualColumn>();
        }
    }
}