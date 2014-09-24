using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MsSqlDBUtility;
using One.Net.BLL.Forms;
using System.Threading;
using log4net;

namespace One.Net.BLL.DAL
{
    public class DbForm 
    {
        private static readonly ILog log = LogManager.GetLogger("DbForm");

        public void Change(BOForm form)
        {
            SqlParameter[] paramsToPass = new SqlParameter[7];
            paramsToPass[0] = SqlHelper.GetNullable("Id", form.Id);
            paramsToPass[1] = new SqlParameter("@ContentId", form.ContentId.Value);

            string formType = "";
            switch (form.FormType)
            {
                case FormTypes.Poll: formType = BOForm.FORM_TYPE_POLL; break;
                case FormTypes.Questionaire: formType = BOForm.FORM_TYPE_QUESTIONAIRE; break;
            }

            paramsToPass[2] = new SqlParameter("@FormType", formType);
            paramsToPass[3] = SqlHelper.GetNullable("@SendTo", form.SendToString); 
            paramsToPass[4] = new SqlParameter("@AllowModifyInSubmission", form.AllowModifyInSubmission);
            paramsToPass[5] = new SqlParameter("@AllowMultipleSubmissions", form.AllowMultipleSubmissions);
            paramsToPass[6] = string.IsNullOrEmpty(form.CompletionRedirect) ? new SqlParameter("@CompletionRedirect", DBNull.Value) : new SqlParameter("@CompletionRedirect", form.CompletionRedirect);
            paramsToPass[0].Direction = ParameterDirection.InputOutput;
            paramsToPass[0].DbType = DbType.Int32;

            string sql;

            if (form.Id.HasValue)
            {
                sql = @"UPDATE [dbo].[nform] SET content_fk_id=@ContentId, form_type=@FormType, send_to=@SendTo, allow_modify_in_submission=@AllowModifyInSubmission, allow_multiple_submissions=@AllowMultipleSubmissions, completion_redirect=@CompletionRedirect WHERE id=@Id";
            }
            else
            {
                sql = @"INSERT INTO [dbo].[nform]
                           (form_type, content_fk_id, send_to, allow_modify_in_submission, allow_multiple_submissions, completion_redirect)
                           VALUES (@FormType, @ContentId, @SendTo, @AllowModifyInSubmission, @AllowMultipleSubmissions, @CompletionRedirect ); SET @Id=SCOPE_IDENTITY();";
            }

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);

            if (!form.Id.HasValue)
            {
                form.Id = Int32.Parse(paramsToPass[0].Value.ToString());
            }
        }

        public void ChangeSection(BOSection section)
        {
            SqlParameter[] paramsToPass = new SqlParameter[6];
            paramsToPass[0] = SqlHelper.GetNullable("@Id", section.Id);
            paramsToPass[1] = new SqlParameter("@ContentId", section.ContentId.Value);

            string sectionType = "";
            switch (section.SectionType)
            {
                case SectionTypes.MultiPage: sectionType = BOSection.SECTION_TYPE_MULTI_PAGE; break;
                case SectionTypes.SinglePage: sectionType = BOSection.SECTION_TYPE_SINGLE_PAGE; break;
            }

            paramsToPass[2] = new SqlParameter("@SectionType", sectionType);
            paramsToPass[3] = new SqlParameter("@FormId", section.ParentId.Value);
            paramsToPass[4] = new SqlParameter("@Idx", section.Idx);

            paramsToPass[5] = string.IsNullOrEmpty(section.OnClientClick) ?  new SqlParameter("@OnClientClick", DBNull.Value) : new SqlParameter("@OnClientClick", section.OnClientClick);
            paramsToPass[5].DbType = DbType.String;

            paramsToPass[0].Direction = ParameterDirection.InputOutput;
            paramsToPass[0].DbType = DbType.Int32;

            string sql = "";

            if (section.Id.HasValue)
            {
                sql = @"UPDATE nform_section SET section_type=@SectionType, content_fk_id=@ContentId, nform_fk_id=@FormId, idx=@Idx, on_client_click=@OnClientClick WHERE id=@Id";
            }
            else
            {
                sql = @"INSERT INTO nform_section
                    (section_type, content_fk_id, nform_fk_id, idx, on_client_click)
                    VALUES (@SectionType, @ContentId, @FormId, @Idx, @OnClientClick); SET @Id=SCOPE_IDENTITY();";
            }

            var recordsUpdated = SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);

            if (!section.Id.HasValue)
            {
                section.Id = Int32.Parse(paramsToPass[0].Value.ToString());
            }
        }

        public void ChangeQuestion(BOQuestion question)
        {
            SqlParameter[] paramsToPass = new SqlParameter[6];
            paramsToPass[0] = SqlHelper.GetNullable("Id", question.Id);
            paramsToPass[1] = new SqlParameter("@ContentId", question.ContentId.Value);
            paramsToPass[2] = new SqlParameter("@IsAnswerRequired", question.IsAnswerRequired);
            paramsToPass[3] = new SqlParameter("@SectionId", question.ParentId.Value);
            paramsToPass[4] = new SqlParameter("@Idx", question.Idx);

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

            paramsToPass[5] = new SqlParameter("@ValidationType", validationType);

            string sql = "";

            paramsToPass[0].Direction = ParameterDirection.InputOutput;
            paramsToPass[0].DbType = DbType.Int32;

            if (question.Id.HasValue)
            {
                sql = @"UPDATE [dbo].[nform_question] SET is_answer_required=@IsAnswerRequired, content_fk_id=@ContentId,
                        nform_section_fk_id=@SectionId, idx=@Idx, validation_type=@ValidationType WHERE id=@Id";
            }
            else
            {
                sql = @"INSERT INTO nform_question
                        (is_answer_required, content_fk_id, nform_section_fk_id, idx, validation_type)
                        VALUES (@IsAnswerRequired, @ContentId, @SectionId, @Idx, @ValidationType); SET @Id=SCOPE_IDENTITY();";
            }

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);

            if (!question.Id.HasValue)
            {
                question.Id = Int32.Parse(paramsToPass[0].Value.ToString());
            }
        }

        public void InsertAnswer(BOAnswer answer)
        {
            SqlParameter[] paramsToPass = new SqlParameter[11];
            paramsToPass[0] = new SqlParameter("@Id", answer.Id);
            paramsToPass[1] = new SqlParameter("@ContentId", answer.ContentId.Value);

            string answerType = "";
            switch (answer.AnswerType)
            {
                case AnswerTypes.Checkbox: answerType = BOAnswer.ANSWER_TYPE_CHECKBOX; break;
                case AnswerTypes.DropDown: answerType = BOAnswer.ANSWER_TYPE_DROPDOWN; break;
                case AnswerTypes.Radio: answerType = BOAnswer.ANSWER_TYPE_RADIO; break;
                case AnswerTypes.SingleFile: answerType = BOAnswer.ANSWER_TYPE_SINGLE_FILE; break;
                case AnswerTypes.SingleText: answerType = BOAnswer.ANSWER_TYPE_SINGLE_TEXT; break;
            }
            paramsToPass[2] = new SqlParameter("@AnswerType", answerType);

            string additionalFieldType = "";
            switch (answer.AdditionalFieldType)
            {
                case AdditionalFieldTypes.File: additionalFieldType = BOAnswer.ADDITIONAL_FIELD_TYPE_FILE; break;
                case AdditionalFieldTypes.Text: additionalFieldType = BOAnswer.ADDITIONAL_FIELD_TYPE_TEXT; break;
                case AdditionalFieldTypes.None: additionalFieldType = BOAnswer.ADDITIONAL_FIELD_TYPE_NONE; break;
            }
            paramsToPass[3] = new SqlParameter("@AdditionalFieldType", additionalFieldType);

            paramsToPass[4] = new SqlParameter("@QuestionId", answer.ParentId.Value);
            paramsToPass[5] = new SqlParameter("@Idx", answer.Idx);
            paramsToPass[6] = (answer.MaxChars.HasValue && answer.MaxChars.Value > 0 ? new SqlParameter("@MaxChars", answer.MaxChars.Value) : new SqlParameter("@MaxChars", DBNull.Value));
            paramsToPass[7] = (answer.NumberOfRows.HasValue && answer.NumberOfRows.Value > 0 ? new SqlParameter("@NumberOfRows", answer.NumberOfRows.Value) : new SqlParameter("@NumberOfRows", DBNull.Value));
            paramsToPass[8] = (answer.MaxFileSize.HasValue && answer.MaxFileSize.Value > 0 ? new SqlParameter("@MaxFileSize", answer.MaxFileSize.Value) : new SqlParameter("@MaxFileSize", DBNull.Value));
            paramsToPass[9] = SqlHelper.GetNullable("@AllowedMimeTypes", answer.AllowedMimeTypes); 
            paramsToPass[10] = new SqlParameter("@IsFake", answer.IsFake);

            string sql = "";

            paramsToPass[0].Direction = ParameterDirection.Output;
            paramsToPass[0].DbType = DbType.Int32;

            sql = @"INSERT INTO nform_answer
                        (answer_type, additional_field_type, content_fk_id, nform_question_fk_id, idx, max_chars, number_of_rows, max_file_size, allowed_mime_types, is_fake)
                        VALUES (@AnswerType, @AdditionalFieldType, @ContentId, @QuestionId, @Idx, @MaxChars, @NumberOfRows, @MaxFileSize, @AllowedMimeTypes, @IsFake); SET @Id=SCOPE_IDENTITY();";

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);

            answer.Id = Int32.Parse(paramsToPass[0].Value.ToString());
        }

        public void DeleteForm(int formId)
        {
            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@formId", formId);

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text,
                @"DELETE FROM nform WHERE id=@formId", paramsToPass);
        }

        public void DeleteSection(int sectionId)
        {
            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@sectionId", sectionId);

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text,
                @"DELETE FROM nform_section WHERE id=@sectionId", paramsToPass);
        }

        public void DeleteQuestion(int questionId)
        {
            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@questionId", questionId);

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text,
                @"DELETE FROM nform_question WHERE id=@questionId", paramsToPass);
        }

        public void DeleteAnswer(int answerId)
        {
            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@answerId", answerId);

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text,
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

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text,
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

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text,
                sql, paramsToPass);
        }

        public BOForm Get(int formId)
        {
            BOForm form = null;

            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@formId", formId);

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
                @"SELECT f.content_fk_id, f.id, f.form_type, f.send_to, 
                  (SELECT COUNT(id) FROM nform_submission WHERE nform_fk_id=f.id) cnt,
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
                    form = new BOForm();
                    form.ContentId = reader.GetInt32(0);
                    form.Id = reader.GetInt32(1);

                    switch (reader.GetString(2))
                    {
                        case BOForm.FORM_TYPE_POLL: form.FormType = FormTypes.Poll; break;
                        case BOForm.FORM_TYPE_QUESTIONAIRE: form.FormType = FormTypes.Questionaire; break;
                    }

                    form.SendToString = (reader.IsDBNull(3) ? "" : reader.GetString(3));
                    form.SubmissionCount = reader.GetInt32(4);
                    form.FirstSubmissionDate = (reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5));
                    form.LastSubmissionDate = (reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6));
                    form.AllowMultipleSubmissions = reader.GetBoolean(7);
                    form.AllowModifyInSubmission = reader.GetBoolean(8);
                    form.CompletionRedirect = (reader["completion_redirect"] == DBNull.Value ? "" : (string)reader["completion_redirect"]);
                }
            }

            return form;
        }

        public BOFormSubmission GetFormSubmission(int submissionId)
        {
            BOFormSubmission submission = null;

            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@submissionId", submissionId);

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
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

        public PagedList<BOForm> List(ListingState state, bool includeFormsWithoutTranslation, int languageId)
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

            paramsToPass = new SqlParameter[6];
            paramsToPass[0] = state.SortField.Length < 2 ? new SqlParameter("@sortBy", DBNull.Value) : new SqlParameter("@sortBy", state.SortField);
            paramsToPass[1] = new SqlParameter("@sortDirection", state.SortDirection == SortDir.Ascending ? "ASC" : "DESC");
            paramsToPass[2] = new SqlParameter("@fromRecordIndex", fromRecordIndex);
            paramsToPass[3] = new SqlParameter("@toRecordIndex", toRecordIndex);
            paramsToPass[4] = new SqlParameter("@languageId", languageId);
            paramsToPass[5] = new SqlParameter("@includeFormsWithoutTranslation", includeFormsWithoutTranslation);

            commandType = CommandType.StoredProcedure;

            sql = "[dbo].[ListPagedForms]";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, commandType, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    BOForm form = new BOForm();
                    form.ContentId = reader.GetInt32(10);
                    form.Id = reader.GetInt32(11);
                    DbHelper.PopulateContent(reader, form, languageId);

                    string formType = reader.GetString(12);
                    switch (formType)
                    {
                        case BOForm.FORM_TYPE_POLL:
                            form.FormType = FormTypes.Poll;
                            break;
                        case BOForm.FORM_TYPE_QUESTIONAIRE:
                            form.FormType = FormTypes.Questionaire;
                            break;
                    }

                    form.SendToString = (reader.IsDBNull(13) ? "" : reader.GetString(13));
                    form.SubmissionCount = reader.GetInt32(14);
                    form.FirstSubmissionDate = (reader.IsDBNull(15) ? (DateTime?) null : reader.GetDateTime(15));
                    form.LastSubmissionDate = (reader.IsDBNull(16) ? (DateTime?) null : reader.GetDateTime(16));
                    form.AllowMultipleSubmissions = reader.GetBoolean(17);
                    form.AllowModifyInSubmission = reader.GetBoolean(18);
                    form.CompletionRedirect = (reader["completion_redirect"] == DBNull.Value ? "" : (string)reader["completion_redirect"]);

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

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, commandType, sql, paramsToPass))
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

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
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

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
                @"SELECT id, nform_fk_id, section_type, idx, content_fk_id, on_client_click
                  FROM nform_section 
                  WHERE nform_fk_id=@formId 
                  ORDER BY idx ASC",
                paramsToPass))
            {
                while (reader.Read())
                {
                    var section = new BOSection();
                    section.Id = reader.GetInt32(0);
                    section.ParentId = reader.GetInt32(1);

                    string sectionType = reader.GetString(2);
                    switch (sectionType)
                    {
                        case BOSection.SECTION_TYPE_MULTI_PAGE: section.SectionType = SectionTypes.MultiPage; break;
                        case BOSection.SECTION_TYPE_SINGLE_PAGE: section.SectionType = SectionTypes.SinglePage; break;
                    }

                    section.Idx = reader.GetInt32(3);
                    section.ContentId = reader.GetInt32(4);
                    section.OnClientClick = reader["on_client_click"] != DBNull.Value
                                                ? (string) reader["on_client_click"]
                                                : "";
                    sections.Add(section.Id.Value, section);
                }
            }

            return sections;
        }

        public Dictionary<int, BOQuestion> ListQuestions(int sectionId)
        {
            Dictionary<int, BOQuestion> questions = new Dictionary<int, BOQuestion>();

            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@sectionId", sectionId);

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
                @"SELECT nq.id, nq.nform_section_fk_id, nq.is_answer_required, nq.idx, nq.content_fk_id, nq.validation_type, 
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
                    BOQuestion question = new BOQuestion();
                    question.Id = reader.GetInt32(0);
                    question.ParentId = reader.GetInt32(1);
                    question.IsAnswerRequired = reader.GetBoolean(2);
                    question.Idx = reader.GetInt32(3);
                    question.ContentId = reader.GetInt32(4);

                    if (reader.IsDBNull(5))
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

                    questions.Add(question.Id.Value, question);
                }
            }

            return questions;
        }

        public Dictionary<int, BOAnswer> ListAnswers(int questionId)
        {
            Dictionary<int, BOAnswer> answers = new Dictionary<int, BOAnswer>();

            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@questionId", questionId);

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
                @"SELECT fa.id, fa.nform_question_fk_id, fa.answer_type, fa.additional_field_type, fa.idx, fa.content_fk_id, fa.max_chars, fa.number_of_rows, fa.max_file_size, fa.allowed_mime_types,
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
                      AS cnt, 
                      is_fake
                    FROM nform_answer fa
                  WHERE fa.nform_question_fk_id=@questionId 
                  ORDER BY fa.idx ASC",
                paramsToPass))
            {
                while (reader.Read())
                {
                    BOAnswer answer = new BOAnswer();
                    answer.Id = reader.GetInt32(0);
                    answer.ParentId = reader.GetInt32(1);

                    string answerType = reader.GetString(2);
                    switch (answerType)
                    {
                        case BOAnswer.ANSWER_TYPE_CHECKBOX: answer.AnswerType = AnswerTypes.Checkbox; break;
                        case BOAnswer.ANSWER_TYPE_DROPDOWN: answer.AnswerType = AnswerTypes.DropDown; break;
                        case BOAnswer.ANSWER_TYPE_RADIO: answer.AnswerType = AnswerTypes.Radio; break;
                        case BOAnswer.ANSWER_TYPE_SINGLE_FILE: answer.AnswerType = AnswerTypes.SingleFile; break;
                        case BOAnswer.ANSWER_TYPE_SINGLE_TEXT: answer.AnswerType = AnswerTypes.SingleText; break;
                    }

                    string additionalFieldType = reader.GetString(3);
                    switch (additionalFieldType)
                    {
                        case BOAnswer.ADDITIONAL_FIELD_TYPE_FILE: answer.AdditionalFieldType = AdditionalFieldTypes.File; break;
                        case BOAnswer.ADDITIONAL_FIELD_TYPE_TEXT: answer.AdditionalFieldType = AdditionalFieldTypes.Text; break;
                        case BOAnswer.ADDITIONAL_FIELD_TYPE_NONE: answer.AdditionalFieldType = AdditionalFieldTypes.None; break;
                    }

                    answer.Idx = reader.GetInt32(4);
                    answer.ContentId = reader.GetInt32(5);
                    answer.MaxChars = (reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6));
                    answer.NumberOfRows = (reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7));
                    answer.MaxFileSize = (reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8));
                    answer.AllowedMimeTypes = (reader.IsDBNull(9) ? "" : reader.GetString(9));
                    answer.TimesAnswered = reader.GetInt32(10);
                    answer.IsFake = reader.GetBoolean(11);

                    answers.Add(answer.Id.Value, answer);
                }
            }

            return answers;
        }
    }
}
