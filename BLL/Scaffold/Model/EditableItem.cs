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
        public string FriendlyName { get; set; }

        public DateTime LastChanged
        {
            get { return DateModified.HasValue ? DateModified.Value : DateCreated; }
        }

        public string LastChangedBy
        {
            get { return (DateModified == null) ? PrincipalCreated : PrincipalModified; }
        }

        public string DisplayLastChanged
        {
            get { return LastChanged + ", " + LastChangedBy; }
        }


        public DateTime? DateModified { get; set;}
        public string PrincipalModified { get; set; }
        public DateTime DateCreated { get; set; }

        public string PrincipalCreated { get; set; }

        public bool HasAudit
        {
            get { return true; }
        }

        public List<string> PrimaryKeys { get; set; }
        public Dictionary<string, VirtualColumn> Columns { get; set; }

        public EditableItem()
        {
            Columns = new Dictionary<string, VirtualColumn>();
        }
    }
}