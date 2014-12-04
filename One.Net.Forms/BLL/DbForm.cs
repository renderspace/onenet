using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using One.Net.BLL;
using MsSqlDBUtility;
using System.Threading;
using NLog;

namespace One.Net.Forms.BLL
{
    public class DbForm 
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();

        public void Change(BOForm form)
        {
            SqlParameter[] paramsToPass = new SqlParameter[10];
            paramsToPass[0] = SqlHelper.GetNullable("@Id", form.Id);
            paramsToPass[0].Direction = ParameterDirection.InputOutput;
            paramsToPass[0].DbType = DbType.Int32;

            string formType = "";
            switch (form.FormType)
            {
                case FormTypes.Poll: formType = BOForm.FORM_TYPE_POLL; break;
                case FormTypes.Questionaire: formType = BOForm.FORM_TYPE_QUESTIONAIRE; break;
            }

            paramsToPass[1] = new SqlParameter("@FormType", formType);
            paramsToPass[2] = SqlHelper.GetNullable("@SendTo", form.SendToString); 
            paramsToPass[3] = new SqlParameter("@AllowModifyInSubmission", form.AllowModifyInSubmission);
            paramsToPass[4] = new SqlParameter("@AllowMultipleSubmissions", form.AllowMultipleSubmissions);
            paramsToPass[5] = string.IsNullOrEmpty(form.CompletionRedirect) ? new SqlParameter("@CompletionRedirect", DBNull.Value) : new SqlParameter("@CompletionRedirect", form.CompletionRedirect);
            paramsToPass[6] = new SqlParameter("@Title", form.Title);
            paramsToPass[7] = string.IsNullOrEmpty(form.Description) ? new SqlParameter("@Description", DBNull.Value) : new SqlParameter("@Description", form.Description);
            paramsToPass[8] = new SqlParameter("@SubTitle", form.SubTitle);
            paramsToPass[9] = new SqlParameter("@ThankYouNote", form.ThankYouNote);

            string sql;

            if (form.Id.HasValue)
            {
                sql = @"UPDATE [dbo].[nform] SET form_type=@FormType, send_to=@SendTo, allow_modify_in_submission=@AllowModifyInSubmission, allow_multiple_submissions=@AllowMultipleSubmissions, completion_redirect=@CompletionRedirect WHERE id=@Id";
            }
            else
            {
                sql = @"INSERT INTO [dbo].[nform]
                           (form_type, send_to, allow_modify_in_submission, allow_multiple_submissions, completion_redirect)
                           VALUES (@FormType, @SendTo, @AllowModifyInSubmission, @AllowMultipleSubmissions, @CompletionRedirect ); SET @Id=SCOPE_IDENTITY();";
            }

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMainCustom, CommandType.Text, sql, paramsToPass);

            if (!form.Id.HasValue)
            {
                form.Id = Int32.Parse(paramsToPass[0].Value.ToString());
            }
        }

        public void ChangeSection(BOSection section)
        {
            SqlParameter[] paramsToPass = new SqlParameter[7];
            paramsToPass[0] = SqlHelper.GetNullable("@Id", section.Id);
            paramsToPass[0].Direction = ParameterDirection.InputOutput;
            paramsToPass[0].DbType = DbType.Int32;

            string sectionType = "";
            switch (section.SectionType)
            {
                case SectionTypes.MultiPage: sectionType = BOSection.SECTION_TYPE_MULTI_PAGE; break;
                case SectionTypes.SinglePage: sectionType = BOSection.SECTION_TYPE_SINGLE_PAGE; break;
            }

            paramsToPass[1] = new SqlParameter("@SectionType", sectionType);
            paramsToPass[2] = new SqlParameter("@FormId", section.ParentId.Value);
            paramsToPass[3] = new SqlParameter("@Idx", section.Idx);

            paramsToPass[4] = string.IsNullOrEmpty(section.OnClientClick) ?  new SqlParameter("@OnClientClick", DBNull.Value) : new SqlParameter("@OnClientClick", section.OnClientClick);
            paramsToPass[4].DbType = DbType.String;

            paramsToPass[5] = new SqlParameter("@Title", section.Title);
            paramsToPass[6] = string.IsNullOrEmpty(section.Description) ? new SqlParameter("@Description", DBNull.Value) : new SqlParameter("@Description", section.Description);

            string sql = "";

            if (section.Id.HasValue)
            {
                sql = @"UPDATE nform_section SET section_type=@SectionType, nform_fk_id=@FormId, idx=@Idx, on_client_click=@OnClientClick WHERE id=@Id";
            }
            else
            {
                sql = @"INSERT INTO nform_section
                    (section_type, nform_fk_id, idx, on_client_click)
                    VALUES (@SectionType, @FormId, @Idx, @OnClientClick); SET @Id=SCOPE_IDENTITY();";
            }

            var recordsUpdated = SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMainCustom, CommandType.Text, sql, paramsToPass);

            if (!section.Id.HasValue)
            {
                section.Id = Int32.Parse(paramsToPass[0].Value.ToString());
            }
        }

        public void ChangeQuestion(BOQuestion question)
        {
            SqlParameter[] paramsToPass = new SqlParameter[7];
            paramsToPass[0] = SqlHelper.GetNullable("@Id", question.Id);
            paramsToPass[0].Direction = ParameterDirection.InputOutput;
            paramsToPass[0].DbType = DbType.Int32;

            paramsToPass[1] = new SqlParameter("@IsAnswerRequired", question.IsAnswerRequired);
            paramsToPass[2] = new SqlParameter("@SectionId", question.ParentId.Value);
            paramsToPass[3] = new SqlParameter("@Idx", question.Idx);

            string validationType = "";
            switch (question.ValidationType)
            {
                case ValidationTypes.None: validationType = BOQuestion.VALIDATION_TYPE_NONE; break;
                case ValidationTypes.Numeric: validationType = BOQuestion.VALIDATION_TYPE_NUMERIC; break;
                case ValidationTypes.Integer: validationType = BOQuestion.VALIDATION_TYPE_INTEGER; break;
                case ValidationTypes.Email: validationType = BOQuestion.VALIDATION_TYPE_EMAIL; break;
                case ValidationTypes.DateTime: validationType = BOQuestion.VALIDATION_TYPE_DATETIME; break;
                case ValidationTypes.Time: validationType = BOQuestion.VALIDATION_TYPE_TIME; break;
                case ValidationTypes.Captcha: validationType = BOQuestion.VALIDATION_TYPE_CAPTCHA; break;
            }

            paramsToPass[4] = new SqlParameter("@ValidationType", validationType);
            paramsToPass[5] = new SqlParameter("@Title", question.Title);
            paramsToPass[6] = string.IsNullOrEmpty(question.Description) ? new SqlParameter("@Description", DBNull.Value) : new SqlParameter("@Description", question.Description);

            string sql = "";

            if (question.Id.HasValue)
            {
                sql = @"UPDATE [dbo].[nform_question] SET is_answer_required=@IsAnswerRequired,
                        nform_section_fk_id=@SectionId, idx=@Idx, validation_type=@ValidationType WHERE id=@Id";
            }
            else
            {
                sql = @"INSERT INTO nform_question
                        (is_answer_required, nform_section_fk_id, idx, validation_type)
                        VALUES (@IsAnswerRequired, @SectionId, @Idx, @ValidationType); SET @Id=SCOPE_IDENTITY();";
            }

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMainCustom, CommandType.Text, sql, paramsToPass);

            if (!question.Id.HasValue)
            {
                question.Id = Int32.Parse(paramsToPass[0].Value.ToString());
            }
        }

        public void InsertAnswer(BOAnswer answer)
        {
            SqlParameter[] paramsToPass = new SqlParameter[12];
            paramsToPass[0] = new SqlParameter("@Id", answer.Id);
            paramsToPass[0].Direction = ParameterDirection.Output;
            paramsToPass[0].DbType = DbType.Int32;

            string answerType = "";
            switch (answer.AnswerType)
            {
                case AnswerTypes.Checkbox: answerType = BOAnswer.ANSWER_TYPE_CHECKBOX; break;
                case AnswerTypes.DropDown: answerType = BOAnswer.ANSWER_TYPE_DROPDOWN; break;
                case AnswerTypes.Radio: answerType = BOAnswer.ANSWER_TYPE_RADIO; break;
                case AnswerTypes.SingleFile: answerType = BOAnswer.ANSWER_TYPE_SINGLE_FILE; break;
                case AnswerTypes.SingleText: answerType = BOAnswer.ANSWER_TYPE_SINGLE_TEXT; break;
            }
            paramsToPass[1] = new SqlParameter("@AnswerType", answerType);

            string additionalFieldType = "";
            switch (answer.AdditionalFieldType)
            {
                case AdditionalFieldTypes.File: additionalFieldType = BOAnswer.ADDITIONAL_FIELD_TYPE_FILE; break;
                case AdditionalFieldTypes.Text: additionalFieldType = BOAnswer.ADDITIONAL_FIELD_TYPE_TEXT; break;
                case AdditionalFieldTypes.None: additionalFieldType = BOAnswer.ADDITIONAL_FIELD_TYPE_NONE; break;
            }
            paramsToPass[2] = new SqlParameter("@AdditionalFieldType", additionalFieldType);

            paramsToPass[3] = new SqlParameter("@QuestionId", answer.ParentId.Value);
            paramsToPass[4] = new SqlParameter("@Idx", answer.Idx);
            paramsToPass[5] = (answer.MaxChars.HasValue && answer.MaxChars.Value > 0 ? new SqlParameter("@MaxChars", answer.MaxChars.Value) : new SqlParameter("@MaxChars", DBNull.Value));
            paramsToPass[6] = (answer.NumberOfRows.HasValue && answer.NumberOfRows.Value > 0 ? new SqlParameter("@NumberOfRows", answer.NumberOfRows.Value) : new SqlParameter("@NumberOfRows", DBNull.Value));
            paramsToPass[7] = (answer.MaxFileSize.HasValue && answer.MaxFileSize.Value > 0 ? new SqlParameter("@MaxFileSize", answer.MaxFileSize.Value) : new SqlParameter("@MaxFileSize", DBNull.Value));
            paramsToPass[8] = SqlHelper.GetNullable("@AllowedMimeTypes", answer.AllowedMimeTypes); 
            paramsToPass[9] = new SqlParameter("@IsFake", answer.IsFake);
            paramsToPass[10] = new SqlParameter("@Title", answer.Title);
            paramsToPass[11] = string.IsNullOrEmpty(answer.Description) ? new SqlParameter("@Description", DBNull.Value) : new SqlParameter("@Description", answer.Description);

            string sql = "";

            sql = @"INSERT INTO nform_answer
                        (answer_type, additional_field_type, nform_question_fk_id, idx, max_chars, number_of_rows, max_file_size, allowed_mime_types, is_fake)
                        VALUES (@AnswerType, @AdditionalFieldType, @QuestionId, @Idx, @MaxChars, @NumberOfRows, @MaxFileSize, @AllowedMimeTypes, @IsFake); SET @Id=SCOPE_IDENTITY();";

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMainCustom, CommandType.Text, sql, paramsToPass);

            answer.Id = Int32.Parse(paramsToPass[0].Value.ToString());
        }

        public void DeleteForm(int formId)
        {
            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@formId", formId);

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMainCustom, CommandType.Text,
                @"DELETE FROM nform WHERE id=@formId", paramsToPass);
        }

        public void DeleteSection(int sectionId)
        {
            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@sectionId", sectionId);

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMainCustom, CommandType.Text,
                @"DELETE FROM nform_section WHERE id=@sectionId", paramsToPass);
        }

        public void DeleteQuestion(int questionId)
        {
            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@questionId", questionId);

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMainCustom, CommandType.Text,
                @"DELETE FROM nform_question WHERE id=@questionId", paramsToPass);
        }

        public void DeleteAnswer(int answerId)
        {
            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@answerId", answerId);

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMainCustom, CommandType.Text,
                @"DELETE FROM nform_answer WHERE id=@answerId", paramsToPass);
        }

        public void InsertFormSubmission(BOFormSubmission submission)
        {
            SqlParameter[] paramsToPass = new SqlParameter[3];
            paramsToPass[0] = new SqlParameter("@Id", submission.Id);
            paramsToPass[1] = new SqlParameter("@FormId", submission.FormId);
            paramsToPass[2] = new SqlParameter("@Finished", submission.Finished);
            paramsToPass[0].Direction = ParameterDirection.Output;
            paramsToPass[0].DbType = DbType.Int32;

            string sql = @"INSERT INTO nform_submission
                           (nform_fk_id, finished)
                           VALUES (@FormId, @Finished); SET @Id=SCOPE_IDENTITY();";

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMainCustom, CommandType.Text,
                sql, paramsToPass);

            submission.Id = Int32.Parse(paramsToPass[0].Value.ToString());
        }

        public void InsertSubmittedAnswer(BOSubmittedAnswer submittedAnswer)
        {
            SqlParameter[] paramsToPass = new SqlParameter[4];
            paramsToPass[0] = new SqlParameter("@AnswerId", submittedAnswer.Answer.Id.Value);
            paramsToPass[1] = new SqlParameter("@SubmissionId", submittedAnswer.SubmissionId.Value);
            paramsToPass[2] = SqlHelper.GetNullable("SubmittedText", submittedAnswer.SubmittedText); 
            paramsToPass[3] = (submittedAnswer.SubmittedFile != null && submittedAnswer.SubmittedFile.Id.HasValue ? new SqlParameter("@SubmittedFileId", submittedAnswer.SubmittedFile.Id.Value) : new SqlParameter("@SubmittedFileId", DBNull.Value));

            string sql = @"INSERT INTO nform_submitted_answer
                           (nform_answer_fk_id, nform_submission_fk_id, submitted_text, files_fk_id)
                           VALUES (@AnswerId, @SubmissionId, @SubmittedText, @SubmittedFileId)";

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMainCustom, CommandType.Text,
                sql, paramsToPass);
        }

        public BOForm Get(int formId)
        {
            BOForm form = null;

            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@formId", formId);

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMainCustom, CommandType.Text,
                @"SELECT f.id form_id, f.form_type, f.send_to, 
                  (SELECT COUNT(id) FROM nform_submission WHERE nform_fk_id=f.id) submission_count,
                  (SELECT MIN(finished) FROM nform_submission WHERE nform_fk_id=f.id) first_submission_date,
                  (SELECT MAX(finished) FROM nform_submission WHERE nform_fk_id=f.id) last_submission_date,
                  allow_multiple_submissions,
                  allow_modify_in_submission,
                  completion_redirect
                  FROM nform f 
                  WHERE f.id=@formId",
                paramsToPass))
            {
                if (reader.Read())
                {
                    form = PopulateForm(reader);
                }
            }

            return form;
        }

        private BOForm PopulateForm(IDataRecord reader)
        {
            var form = new BOForm();

            form.Id = (int)reader["form_id"];
            form.Title = (string)reader["title"];
            form.Description = reader["description"] != DBNull.Value ? (string)reader["description"] : "";
            form.SubTitle = reader["sub_title"] != DBNull.Value ? (string)reader["sub_title"] : "";
            form.ThankYouNote = reader["thank_you_note"] != DBNull.Value ? (string)reader["thank_you_note"] : "";

            switch ((string)reader["form_type"])
            {
                case BOForm.FORM_TYPE_POLL: form.FormType = FormTypes.Poll; break;
                case BOForm.FORM_TYPE_QUESTIONAIRE: form.FormType = FormTypes.Questionaire; break;
            }

            form.SendToString = (reader["send_to"] == DBNull.Value ? "" : (string)reader["send_to"]);
            form.SubmissionCount = (int)reader["submission_count"];
            form.FirstSubmissionDate = reader["first_submission_date"] == DBNull.Value ? (DateTime?)null : (DateTime?)reader["first_submission_date"];
            form.LastSubmissionDate = reader["last_submission_date"] == DBNull.Value ? (DateTime?)null : (DateTime?)reader["last_submission_date"];
            form.AllowMultipleSubmissions = (bool)reader["allow_multiple_submissions"];
            form.AllowModifyInSubmission = (bool)reader["allow_modify_in_submission"];
            form.CompletionRedirect = reader["completion_redirect"] == DBNull.Value ? "" : (string)reader["completion_redirect"];

            return form;
        }

        public BOFormSubmission GetFormSubmission(int submissionId)
        {
            BOFormSubmission submission = null;

            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@submissionId", submissionId);

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMainCustom, CommandType.Text,
                "SELECT id, nform_fk_id, finished FROM nform_submission WHERE id=@submissionId",
                paramsToPass))
            {
                if (reader.Read())
                {
                    submission = new BOFormSubmission();
                    submission.Id = reader.GetInt32(0);
                    submission.FormId = reader.GetInt32(1);
                    submission.Finished = reader.GetDateTime(2);
                }
            }

            return submission;
        }

        public PagedList<BOForm> List(ListingState state)
        {
            PagedList<BOForm> forms = new PagedList<BOForm>();

            string sql;
            CommandType commandType = CommandType.Text;
            SqlParameter[] paramsToPass = null;

            int toRecordIndex = 0, fromRecordIndex = 0;

            if (state.FirstRecordIndex.HasValue && state.RecordsPerPage.HasValue)
            {
                fromRecordIndex = state.FirstRecordIndex.Value + 1;
                toRecordIndex = (fromRecordIndex + state.RecordsPerPage.Value) - 1;
            }

            paramsToPass = new SqlParameter[4];
            paramsToPass[0] = state.SortField.Length < 2 ? new SqlParameter("@sortBy", DBNull.Value) : new SqlParameter("@sortBy", state.SortField);
            paramsToPass[1] = new SqlParameter("@sortDirection", state.SortDirection == SortDir.Ascending ? "ASC" : "DESC");
            paramsToPass[2] = new SqlParameter("@fromRecordIndex", fromRecordIndex);
            paramsToPass[3] = new SqlParameter("@toRecordIndex", toRecordIndex);

            commandType = CommandType.StoredProcedure;

            sql = "[dbo].[ListPagedForms]";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMainCustom, commandType, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    var form = PopulateForm(reader);                    

                    if (forms.AllRecords < 1)
                        forms.AllRecords = reader.GetInt32(21);

                    forms.Add(form);
                }

            }

            return forms;
        }

        public PagedList<BOFormSubmission> ListFormSubmissions(int formId, ListingState state)
        {
            PagedList<BOFormSubmission> submissions = new PagedList<BOFormSubmission>();

            string sql = "";
            CommandType commandType = CommandType.Text;
            SqlParameter[] paramsToPass = null;

            if (state.UsesPaging)
            {
                int toRecordIndex = 0, fromRecordIndex = 0;

                if (state.FirstRecordIndex.HasValue && state.RecordsPerPage.HasValue)
                {
                    fromRecordIndex = state.FirstRecordIndex.Value + 1;
                    toRecordIndex = (fromRecordIndex + state.RecordsPerPage.Value) - 1;
                }

                paramsToPass = new SqlParameter[5];
                paramsToPass[0] = new SqlParameter("@sortBy", (string.IsNullOrEmpty(state.SortField) ? "" : state.SortField));
                paramsToPass[1] = new SqlParameter("@sortDirection", "");
                paramsToPass[2] = new SqlParameter("@fromRecordIndex", fromRecordIndex);
                paramsToPass[3] = new SqlParameter("@toRecordIndex", toRecordIndex);
                paramsToPass[4] = new SqlParameter("@formId", formId);

                commandType = CommandType.StoredProcedure;

                sql = "[dbo].[ListPagedFormSubmissions]";
            }
            else
            {
                sql = @"SELECT id, finished FROM nform_submission 
                        WHERE nform_fk_id=@formId 
                        ORDER BY finished DESC";

                commandType = CommandType.Text;

                paramsToPass = new SqlParameter[1];
                paramsToPass[0] = new SqlParameter("@formId", formId);
            }

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMainCustom, commandType, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    BOFormSubmission submission = new BOFormSubmission();
                    submission.Id = reader.GetInt32(0);
                    submission.FormId = formId;
                    submission.Finished = reader.GetDateTime(1);
                    submissions.Add(submission);
                }

                reader.NextResult();

                if (reader.Read())
                {
                    submissions.AllRecords = reader.GetInt32(0);
                }
            }

            return submissions;
        }

        public Dictionary<int, BOSubmittedAnswer> ListSubmittedAnswers(int submissionId)
        {
            Dictionary<int, BOSubmittedAnswer> submittedAnswers = new Dictionary<int, BOSubmittedAnswer>();

            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@submissionId", submissionId);

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMainCustom, CommandType.Text,
                @"SELECT nsa.nform_answer_fk_id, nsa.nform_submission_fk_id, nsa.submitted_text, nsa.files_fk_id, (SELECT COUNT(nform_answer_fk_id) FROM nform_submitted_answer WHERE nform_answer_fk_id=nsa.nform_answer_fk_id) cnt
                  FROM nform_submitted_answer nsa
                  WHERE nsa.nform_submission_fk_id=@submissionId",
                paramsToPass))
            {
                while (reader.Read())
                {
                    BOSubmittedAnswer submittedAnswer = new BOSubmittedAnswer();
                    submittedAnswer.Answer = new BOAnswer();
                    submittedAnswer.Answer.Id = reader.GetInt32(0);
                    submittedAnswer.SubmissionId = reader.GetInt32(1);

                    if (!reader.IsDBNull(2))
                    {
                        submittedAnswer.SubmittedText = reader.GetString(2);
                    }

                    if (!reader.IsDBNull(3))
                    {
                        int submittedFileId = reader.GetInt32(3);
                        submittedAnswer.SubmittedFile = new BOFile();
                        submittedAnswer.SubmittedFile.Id = submittedFileId;
                    }

                    submittedAnswer.Answer.TimesAnswered = reader.GetInt32(4);

                    submittedAnswers.Add(submittedAnswer.Answer.Id.Value, submittedAnswer);
                }
            }

            return submittedAnswers;
        }

        public Dictionary<int, BOSection> ListSections(int formId)
        {
            Dictionary<int, BOSection> sections = new Dictionary<int, BOSection>();

            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@formId", formId);

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMainCustom, CommandType.Text,
                @"SELECT id section_id, nform_fk_id, section_type, idx, on_client_click
                  FROM nform_section 
                  WHERE nform_fk_id=@formId 
                  ORDER BY idx ASC",
                paramsToPass))
            {
                while (reader.Read())
                {
                    var section = PopulateSection(reader);
                    sections.Add(section.Id.Value, section);
                }
            }

            return sections;
        }

        private BOSection PopulateSection(IDataRecord reader)
        {
            var section = new BOSection();
            section.Id = (int)reader["section_id"];
            section.ParentId = (int)reader["nform_fk_id"];
            section.Title = (string)reader["title"];
            section.Description = reader["description"] != DBNull.Value ? (string)reader["description"] : "";

            string sectionType = (string)reader["section_type"];
            switch (sectionType)
            {
                case BOSection.SECTION_TYPE_MULTI_PAGE: section.SectionType = SectionTypes.MultiPage; break;
                case BOSection.SECTION_TYPE_SINGLE_PAGE: section.SectionType = SectionTypes.SinglePage; break;
            }

            section.Idx = (int)reader["idx"];
            section.OnClientClick = reader["on_client_click"] != DBNull.Value ? (string)reader["on_client_click"] : "";
            
            return section;
        }

        public Dictionary<int, BOQuestion> ListQuestions(int sectionId)
        {
            Dictionary<int, BOQuestion> questions = new Dictionary<int, BOQuestion>();

            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@sectionId", sectionId);

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMainCustom, CommandType.Text,
                @"SELECT nq.id question_id, nq.nform_section_fk_id, nq.is_answer_required, nq.idx, nq.validation_type, 
                    (SELECT COUNT(distinct ns.id) FROM nform_submitted_answer nsa
                     INNER JOIN nform_submission ns ON ns.id=nsa.nform_submission_fk_id
                     INNER JOIN nform_answer na ON na.id=nsa.nform_answer_fk_id
                     WHERE na.nform_question_fk_id=nq.id and na.is_fake=0) NoAnsweredQuestions
                  FROM nform_question nq
                  WHERE nq.nform_section_fk_id=@sectionId 
                  ORDER BY nq.idx ASC",
                paramsToPass))
            {
                while (reader.Read())
                {
                    var question = PopulateQuestion(reader);

                    questions.Add(question.Id.Value, question);
                }
            }

            return questions;
        }

        private BOQuestion PopulateQuestion(IDataRecord reader)
        {
            var question = new BOQuestion();

            question.Id = (int)reader["question_id"];
            question.ParentId = (int)reader["nform_section_fk_id"];
            question.IsAnswerRequired = (bool)reader["is_answer_required"];
            question.Idx = (int)reader["idx"];
            question.Title = (string)reader["title"];
            question.Description = reader["description"] != DBNull.Value ? (string)reader["description"] : "";

            if (reader["validation_type"] == System.DBNull.Value)
            {
                question.ValidationType = ValidationTypes.None;
            }
            else
            {
                switch (reader.GetString(5))
                {
                    case BOQuestion.VALIDATION_TYPE_NONE: question.ValidationType = ValidationTypes.None; break;
                    case BOQuestion.VALIDATION_TYPE_DATETIME: question.ValidationType = ValidationTypes.DateTime; break;
                    case BOQuestion.VALIDATION_TYPE_TIME: question.ValidationType = ValidationTypes.Time; break;
                    case BOQuestion.VALIDATION_TYPE_EMAIL: question.ValidationType = ValidationTypes.Email; break;
                    case BOQuestion.VALIDATION_TYPE_INTEGER: question.ValidationType = ValidationTypes.Integer; break;
                    case BOQuestion.VALIDATION_TYPE_NUMERIC: question.ValidationType = ValidationTypes.Numeric; break;
                    case BOQuestion.VALIDATION_TYPE_CAPTCHA: question.ValidationType = ValidationTypes.Captcha; break;
                }
            }

            question.TimesAnswered = reader.GetInt32(6);

            return question;
        }

        public Dictionary<int, BOAnswer> ListAnswers(int questionId)
        {
            Dictionary<int, BOAnswer> answers = new Dictionary<int, BOAnswer>();

            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@questionId", questionId);

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMainCustom, CommandType.Text,
                @"SELECT fa.id answer_id, fa.nform_question_fk_id, fa.answer_type, fa.additional_field_type, fa.idx, fa.max_chars, fa.number_of_rows, fa.max_file_size, fa.allowed_mime_types,
		                    CASE 
			                    WHEN (fa.answer_type = 'SingleText')
				                    THEN
					                    (SELECT COUNT(nform_answer_fk_id) 
					                    FROM nform_submitted_answer 
					                    WHERE nform_answer_fk_id=fa.id AND submitted_text IS NOT NULL AND DATALENGTH(submitted_text) > 0 )
			                    WHEN ( fa.answer_type='SingleFile')
				                    THEN
					                    (SELECT COUNT(nform_answer_fk_id) 
					                    FROM nform_submitted_answer 
					                    WHERE nform_answer_fk_id=fa.id AND files_fk_id IS NOT NULL)
			                    ELSE
					                    (SELECT COUNT(nform_answer_fk_id) 
					                    FROM nform_submitted_answer 
					                    WHERE nform_answer_fk_id=fa.id)
		                    END
                      AS times_answered, 
                      is_fake
                    FROM nform_answer fa
                  WHERE fa.nform_question_fk_id=@questionId 
                  ORDER BY fa.idx ASC",
                paramsToPass))
            {
                while (reader.Read())
                {
                    var answer = PopulateAnswer(reader);

                    answers.Add(answer.Id.Value, answer);
                }
            }

            return answers;
        }

        private BOAnswer PopulateAnswer(IDataRecord reader)
        {
            var answer = new BOAnswer();
            answer.Id = (int)reader["answer_id"];
            answer.ParentId = (int)reader["nform_question_fk_id"];
            answer.Title = (string)reader["title"];
            answer.Description = reader["description"] != DBNull.Value ? (string)reader["description"] : "";

            string answerType = (string)reader["answer_type"];
            switch (answerType)
            {
                case BOAnswer.ANSWER_TYPE_CHECKBOX: answer.AnswerType = AnswerTypes.Checkbox; break;
                case BOAnswer.ANSWER_TYPE_DROPDOWN: answer.AnswerType = AnswerTypes.DropDown; break;
                case BOAnswer.ANSWER_TYPE_RADIO: answer.AnswerType = AnswerTypes.Radio; break;
                case BOAnswer.ANSWER_TYPE_SINGLE_FILE: answer.AnswerType = AnswerTypes.SingleFile; break;
                case BOAnswer.ANSWER_TYPE_SINGLE_TEXT: answer.AnswerType = AnswerTypes.SingleText; break;
            }

            string additionalFieldType = (string)reader["additional_field_type"];
            switch (additionalFieldType)
            {
                case BOAnswer.ADDITIONAL_FIELD_TYPE_FILE: answer.AdditionalFieldType = AdditionalFieldTypes.File; break;
                case BOAnswer.ADDITIONAL_FIELD_TYPE_TEXT: answer.AdditionalFieldType = AdditionalFieldTypes.Text; break;
                case BOAnswer.ADDITIONAL_FIELD_TYPE_NONE: answer.AdditionalFieldType = AdditionalFieldTypes.None; break;
            }

            answer.Idx = (int)reader["idx"];
            answer.MaxChars = (reader["max_chars"] == DBNull.Value ? (int?)null : (int)reader["max_chars"]);
            answer.NumberOfRows = (reader["number_of_rows"] == DBNull.Value ? (int?)null : (int)reader["number_of_rows"]);
            answer.MaxFileSize = (reader["max_file_size"] == DBNull.Value ? (int?)null : (int)reader["max_file_size"]);
            answer.AllowedMimeTypes = (reader["allowed_mime_types"] == DBNull.Value ? "" : (string)reader["allowed_mime_types"]);
            answer.TimesAnswered = (int)reader["times_answered"];
            answer.IsFake = (bool)reader["is_fake"];

            return answer;
        }
    }
}