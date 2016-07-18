using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Activation;
using System.Runtime.Serialization;
using One.Net.BLL.DAL;
using Newtonsoft.Json;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;
using NLog;
using System.ServiceModel;

namespace One.Net.Forms
{
    [DataContract, JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class DTOForm
    {
        [DataMember, JsonProperty]
        public int Idx { get; set; }

        [DataMember, JsonProperty]
        public string Title { get; set; }

        [DataMember, JsonProperty]
        public string Description { get; set; }

        [DataMember, JsonProperty]
        public IEnumerable<DTOSection> Sections
        {
            get; set;
        }

        public DTOForm() { }

        public DTOForm(BOForm form)
        {
            this.Idx = form.Idx;
            this.Description = form.Description;
            this.Title = form.Title;
            var sections = new List<DTOSection>();
            foreach (var s in form.Sections.Values)
            {
                sections.Add(new DTOSection(s));
            }
            this.Sections = sections;
        }
    }
}
