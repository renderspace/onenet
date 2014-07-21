using System;
using System.Collections.Generic;

namespace One.Net.BLL.Forms
{
    public enum FormTypes { Poll = 1, Questionaire }
    public enum AnswerTypes { Checkbox = 1, Radio, DropDown, SingleText, SingleFile }
    public enum AdditionalFieldTypes { Text = 1, File, None }
    public enum SectionTypes { SinglePage = 1, MultiPage }
    public enum ValidationTypes { None = 1, AlphaNumeric, Numeric, Integer, Email, DateTime, Time, Captcha, VAT, Telephone }

    [Serializable]
    public abstract class BOElement : BOInternalContent
    {
        private int? id = null;
        private int? parentId = null;

        public int? ParentId { get { return parentId; } set { parentId = value; } }
        public int? Id { get { return id; } set { id = value; } }
    }

    [Serializable]
    public class BOForm : BOElement
    {
        public const string FILE_USE_FORMS = "nform";

        public const string FORM_TYPE_POLL = "Poll";
        public const string FORM_TYPE_QUESTIONAIRE = "Questionaire";

        private bool allowMultipleSubmissions = false;
        private bool allowModifyInSubmission = false;
        private int submissionCount = 0;
        private string sendToString = "";
        private string completionRedirect = "";
        private FormTypes formType = FormTypes.Poll;
        private Dictionary<int, BOSection> sections = new Dictionary<int, BOSection>();
        private DateTime? firstSubmissionDate = null;
        private DateTime? lastSubmissionDate = null;

        public bool AllowMultipleSubmissions { get { return allowMultipleSubmissions; } set { allowMultipleSubmissions = value; } }
        public bool AllowModifyInSubmission { get { return allowModifyInSubmission; } set { allowModifyInSubmission = value; } }
        public int SubmissionCount { get { return this.submissionCount; } set { this.submissionCount = value; } }
        public FormTypes FormType { get { return formType; } set { formType = value; } }
        public Dictionary<int, BOSection> Sections { get { return sections; } set { sections = value; } }
        public string SendToString { get { return sendToString; } set { sendToString = value; } }

        public string CompletionRedirect
        {
            get { return completionRedirect; }
            set { completionRedirect = value; }
        }

        public string[] SendToAddresses { get { return (string.IsNullOrEmpty(sendToString) ? null : sendToString.Split(';')); } }
        public DateTime? FirstSubmissionDate { get { return firstSubmissionDate; } set { firstSubmissionDate = value; } }
        public DateTime? LastSubmissionDate { get { return lastSubmissionDate; } set { lastSubmissionDate = value; } }

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


    }

    [Serializable]
    public class BOSection : BOElement
    {
        public const string SECTION_TYPE_MULTI_PAGE = "MultiPage";
        public const string SECTION_TYPE_SINGLE_PAGE = "SinglePage";

        private int idx;
        private string onClientClick = "";
        private SectionTypes sectionType = SectionTypes.SinglePage;
        private Dictionary<int, BOQuestion> questions = new Dictionary<int, BOQuestion>();

        public string OnClientClick
        {
            get { return onClientClick; }
            set { onClientClick = value; }
        }
        public int Idx { get { return idx; } set { idx = value; } }
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
        public const string VALIDATION_TYPE_CAPTCHA = "Captcha";
        public const string VALIDATION_TYPE_VAT = "VAT";
        public const string VALIDATION_TYPE_TELEPHONE = "Telephone";

        private int idx;
        private bool isAnswerRequired = false;
        private Dictionary<int, BOAnswer> answers = new Dictionary<int, BOAnswer>();
        private ValidationTypes validationType = ValidationTypes.None;
        private int timesAnswered = 0; // used when displaying poll results

        public int Idx { get { return idx; } set { idx = value; } }
        public bool IsAnswerRequired { get { return isAnswerRequired; } set { isAnswerRequired = value; } }
        public Dictionary<int, BOAnswer> Answers { get { return answers; } set { answers = value; } }
        public ValidationTypes ValidationType { get { return validationType; } set { validationType = value; } }
        public int TimesAnswered { get { return timesAnswered; } set { timesAnswered = value; } }

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
        public const string ANSWER_TYPE_CHECKBOX = "Checkbox";
        public const string ANSWER_TYPE_SINGLE_TEXT = "SingleText";
        public const string ANSWER_TYPE_SINGLE_FILE = "SingleFile";
        public const string ANSWER_TYPE_RADIO = "Radio";
        public const string ANSWER_TYPE_DROPDOWN = "DropDown";
        public const string ADDITIONAL_FIELD_TYPE_FILE = "File";
        public const string ADDITIONAL_FIELD_TYPE_TEXT = "Text";
        public const string ADDITIONAL_FIELD_TYPE_NONE = "None";

        private int idx;
        private int? maxChars = null;
        private int? numberOfRows = null;
        private int? maxFileSize = null;
        private string allowedMimeTypes = "";
        private AnswerTypes answerType = AnswerTypes.SingleText;
        private AdditionalFieldTypes additionalFieldType = AdditionalFieldTypes.None;
        private int timesAnswered = 0; // used when displaying poll results, and aggregate results
        private double percentageAnswered = 0; // used when displaying poll results, and aggregate results
        private double overallPercentageAnswered = 0; // used when displaying poll results, and aggregate results
        private bool isFake = false;

        public BOAnswer() { }

        public BOAnswer(int id, int? parentId, int idx, string title, string subtitle, string teaser,
            string html, int languageId, AnswerTypes answerType, int maxChars, int numberOfRows, AdditionalFieldTypes additionalFieldType, bool isFakeAnswer)
        {
            this.Id = id;
            this.ParentId = parentId;
            this.Idx = idx;
            this.Title = title;
            this.SubTitle = subtitle;
            this.Teaser = teaser;
            this.Html = html;
            this.LanguageId = languageId;
            this.AnswerType = answerType;
            this.MaxChars = (maxChars <= 0 ? (int?)null : maxChars);
            this.NumberOfRows = (numberOfRows <= 0 ? (int?)null : numberOfRows);
            this.AdditionalFieldType = additionalFieldType;
            this.IsFake = isFake;
        }

        public int Idx { get { return idx; } set { idx = value; } }
        public int? MaxChars { get { return maxChars; } set { maxChars = value; } }
        public int? NumberOfRows { get { return numberOfRows; } set { numberOfRows = value; } }
        public int? MaxFileSize { get { return maxFileSize; } set { maxFileSize = value; } }
        public string AllowedMimeTypes { get { return allowedMimeTypes; } set { allowedMimeTypes = value; } }
        public AnswerTypes AnswerType { get { return answerType; } set { answerType = value; } }
        public AdditionalFieldTypes AdditionalFieldType { get { return additionalFieldType; } set { additionalFieldType = value; } }
        public int TimesAnswered { get { return timesAnswered; } set { timesAnswered = value; } }
        public double PercentageAnswered { get { return percentageAnswered; } set { percentageAnswered = value; } }
        public double OverallPercentageAnswered { get { return overallPercentageAnswered; } set { overallPercentageAnswered = value; } }
        public bool IsFake { get { return isFake; } set { isFake = value; } }
    }

    [Serializable]
    public class BOFormSubmission
    {
        private int formId;
        private int? id = null;
        private DateTime? finished = null;
        private Dictionary<int, BOSubmittedQuestion> submittedQuestions = new Dictionary<int, BOSubmittedQuestion>();

        public int FormId { get { return formId; } set { formId = value; } }
        public int? Id { get { return id; } set { id = value; } }
        public DateTime? Finished { get { return finished; } set { finished = value; } }
        public Dictionary<int, BOSubmittedQuestion> SubmittedQuestions { get { return submittedQuestions; } set { submittedQuestions = value; } }
    }

    [Serializable]
    public class BOSubmittedQuestion
    {
        private int? submissionId;
        private BOQuestion question = null;
        private Dictionary<int, BOSubmittedAnswer> submittedAnswers = new Dictionary<int, BOSubmittedAnswer>();

        public int? SubmissionId { get { return submissionId; } set { submissionId = value; } }
        public BOQuestion Question { get { return question; } set { question = value; } }
        public Dictionary<int, BOSubmittedAnswer> SubmittedAnswers { get { return submittedAnswers; } set { submittedAnswers = value; } }
    }

    [Serializable]
    public class BOSubmittedAnswer
    {
        private int? submissionId;
        private BOAnswer answer = null;
        private string submittedText;
        private BOFile submittedFile;

        public int? SubmissionId { get { return submissionId; } set { submissionId = value; } }
        public BOAnswer Answer { get { return answer; } set { answer = value; } }
        public string SubmittedText { get { return submittedText; } set { submittedText = value; } }
        public BOFile SubmittedFile { get { return submittedFile; } set { submittedFile = value; } }
    }
}
