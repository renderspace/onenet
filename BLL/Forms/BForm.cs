using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Text;
using One.Net.BLL;
using One.Net.BLL.Web;
using NLog;

namespace One.Net.Forms
{
    public class BForm
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();

        private static readonly DbForm formsDb = new DbForm();
        private static readonly BInternalContent contentB = new BInternalContent();
        private static readonly BFileSystem fileB = new BFileSystem();

        /// <summary>
        /// Change method, used to change form structure and underlying detail.
        /// Content ids for each form, its sections, their questions and their answers are retrieved from db if they exist.
        /// Then change method is called on form object content.
        /// Then change method is called on form object.
        /// Then change method is called on each section, its questions and their answers
        /// </summary>
        /// <param name="form"></param>
        public void Change(BOForm form)
        {
            if (form.Id.HasValue)
            {
                BOForm existingForm = this.GetUnCached(form.Id.Value);

                // synchronise existing form object and current form object ( delete pending delete sections, questions, answers ).
                if (existingForm != null && existingForm.Id.HasValue)
                {
                    foreach (BOSection sec in existingForm.Sections.Values)
                    {
                        if (form.Sections.ContainsKey(sec.Id.Value))
                        {
                            foreach (BOQuestion ques in sec.Questions.Values)
                            {
                                if (form.Sections[sec.Id.Value].Questions.ContainsKey(ques.Id.Value))
                                {
                                    if (form.SubmissionCount == 0)
                                    {
                                        // delete all answers... since they are recreated fruther down in this method
                                        foreach (BOAnswer ans in ques.Answers.Values)
                                        {
                                            formsDb.DeleteAnswer(ans.Id.Value);
                                        }
                                    }
                                }
                                else
                                {
                                    DeleteQuestion(ques);
                                }
                            }
                        }
                        else
                        {
                            DeleteSection(sec);
                        }
                    }
                }
            }

            formsDb.Change(form);

            // store all sub data of form object
            foreach (BOSection section in form.Sections.Values)
            {
                section.ParentId = form.Id;
                formsDb.ChangeSection(section);

                foreach (BOQuestion question in section.Questions.Values)
                {
                    question.ParentId = section.Id;
                    formsDb.ChangeQuestion(question);

                    if (form.SubmissionCount == 0)
                    {
                        foreach (BOAnswer answer in question.Answers.Values)
                        {
                            answer.ParentId = question.Id;
                            formsDb.InsertAnswer(answer);
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Deletes form entry, and it's underlying detail
        /// </summary>
        /// <param name="formId"></param>
        public bool DeleteForm(int formId)
        {
            BOForm form = this.GetUnCached(formId);
            return DeleteForm(form);
        }

        private static void DeleteSection(BOSection section)
        {
            if (section.Id.HasValue)
            {
                foreach (BOQuestion question in section.Questions.Values)
                {
                    if (question.Id.HasValue)
                    {
                        foreach (BOAnswer answer in question.Answers.Values)
                        {
                            if (answer.Id.HasValue)
                            {
                                formsDb.DeleteAnswer(answer.Id.Value);
                            }
                        }

                        formsDb.DeleteQuestion(question.Id.Value);
                    }
                }

                formsDb.DeleteSection(section.Id.Value);
            }
        }

        private static void DeleteQuestion(BOQuestion question)
        {
            if (question.Id.HasValue)
            {
                foreach (BOAnswer answer in question.Answers.Values)
                {
                    if (answer.Id.HasValue)
                    {
                        formsDb.DeleteAnswer(answer.Id.Value);
                    }
                }

                formsDb.DeleteQuestion(question.Id.Value);
            }
        }

        /// <summary>
        /// Delete entire form from database
        /// to get it ready for saving
        /// </summary>
        /// <param name="form"></param>
        private static bool DeleteForm(BOForm form)
        {   
            if (form == null || !form.Id.HasValue)
                return false;
            foreach (BOSection section in form.Sections.Values)
            {
                if (section.Id.HasValue)
                {
                    foreach (BOQuestion question in section.Questions.Values)
                    {
                        if (question.Id.HasValue)
                        {
                            foreach (BOAnswer answer in question.Answers.Values)
                            {
                                if (answer.Id.HasValue)
                                {
                                    formsDb.DeleteAnswer(answer.Id.Value);
                                }
                            }

                            formsDb.DeleteQuestion(question.Id.Value);
                        }
                    }
                    formsDb.DeleteSection(section.Id.Value);
                }
            }
            formsDb.DeleteForm(form.Id.Value);
            return true;
        }

        /// <summary>
        /// Data retrieval method, retrieves form detail including underlying BOInternalContent details.
        /// Retrieves form sections, their questions and their answers.
        /// Method is fully cached.
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public BOForm Get(int formId)
        {
            return GetUnCached(formId);
        }

        /// <summary>
        /// Data retrieval method, retrieves form detail including underlying BOInternalContent details.
        /// Retrieves form sections, their questions and their answers.
        /// Method is uncached.
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public BOForm GetUnCached(int formId)
        {
            BOForm form = formsDb.Get(formId);

            if (form != null)
            {
                form.Sections = formsDb.ListSections(formId);

                foreach (BOSection section in form.Sections.Values)
                {
                    section.Questions = formsDb.ListQuestions(section.Id.Value);

                    foreach (BOQuestion question in section.Questions.Values)
                    {
                        question.Answers = formsDb.ListAnswers(question.Id.Value);

                        foreach (BOAnswer answer in question.Answers.Values)
                        {
                            if ( form.SubmissionCount > 0 )
                            {
                                answer.PercentageAnswered = ((double)answer.TimesAnswered / (double)question.TimesAnswered) * (double)100;
                                answer.OverallPercentageAnswered = ((double)answer.TimesAnswered / (double)form.SubmissionCount) * (double)100;
                            }
                        }
                    }
                }
            }

            return form;
        }
 
        /// <summary>
        /// Lists all forms based on provided input parameters. Populates internal detail for each form.
        /// Parameters yet to be determined.
        /// Method is uncached.
        /// </summary>
        /// <returns></returns>
        public PagedList<BOForm> ListUnCached(ListingState state)
        {
            PagedList<BOForm> forms = formsDb.List(state);
            return forms;
        }

        /// <summary>
        /// Retrive submission data.
        /// Populate related submission of form object and its submitted answers
        /// </summary>
        /// <param name="submissionId"></param>
        /// <returns></returns>
        public BOFormSubmission GetFormSubmission(int submissionId)
        {
            BOFormSubmission submission = formsDb.GetFormSubmission(submissionId);
            BOForm form = this.Get(submission.FormId);
            submission.SubmittedQuestions = new Dictionary<int, BOSubmittedQuestion>();

            Dictionary<int, BOSubmittedAnswer> submittedAnswers = formsDb.ListSubmittedAnswers(submissionId);

            foreach (BOSubmittedAnswer submittedAnswer in submittedAnswers.Values)
            {
                if (submittedAnswer.SubmittedFile != null && submittedAnswer.SubmittedFile.Id.HasValue)
                {
                    submittedAnswer.SubmittedFile = fileB.Get(submittedAnswer.SubmittedFile.Id.Value);
                }

                submittedAnswer.Answer = BOForm.FindAnswer(form, submittedAnswer.Answer.Id.Value);

                if (!submission.SubmittedQuestions.ContainsKey(submittedAnswer.Answer.ParentId.Value))
                {
                    submission.SubmittedQuestions.Add(submittedAnswer.Answer.ParentId.Value, new BOSubmittedQuestion());
                }

                submission.SubmittedQuestions[submittedAnswer.Answer.ParentId.Value].SubmissionId = submissionId;
                submission.SubmittedQuestions[submittedAnswer.Answer.ParentId.Value].SubmittedAnswers.Add(submittedAnswer.Answer.Id.Value, submittedAnswer);
                submission.SubmittedQuestions[submittedAnswer.Answer.ParentId.Value].Question = BOForm.FindQuestion(form, submittedAnswer.Answer.ParentId.Value);

                submittedAnswer.Answer.PercentageAnswered = ((double)submittedAnswer.Answer.TimesAnswered / (double)submission.SubmittedQuestions[submittedAnswer.Answer.ParentId.Value].Question.TimesAnswered) * (double)100;
            }

            return submission;
        }

        /// <summary>
        /// List all submissions based on given form id.
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public PagedList<BOFormSubmission> ListFormSubmissions(int formId, ListingState state)
        {
            PagedList<BOFormSubmission> submissions = formsDb.ListFormSubmissions(formId, state);
            BOForm form = this.Get(formId);

            foreach (BOFormSubmission submission in submissions)
            {
                Dictionary<int, BOSubmittedAnswer> submittedAnswers = formsDb.ListSubmittedAnswers(submission.Id.Value);

                foreach (BOSubmittedAnswer submittedAnswer in submittedAnswers.Values)
                {
                    if (submittedAnswer.SubmittedFile != null && submittedAnswer.SubmittedFile.Id.HasValue)
                    {
                        submittedAnswer.SubmittedFile = fileB.Get(submittedAnswer.SubmittedFile.Id.Value);
                    }
                    submittedAnswer.Answer = BOForm.FindAnswer(form, submittedAnswer.Answer.Id.Value);
                    if (submittedAnswer.Answer != null)
                    {
                        if (!submission.SubmittedQuestions.ContainsKey(submittedAnswer.Answer.ParentId.Value))
                        {
                            submission.SubmittedQuestions[submittedAnswer.Answer.ParentId.Value] = new BOSubmittedQuestion();
                        }

                        submission.SubmittedQuestions[submittedAnswer.Answer.ParentId.Value].SubmissionId = submission.Id.Value;
                        submission.SubmittedQuestions[submittedAnswer.Answer.ParentId.Value].SubmittedAnswers.Add(submittedAnswer.Answer.Id.Value, submittedAnswer);
                        submission.SubmittedQuestions[submittedAnswer.Answer.ParentId.Value].Question = BOForm.FindQuestion(form, submittedAnswer.Answer.ParentId.Value);

                        submittedAnswer.Answer.PercentageAnswered = ((double)submittedAnswer.Answer.TimesAnswered / (double)submission.SubmittedQuestions[submittedAnswer.Answer.ParentId.Value].Question.TimesAnswered) * (double)100;
                    }
                }
            }

            return submissions;
        }

        /// <summary>
        /// Change form submission.
        /// Store all user selected data ( selected answers ).
        /// Sends answers to SendToAddresses defined in BONForm object, if SendToAddresses is not null
        /// </summary>
        /// <param name="submission"></param>
        public bool ProcessFormSubmission(BOFormSubmission submission, BOForm form)
        {
            bool isSubmissionComplete = false;
            log.Info("---------------- processing form ----------------");
            if (submission != null && form != null)
            {

                formsDb.InsertFormSubmission(submission);
                log.Info("FormSubmission saved to Database");

                foreach (BOSubmittedQuestion submittedQuestion in submission.SubmittedQuestions.Values)
                {
                    foreach (BOSubmittedAnswer submittedAnswer in submittedQuestion.SubmittedAnswers.Values)
                    {
                        submittedAnswer.SubmissionId = submission.Id;

                        if (submittedAnswer.SubmittedFile != null)
                        {
                            fileB.Change(submittedAnswer.SubmittedFile);
                            log.Info("SubmittedFile saved to Database: " + submittedAnswer.SubmittedFile.Name);
                        }

                        formsDb.InsertSubmittedAnswer(submittedAnswer);
                        log.Debug("SubmittedAnswer saved to Database");
                    }
                }
                isSubmissionComplete = true; // only set to true if all is ok

                // Email sending part
                if (isSubmissionComplete)
                {
                    var formBody = new StringBuilder();
                    var message = new MailMessage();
                    var encoding = Encoding.UTF8;

                    formBody.Append("\n\r");
                    formBody.Append("Form : " + form.Title + "\n\r");
                    formBody.Append("Finished at : " + submission.Finished.Value.ToString() + "\n\r\n\r");

                    foreach (BOSection section in form.Sections.Values)
                    {
                        formBody.Append("--------------- " + section.Title + " ---------------\n\r\n\r");
                        foreach (BOQuestion question in section.Questions.Values)
                        {
                            formBody.Append("" + question.Title + " ");

                            if (submission.SubmittedQuestions.ContainsKey(question.Id.Value) &&
                                submission.SubmittedQuestions[question.Id.Value] != null)
                            {
                                BOSubmittedQuestion submittedQuestion = submission.SubmittedQuestions[question.Id.Value];
                                foreach (BOAnswer answer in question.Answers.Values)
                                {
                                    if (submittedQuestion.SubmittedAnswers.ContainsKey(answer.Id.Value) &&
                                        submittedQuestion.SubmittedAnswers[answer.Id.Value] != null)
                                    {
                                        if (!answer.IsFake)
                                        {
                                            formBody.Append("" + answer.Title);
                                            if (submittedQuestion.SubmittedAnswers[answer.Id.Value].SubmittedText !=
                                                null)
                                            {
                                                formBody.Append(" " +
                                                                submittedQuestion.SubmittedAnswers[answer.Id.Value].
                                                                    SubmittedText + "");
                                            }
                                            if (submittedQuestion.SubmittedAnswers[answer.Id.Value].SubmittedFile !=
                                                null &&
                                                submittedQuestion.SubmittedAnswers[answer.Id.Value].SubmittedFile.Id.
                                                    HasValue)
                                            {
                                                BOFile submittedFile =
                                                    submittedQuestion.SubmittedAnswers[answer.Id.Value].SubmittedFile;
                                                MemoryStream fileStream = new MemoryStream(submittedFile.File);
                                                Attachment att = new Attachment(fileStream, submittedFile.Name);
                                                att.ContentType.MediaType = submittedFile.MimeType;
                                                att.ContentType.Name = submittedFile.Name;
                                                message.Attachments.Add(att);
                                                formBody.Append("\n\r# Submitted File : " + submittedFile.Id.Value + "\n\r");
                                            }
                                            formBody.Append("\n\r");
                                        }
                                    }
                                }
                            }
                            formBody.Append("\n\r");
                        }
                        formBody.Append("\n\r");
                    }
                    formBody.Append("\n\r");

                    message.IsBodyHtml = false;


                    message.Body = formBody.ToString();
                    message.BodyEncoding = encoding;
                    message.SubjectEncoding = encoding;

                    if (form.SendToAddresses != null)
                    {
                        foreach (string s in form.SendToAddresses)
                        {
                            if (!string.IsNullOrEmpty(s.Trim()))
                                message.To.Add(new MailAddress(s));
                        }
                    }

                    string formTitle = String.IsNullOrEmpty(form.SubTitle) ? form.Title : form.SubTitle;

                    message.Subject = "Form [" + form.Id + "] - " +
                                      (isSubmissionComplete ? formTitle : "ERROR occured while storing to DB");

                    log.Info(message.Subject);
                    log.Info(message.Body);

                    if (form.SendToAddresses != null)
                    {
                        try
                        {
                            // NOTE : we are using the default from address as defined in web.config <smpt from="">
                            var client = new SmtpClient();
                            client.Send(message);
                            log.Info("---------------- form message sent to: " + form.SendToString);
                        }
                        catch (Exception ex)
                        {
                            log.Fatal(ex, "** Sending to " + form.SendToAddresses + " failed **");
                        }
                    }
                }
            }
            log.Info("---------------- form " + (isSubmissionComplete ? "processed sucessfully" : "FAILED") + " ----------------");
            return isSubmissionComplete;
        }

        public static List<string> ListFormTypes()
        {
            List<string> types = new List<string>();
            types.Add(BOForm.FORM_TYPE_QUESTIONAIRE);
            return types;
        }

        public static List<string> ListSectionTypes()
        {
            List<string> types = new List<string>();

            types.Add(BOSection.SECTION_TYPE_MULTI_PAGE);
            types.Add(BOSection.SECTION_TYPE_SINGLE_PAGE);

            return types;
        }

        public static List<string> ListAdditionalFieldTypes()
        {
            List<string> types = new List<string>();

            types.Add(BOAnswer.ADDITIONAL_FIELD_TYPE_FILE);
            types.Add(BOAnswer.ADDITIONAL_FIELD_TYPE_TEXT);
            types.Add(BOAnswer.ADDITIONAL_FIELD_TYPE_NONE);

            return types;
        }

        public BOCategory GetUploadFolder(int folderId)
        {
            return fileB.GetFolder(folderId);
        }
    }
}
