using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Activation;
using System.Runtime.Serialization;
using One.Net.BLL.DAL;
using One.Net.BLL.Forms;
using Newtonsoft.Json;

namespace One.Net.BLL.Service
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class FormService : IFormService
    {
        private  static BForm formB = new BForm();

        public string Ping()
        {
            return "Hello";
        }

        public DTOForm Get(int id)
        {
            var form = formB.Get(id);
            var result = new DTOForm(form);
            return result;
        }
    }

    [Serializable]
    [DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
    public class DTOAnswer
    {
        [DataMember, JsonProperty]
        public int Id { get; set; }

        [DataMember, JsonProperty]
        public string Title { get; set; }

        [DataMember, JsonProperty]
        public string AnswerType { get; set; }

        public DTOAnswer() { }
        public DTOAnswer(BOAnswer answer)
        {
            Id = answer.Id.Value;
            Title = answer.Title;
            AnswerType = answer.AnswerType.ToString("F");
        }
    }


    [Serializable]
    [DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
    public class DTOQuestion
    {
        [DataMember, JsonProperty]
        public int Id { get; set; }

        [DataMember, JsonProperty]
        public string Title { get; set; }

        [DataMember, JsonProperty]
        public string Description { get; set; }

        [DataMember, JsonProperty]
        public List<DTOAnswer> Answers { get; set; }

        public DTOQuestion() { }
        public DTOQuestion(BOQuestion question) 
        {
            Id = question.Id.Value;
            Title = question.Title;
            Description = question.Teaser;
            Answers = new List<DTOAnswer>();
            foreach (var a in question.Answers.Values)
            {
                Answers.Add(new DTOAnswer(a));
            }
        }
    }

    [Serializable]
    [DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
    public class DTOSection
    {
        [DataMember, JsonProperty]
        public int Id { get; set; }

        [DataMember, JsonProperty]
        public List<DTOQuestion> Questions { get; set; }

        public DTOSection() {
        }
        public DTOSection(BOSection section) 
        {
            Questions = new List<DTOQuestion>();
            Id = section.Id.Value;
            foreach (var q in section.Questions.Values)
            {
                Questions.Add(new DTOQuestion(q));
            }
        }
    }

    [Serializable]
    [DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
    public class DTOForm
    {

        [DataMember, JsonProperty]
        public int Id { get; set; }

        [DataMember, JsonProperty]
        public string Title { get; set; }

        [DataMember, JsonProperty]
        public string Description { get; set; }

        [DataMember, JsonProperty]
        public List<DTOSection> Sections { get; set; }

        public DTOForm() { }
        public DTOForm(BOForm form) 
        {
            Id = form.Id.Value;
            Title = form.Title;
            Description = form.Teaser;
            Sections = new List<DTOSection>();

            foreach (var s in form.Sections.Values)
            {
                Sections.Add(new DTOSection(s));
            }
        }

    }
}
