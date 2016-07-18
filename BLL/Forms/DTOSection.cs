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
    public class DTOSection
    {
        [DataMember, JsonProperty]
        public int Idx { get; set; }

        [DataMember, JsonProperty]
        public string Title { get; set; }

        [DataMember, JsonProperty]
        public string Description { get; set; }

        [DataMember, JsonProperty]
        public IEnumerable<DTOQuestion> Questions
        {
            get; set;
        }

        public DTOSection() { }

        public DTOSection(BOSection section)
        {
            this.Idx = section.Idx;
            this.Description = section.Description;
            this.Title = section.Title;
            var questions = new List<DTOQuestion>();
            foreach (var q in section.Questions.Values)
            {
                questions.Add(new DTOQuestion(q));
            }
            this.Questions = questions;
        }
    }
}
