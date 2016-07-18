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
using Newtonsoft.Json.Converters;

namespace One.Net.Forms
{
    [DataContract, JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class DTOQuestion
    {
        [DataMember]
        public int Idx { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string ValidationType { get; set; }

        [DataMember]
        public IEnumerable<DTOAnswer> Answers
        {
            get; set;
        }

        public DTOQuestion() { }

        public DTOQuestion(BOQuestion question)
        {
            this.Idx = question.Idx;
            this.Description = question.Description;
            this.Title = question.Title;
            this.ValidationType = Enum.GetName(typeof(ValidationTypes), question.ValidationType);
            var answers = new List<DTOAnswer>();
            foreach (var a in question.Answers.Values)
            {
                answers.Add(new DTOAnswer(a));
            }
            this.Answers = answers;
        }
    }
}
