using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace One.Net.BLL
{
    [Serializable]
    public class BOContentTemplate
    {
        public BOContentTemplate()
        { }

        public int? Id { get; set; }

        public DateTime? DateModified { get; set; }
        public string PrincipalModified { get; set; }

        public DateTime DateCreated { get; set; }
        public string PrincipalCreated { get; set; }

        public Dictionary<string, string> ContentFields { get; set; }
    }
}
