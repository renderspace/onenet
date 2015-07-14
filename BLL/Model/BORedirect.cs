using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace One.Net.BLL
{
    [Serializable]
    public class BORedirect
    {
        public int? Id { get; set; }
        public string FromLink { get; set; }
        public string ToLink { get; set; }
        public DateTime Created { get; set; }
        public bool IsShortener { get; set; }
    }
}
