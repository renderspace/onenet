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
    public class DTOAnswer
    {
        [DataMember, JsonProperty]
        public int Idx { get; set; }

        [DataMember, JsonProperty]
        public string Title { get; set; }

        [DataMember, JsonProperty]
        public string Description { get; set; }

        [DataMember, JsonProperty]
        public string AdditionalFieldType { get; set; }

        [DataMember, JsonProperty]
        public int MaxChars { get; set; }

        [DataMember, JsonProperty]
        public int NumberOfRows { get; set; }

        [DataMember, JsonProperty]
        public string AnswerType { get; set; }

        public DTOAnswer() { }

        public DTOAnswer(BOAnswer answer)
        {
            this.Idx = answer.Idx;
            this.Title = answer.Title;
            this.Description = answer.Description;
            this.AdditionalFieldType = Enum.GetName(typeof(AdditionalFieldTypes), answer.AdditionalFieldType);
            if (answer.MaxChars.HasValue)
            {
                this.MaxChars = answer.MaxChars.Value;
            }
            if (answer.NumberOfRows.HasValue)
            {
                this.NumberOfRows = answer.NumberOfRows.Value;
            }
            this.AnswerType = Enum.GetName(typeof(AnswerTypes), answer.AnswerType);
        }
    }
}