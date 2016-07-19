using System;
using System.Collections.Generic;
using One.Net.BLL;

namespace One.Net.Forms
{
    public enum FormTypes { Questionaire = 2, WeightedQuiz }
    public enum AnswerTypes { Checkbox = 1, Radio, DropDown, SingleText, SingleFile }
    public enum AdditionalFieldTypes { Text = 1, File, None }
    public enum SectionTypes { SinglePage = 1, MultiPage }
    public enum ValidationTypes { None = 1, AlphaNumeric, Numeric, Integer, Email, DateTime, Time, Captcha }

    [Serializable]
    public abstract class BOElement
    {
        public int? Id { get; set; }
        public int? ParentId { get; set; }
        public int Idx { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Weight { get; set; }
    }

    [Serializable]
    public class BOForm : BOElement
    {
        public string ThankYouNote { get; set; }
        public string SubTitle { get; set; }

        private Dictionary<int, BOSection> sections = new Dictionary<int, BOSection>();

        public FormTypes FormType { get; set; }
        public Dictionary<int, BOSection> Sections { get { return sections; } set { sections = value; } }

        public bool AllowMultipleSubmissions { get; set; }
        public bool AllowModifyInSubmission { get; set; }
        public int SubmissionCount { get; set; }
        public string SendToString { get; set; }
        public string CompletionRedirect { get;set; }
        
        public string[] SendToAddresses 
        { 
            get 
            {
                try
                {
                    return (string.IsNullOrEmpty(SendToString) ? null : StringTool.SplitString(SendToString)).ToArray(); 
                }
                catch
                {
                    return null;
                }  
            } 
        }
        
        public DateTime? FirstSubmissionDate { get; set; }
        public DateTime? LastSubmissionDate { get; set; }

        public BOSection FirstSection
        {
            get
            {
                if (!FirstSectionKey.HasValue)
                    return null;
                else
                    return Sections[FirstSectionKey.Value];
            }
        }

        public int? FirstSectionKey
        {
            get
            {
                int? firstKey = null;
                foreach (int key in sections.Keys)
                {
                    firstKey = (int?)key;
                    break;
                }
                return firstKey;
            }
        }

        public List<BOQuestion> Questions
        {
            get 
            {
                var result = new List<BOQuestion>();
                foreach (var s in Sections.Values)
                {
                    result.AddRange(s.Questions.Values);
                }
                return result;
            }
        }

        public static BOQuestion FindQuestion(BOForm form, int questionId)
        {
            if (form != null)
            {
                foreach (BOSection section in form.Sections.Values)
                {
                    foreach (BOQuestion question in section.Questions.Values)
                    {
                        if (question.Id.Value == questionId)
                        {
                            return question;
                        }
                    }
                }
            }

            return null;
        }

        public static BOAnswer FindAnswer(BOForm form, int answerId)
        {
            if (form != null)
            {
                foreach (BOSection section in form.Sections.Values)
                {
                    foreach (BOQuestion question in section.Questions.Values)
                    {
                        foreach (BOAnswer answer in question.Answers.Values)
                        {
                            if (answer.Id.Value == answerId)
                            {
                                return answer;
                            }
                        }
                    }
                }
            }

            return null;
        }


        public static int? GetPrevSectionId(BOForm form, int sectionId)
        {
            if (form != null)
            {
                int? prevSectionId = null;
                foreach (BOSection section in form.Sections.Values)
                {
                    if (section.Id.Value == sectionId)
                    {
                        return prevSectionId;
                    }
                    prevSectionId = section.Id.Value;
                }
            }

            return null;
        }

        public static int? GetNextSectionId(BOForm form, int sectionId)
        {
            if (form != null)
            {
                bool returnNext = false;
                foreach (BOSection section in form.Sections.Values)
                {
                    if (returnNext)
                    {
                        return section.Id.Value;
                    }
                    if (section.Id.Value == sectionId)
                    {
                        returnNext = true;
                    }
                }
            }

            return null;
        }

        public int GetSectionOrder(int sectionId)
        {
            var idx = 0;
            foreach (BOSection section in Sections.Values)
            {
                idx++;
                if (section.Id.Value == sectionId)
                {
                    return idx;
                }
            }
            return idx;
        }
    }

    [Serializable]
    public class BOSection : BOElement
    {
        public const string SECTION_TYPE_MULTI_PAGE = "MultiPage";
        public const string SECTION_TYPE_SINGLE_PAGE = "SinglePage";

        private SectionTypes sectionType = SectionTypes.SinglePage;
        private Dictionary<int, BOQuestion> questions = new Dictionary<int, BOQuestion>();

        public string OnClientClick { get;set; }
        public SectionTypes SectionType { get { return sectionType; } set { sectionType = value; } }
        public Dictionary<int, BOQuestion> Questions { get { return questions; } set { questions = value; } }

        public int? FirstQuestionKey
        {
            get
            {
                int? firstKey = null;
                foreach (int key in questions.Keys)
                {
                    firstKey = (int?)key;
                    break;
                }
                return firstKey;
            }
        }
    }

    [Serializable]
    public class BOQuestion : BOElement
    {
        public const string VALIDATION_TYPE_NONE = "None";
        public const string VALIDATION_TYPE_NUMERIC = "Numeric";
        public const string VALIDATION_TYPE_INTEGER = "Integer";
        public const string VALIDATION_TYPE_EMAIL = "Email";
        public const string VALIDATION_TYPE_DATETIME = "DateTime";
        public const string VALIDATION_TYPE_TIME = "Time";
        public const string VALIDATION_TYPE_ALPHANUMERIC = "AlphaNumeric";

        private Dictionary<int, BOAnswer> answers = new Dictionary<int, BOAnswer>();
        private ValidationTypes validationType = ValidationTypes.None;

        public bool IsAnswerRequired { get; set; }
        public Dictionary<int, BOAnswer> Answers { get { return answers; } set { answers = value; } }
        public ValidationTypes ValidationType { get { return validationType; } set { validationType = value; } }
        public int TimesAnswered { get; set; }

        public int? FirstAnswerKey
        {
            get
            {
                int? firstKey = null;
                foreach (int key in answers.Keys)
                {
                    firstKey = (int?)key;
                    break;
                }
                return firstKey;
            }
        }

        public int? LastAnswerKey
        {
            get
            {
                int? lastKey = null;
                foreach (int key in answers.Keys)
                {
                    lastKey = (int?)key;
                }
                return lastKey;
            }
        }
    }

    [Serializable]
    public class BOAnswer : BOElement
    {
        public const string ADDITIONAL_FIELD_TYPE_FILE = "File";
        public const string ADDITIONAL_FIELD_TYPE_TEXT = "Text";
        public const string ADDITIONAL_FIELD_TYPE_NONE = "None";

        private AnswerTypes answerType = AnswerTypes.SingleText;
        private AdditionalFieldTypes additionalFieldType = AdditionalFieldTypes.None;

        public BOAnswer() { }

        public BOAnswer(int id, int? parentId, int idx, string title, AnswerTypes answerType, int maxChars, int numberOfRows, AdditionalFieldTypes additionalFieldType, bool isFakeAnswer, decimal weight)
        {
            this.Id = id;
            this.ParentId = parentId;
            this.Title = title;

            this.AnswerType = answerType;
            this.MaxChars = (maxChars <= 0 ? (int?)null : maxChars);
            this.NumberOfRows = (numberOfRows <= 0 ? (int?)null : numberOfRows);
            this.AdditionalFieldType = additionalFieldType;
            this.IsFake = isFakeAnswer;
            this.Weight = weight;
        }

        public int? MaxChars { get; set; }
        public int? NumberOfRows { get; set; }
        public int? MaxFileSize { get; set; }
        public string AllowedMimeTypes { get; set; }
        public AnswerTypes AnswerType { get { return answerType; } set { answerType = value; } }
        public AdditionalFieldTypes AdditionalFieldType { get { return additionalFieldType; } set { additionalFieldType = value; } }
        public int TimesAnswered { get; set; }
        public double PercentageAnswered { get; set; }
        public double OverallPercentageAnswered { get; set; }
        public bool IsFake { get; set; }
    }

    [Serializable]
    public class BOFormSubmission
    {
        private Dictionary<int, BOSubmittedQuestion> submittedQuestions = new Dictionary<int, BOSubmittedQuestion>();

        public int FormId { get; set; }
        public int? Id { get; set; }
        public DateTime? Finished { get; set; }
        public Dictionary<int, BOSubmittedQuestion> SubmittedQuestions { get { return submittedQuestions; } set { submittedQuestions = value; } }

        public decimal WeightsSum
        {
            get
            {
                decimal sum = 0;
                foreach (var q in SubmittedQuestions.Values)
                {
                    foreach (var a in q.SubmittedAnswers.Values)
                    {
                        sum += a.Answer.Weight;
                    }
                }
                return sum;
            }
        }
    }

    [Serializable]
    public class BOSubmittedQuestion
    {
        private Dictionary<int, BOSubmittedAnswer> submittedAnswers = new Dictionary<int, BOSubmittedAnswer>();

        public int? SubmissionId { get; set; }
        public BOQuestion Question { get; set; }
        public Dictionary<int, BOSubmittedAnswer> SubmittedAnswers { get { return submittedAnswers; } set { submittedAnswers = value; } }
    }

    [Serializable]
    public class BOSubmittedAnswer
    {
        public int? SubmissionId { get; set; }
        public BOAnswer Answer { get; set; }
        public string SubmittedText { get; set; }
        public BOFile SubmittedFile { get; set; }
    }
}
