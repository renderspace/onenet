using System;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using System.Threading;


using One.Net.BLL.Forms;
using One.Net.BLL;

namespace OneMainWeb
{
    public partial class Forms : OneBasePage
    {
        #region Variables

        protected static BForm formB = new BForm();

        FormHelper helper = new FormHelper();

        #endregion Variables

        #region Properties

        protected string ElementStringId
        {
            get { return (Session["ElementStringId"] != null ? Session["ElementStringId"].ToString() : ""); }
            set { Session["ElementStringId"] = value; }
        }

        protected BOForm SessionForm
        {
            get { return (Session["SessionForm"] != null && Session["SessionForm"] is BOForm ? (BOForm)Session["SessionForm"] : null); }
            set { Session["SessionForm"] = value; }
        }

        protected BOFormSubmission SessionSubmission
        {
            get { return (Session["SessionSubmission"] != null && Session["SessionSubmission"] is BOFormSubmission ? (BOFormSubmission)Session["SessionSubmission"] : null); }
            set { Session["SessionSubmission"] = value; }
        }

        protected Dictionary<string, BOAdminFormElement> ElementMap
        {
            get { return (Session["ElementMap"] != null && Session["ElementMap"] is Dictionary<string, BOAdminFormElement> ? (Dictionary<string, BOAdminFormElement>)Session["ElementMap"] : null); }
            set { Session["ElementMap"] = value; }
        }

        protected List<BOFormSubmission> FormSubmissions
        {
            get { return (Session["FormSubmissions"] != null ? (List<BOFormSubmission>)Session["FormSubmissions"] : new List<BOFormSubmission>()); }
            set { Session["FormSubmissions"] = value; }
        }

        #endregion Properties

        #region Initialization

        protected void Page_Load(object sender, EventArgs e)
        {
            Notifier1.Visible = true;

            if (!IsPostBack)
            {
                MultiView1.ActiveViewIndex = 0;
            }
        }
        protected void Multiview1_ActiveViewChanged(object sender, EventArgs e)
        {
            if (((MultiView)sender).ActiveViewIndex == 0)
            {
                TwoPostbackPager1.RecordsPerPage = GridViewPageSize;
                TwoPostbackPager1.SelectedPage = 1;
                GridViewSortExpression = "";
                FormsList_DataBind();
                ClearSessionValues();
            }
            else if (((MultiView)sender).ActiveViewIndex == 1)
            {
                LoadFormTabControls();
            }


            bool formIsNewlyAdded = false;

            if (SessionForm != null && ElementMap != null && ElementMap["Form" + SessionForm.Id.Value] != null)
            {
                formIsNewlyAdded = ElementMap["Form" + SessionForm.Id.Value].NewlyAdded;
            }

            /*
            tabMultiview.Views[1].Selectable = (tabMultiview.Views[1].Visible || tabMultiview.Views[2].Visible || tabMultiview.Views[3].Visible || tabMultiview.Views[4].Visible || tabMultiview.Views[5].Visible);
            tabMultiview.Views[2].Selectable = tabMultiview.Views[3].Selectable = tabMultiview.Views[4].Selectable = ((tabMultiview.Views[1].Visible || tabMultiview.Views[2].Visible || tabMultiview.Views[3].Visible || tabMultiview.Views[4].Visible || tabMultiview.Views[5].Visible ) && SessionForm != null && !formIsNewlyAdded);
            tabMultiview.Views[5].Selectable = tabMultiview.Views[5].Visible && SessionForm != null && !formIsNewlyAdded;*/
        }

        private void FormsList_DataBind()
        {
            ListingState state = new ListingState();
            state.RecordsPerPage = GridViewPageSize;
            state.SortDirection = GridViewSortDirection;
            state.FirstRecordIndex = (TwoPostbackPager1.SelectedPage - 1) * GridViewPageSize;
            state.SortField = GridViewSortExpression;
            var forms = formB.ListUnCached(state, ShowUntranslated, Thread.CurrentThread.CurrentCulture.LCID);
            TwoPostbackPager1.TotalRecords = forms.AllRecords;
            TwoPostbackPager1.DetermineData();
            formGridView.DataSource = forms;
            formGridView.DataBind(); 
        }

        public void TwoPostbackPager1_Command(object sender, CommandEventArgs e)
        {
            TwoPostbackPager1.SelectedPage = Convert.ToInt32(e.CommandArgument);
            FormsList_DataBind();
        }

        private void LoadFormTabControls()
        {
            overallButtons.Visible = plhUpdateQuestion.Visible = plhUpdateSection.Visible = plhUpdateForm.Visible = plhAddForm.Visible = plhAddSection.Visible = plhAddQuestion.Visible = false;

            if (!string.IsNullOrEmpty(ElementStringId) && SessionForm != null)
            {
                overallButtons.Visible = true;

                if (ElementStringId.Contains("Form"))
                {
                    LoadFormControls();
                }
                else if (ElementStringId.Contains("Section"))
                {
                    LoadSectionControls();
                }
                else if (ElementStringId.Contains("Question"))
                {
                    int questionId = Int32.Parse(ElementStringId.Replace("Question", ""));
                    BOQuestion question = BOForm.FindQuestion(SessionForm, questionId);
                    FormHelper.FrontEndQuestionTypes userQuestionType = FormHelper.FrontEndQuestionTypes.SingleLineOfText;

                    if (question.FirstAnswerKey.HasValue && question.Answers[question.FirstAnswerKey.Value] != null)
                    {
                        BOAnswer firstAnswer = question.Answers[question.FirstAnswerKey.Value];
                        switch (firstAnswer.AnswerType)
                        {
                            case AnswerTypes.Checkbox:  { userQuestionType = FormHelper.FrontEndQuestionTypes.MenuToChooseFrom; break; }
                            case AnswerTypes.Radio:     { userQuestionType = FormHelper.FrontEndQuestionTypes.MenuToChooseFrom; break; }
                            case AnswerTypes.DropDown:  { userQuestionType = FormHelper.FrontEndQuestionTypes.MenuToChooseFrom; break; }
                            case AnswerTypes.SingleText:
                                {
                                    switch (question.ValidationType)
                                    {
                                        case ValidationTypes.None: userQuestionType = (firstAnswer.NumberOfRows.HasValue ? FormHelper.FrontEndQuestionTypes.MultiLineText : FormHelper.FrontEndQuestionTypes.SingleLineOfText); break;
                                        case ValidationTypes.Integer: userQuestionType = FormHelper.FrontEndQuestionTypes.Integer; break;
                                        case ValidationTypes.Email: userQuestionType = FormHelper.FrontEndQuestionTypes.Email; break;
                                        case ValidationTypes.Numeric: userQuestionType = FormHelper.FrontEndQuestionTypes.NumericalValue; break;
                                        case ValidationTypes.DateTime: userQuestionType = FormHelper.FrontEndQuestionTypes.DateTime; break;
                                        case ValidationTypes.Time: userQuestionType = FormHelper.FrontEndQuestionTypes.Time; break;
                                        case ValidationTypes.Captcha: userQuestionType = FormHelper.FrontEndQuestionTypes.Captcha; break;
                                        case ValidationTypes.VAT: userQuestionType = FormHelper.FrontEndQuestionTypes.VAT; break;
                                        case ValidationTypes.Telephone: userQuestionType = FormHelper.FrontEndQuestionTypes.Telephone; break;
                                    }

                                } break;
                            case AnswerTypes.SingleFile: { userQuestionType = FormHelper.FrontEndQuestionTypes.FileUpload; break; }
                        }
                    }

                    LoadQuestionControls(question, userQuestionType);
                }
            }
            else
            {
                plhAddForm.Visible = true;
            }
        }

        private void LoadFormControls()
        {
            if (SessionForm != null)
            {
                plhUpdateForm.Visible = true;
                if (SessionForm.SubmissionCount > 0)
                {
                    plhAddSection.Visible = false;
                }
                else
                {
                    plhAddSection.Visible = true;
                }

                // Now load Session values
                switch (SessionForm.FormType)
                {
                    case FormTypes.Poll: 
                        { 
                            ddlFormTypes.SelectedValue = BOForm.FORM_TYPE_POLL;
                            txtFormThankYouNote.Visible = false;

                            txtFormDescription.Text = SessionForm.Teaser;
                            txtFormDescription.Visible = true;

                            chkAllowModifyInSubmission.Visible = false;
                            break;
                        }
                    case FormTypes.Questionaire:
                        {
                            ddlFormTypes.SelectedValue = BOForm.FORM_TYPE_QUESTIONAIRE;
                            txtFormThankYouNote.Text = SessionForm.Html;
                            txtFormThankYouNote.Visible = true;

                            txtFormDescription.Text = SessionForm.Teaser;
                            txtFormDescription.Visible = true;

                            if ( SessionForm.FirstSectionKey.HasValue )
                            {
                                ddlUpdateSectionTypes.SelectedValue = SessionForm.Sections[SessionForm.FirstSectionKey.Value].SectionType.ToString();
                                chkAllowModifyInSubmission.Visible = SessionForm.Sections[SessionForm.FirstSectionKey.Value].SectionType == SectionTypes.MultiPage;
                            }
                            break;
                        }
                }

                txtFormName.Text = SessionForm.Title;
                InputFormPrivateName.Text = SessionForm.SubTitle;
                txtSendTo.Text = SessionForm.SendToString;
                chkAllowModifyInSubmission.Checked = SessionForm.AllowModifyInSubmission;
                chkAllowMultipleSubmissions.Checked = SessionForm.AllowMultipleSubmissions;
                InputCompletionRedirect.Text = SessionForm.CompletionRedirect;

                ddlFormTypes.Enabled = (SessionForm.Sections.Count == 0 && SessionForm.SubmissionCount == 0);
                ddlUpdateSectionTypes.Enabled = (SessionForm.Sections.Count <= 1 && SessionForm.SubmissionCount == 0);
            }
        }

        protected void ddlFormTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            FormTypes type = FormTypes.Poll;
            type = (ddlFormTypes.SelectedValue == FormTypes.Poll.ToString() ? FormTypes.Poll : FormTypes.Questionaire);

            switch (type)
            {
                case FormTypes.Poll:
                    {
                        txtFormThankYouNote.Visible = false;
                        chkAllowModifyInSubmission.Visible = false;
                        break;
                    }
                case FormTypes.Questionaire:
                    {
                        txtFormThankYouNote.Visible = true;
                        if (SessionForm.FirstSectionKey.HasValue)
                        {
                            chkAllowModifyInSubmission.Visible = SessionForm.Sections[SessionForm.FirstSectionKey.Value].SectionType == SectionTypes.MultiPage;
                        }
                        break;
                    }
            }
        }

        private void LoadSectionControls()
        {
            int sectionId = Int32.Parse(ElementStringId.Replace("Section", ""));

            if (SessionForm.Sections[sectionId] != null)
            {
                plhUpdateSection.Visible = true;
                if (SessionForm.SubmissionCount > 0)
                {
                    plhAddQuestion.Visible = false;
                    cmdUnDeleteSection.Visible = false;
                    cmdDeleteSection.Visible = false;
                }
                else
                {
                    plhAddQuestion.Visible = true;
                    cmdUnDeleteSection.Visible = ElementMap[ElementStringId].PendingDelete;
                    cmdDeleteSection.Visible = !ElementMap[ElementStringId].PendingDelete;
                }

                // Now load Session values
                ddlUpdateSectionTypes.SelectedValue = SessionForm.Sections[sectionId].SectionType.ToString();

                txtSectionName.Text = SessionForm.Sections[sectionId].Title;
                InputSectionDescription.Text = SessionForm.Sections[sectionId].Teaser;

                InputSectionOnClientClick.Text = SessionForm.Sections[sectionId].OnClientClick;
                ddlUpdateSectionTypes.Enabled = (SessionForm.Sections.Count <= 1 && SessionForm.SubmissionCount == 0);
            }
        }

        private void LoadQuestionControls(BOQuestion question, FormHelper.FrontEndQuestionTypes userQuestionType)
        {
            separateInput.Visible = PanelNoOfLines.Visible = txtMaxChars.Visible = PanelMaxFileSize.Visible =
            chkFirstAnswerIsFake.Visible = chkAllowBlankAnswersInMenu.Visible = divAllowedMimeTypes.Visible = PanelAnswersList.Visible = 
            divAnswerPresentationTypes.Visible = divFrontEndQuestionTypes.Visible = false;

            chkAnswerIsRequired.Checked = false;
            chkAnswerIsRequired.Enabled = true;

            if (question != null)
            {
                plhUpdateQuestion.Visible = true;

                // Load common Session values for Poll and Questionaire
                txtQuestionText.Text = question.Title;
                txtQuestionDescription.Text = question.Teaser;
                chkAnswerIsRequired.Checked = question.IsAnswerRequired;

                if (SessionForm.SubmissionCount > 0)
                {
                    radAnswerPresentationTypes.Enabled = false;
                    radFrontEndQuestionTypes.Enabled = false;
                    txtAnswers.ReadOnly = true;
                    cmdUnDeleteQuestion.Visible = false;
                    cmdDeleteQuestion.Visible = false;
                    chkAllowBlankAnswersInMenu.Enabled = false;
                    chkFirstAnswerIsFake.Enabled = false;
                }
                else
                {
                    radAnswerPresentationTypes.Enabled = true;
                    radFrontEndQuestionTypes.Enabled = true;
                    txtAnswers.ReadOnly = false;
                    cmdUnDeleteQuestion.Visible = ElementMap[ElementStringId].PendingDelete;
                    cmdDeleteQuestion.Visible = !ElementMap[ElementStringId].PendingDelete;
                    chkAllowBlankAnswersInMenu.Enabled = true;
                    chkFirstAnswerIsFake.Enabled = true;
                }

                if (SessionForm.FormType == FormTypes.Questionaire)
                {
                    divFrontEndQuestionTypes.Visible = true;

                    if (radFrontEndQuestionTypes.Items.Count == 0)
                    {
                        radFrontEndQuestionTypes.DataSource = FormHelper.ListFrontEndQuestionTypes();
                        radFrontEndQuestionTypes.DataValueField = "Key";
                        radFrontEndQuestionTypes.DataTextField = "Value";
                        radFrontEndQuestionTypes.DataBind();
                        radFrontEndQuestionTypes.SelectedIndex = 0;
                    }

                    if (radFrontEndQuestionTypes.Items.FindByValue(((int)userQuestionType).ToString()) != null)
                    {
                        radFrontEndQuestionTypes.SelectedValue = ((int)userQuestionType).ToString();
                    }

                    switch (userQuestionType)
                    {
                        case FormHelper.FrontEndQuestionTypes.Captcha:
                            {
                                ShowCaptchaControls(question);
                                break;
                            }
                        case FormHelper.FrontEndQuestionTypes.SingleLineOfText:
                            {
                                ShowSingleTextLineControls(question);
                                break;
                            } 
                        case FormHelper.FrontEndQuestionTypes.MultiLineText:
                            {
                                ShowMultiTextLineControls(question);
                                break;
                            } 
                        case FormHelper.FrontEndQuestionTypes.MenuToChooseFrom:
                            {
                                FormHelper.FrontEndMenuTypes presentationType = FormHelper.FrontEndMenuTypes.CheckBox;
                                if (question.FirstAnswerKey.HasValue)
                                {
                                    switch (question.Answers[question.FirstAnswerKey.Value].AnswerType)
                                    {
                                        case AnswerTypes.Checkbox: presentationType = FormHelper.FrontEndMenuTypes.CheckBox; break;
                                        case AnswerTypes.DropDown: presentationType = FormHelper.FrontEndMenuTypes.DropDown; break;
                                        case AnswerTypes.Radio: presentationType = FormHelper.FrontEndMenuTypes.Radio; break;
                                    }
                                    ShowMenuToChooseFromControls(question, presentationType);
                                }
                                break;
                            } 
                        case FormHelper.FrontEndQuestionTypes.FileUpload:
                            {
                                ShowFileUploadControls(question);
                                break;
                            } 
                    }
                }
                else if (SessionForm.FormType == FormTypes.Poll)
                {
                    FormHelper.FrontEndMenuTypes presentationType = FormHelper.FrontEndMenuTypes.CheckBox;
                    switch (question.Answers[question.FirstAnswerKey.Value].AnswerType)
                    {
                        case AnswerTypes.Checkbox: presentationType = FormHelper.FrontEndMenuTypes.CheckBox; break;
                        case AnswerTypes.DropDown: presentationType = FormHelper.FrontEndMenuTypes.DropDown; break;
                        case AnswerTypes.Radio: presentationType = FormHelper.FrontEndMenuTypes.Radio; break;
                    }
                    ShowMenuToChooseFromControls(question, presentationType);
                }
            }
        }

        private void ShowCaptchaControls(BOQuestion question)
        {
            chkAnswerIsRequired.Checked = true;
            chkAnswerIsRequired.Enabled = false;
        }

        private void ShowSingleTextLineControls(BOQuestion question)
        {
            separateInput.Visible = txtMaxChars.Visible = true;
            if (question.FirstAnswerKey.HasValue &&
                question.Answers[question.FirstAnswerKey.Value] != null &&
                question.Answers[question.FirstAnswerKey.Value].MaxChars.HasValue)
            {
                txtMaxChars.Text = question.Answers[question.FirstAnswerKey.Value].MaxChars.Value.ToString();
            }
        }

        private void ShowMultiTextLineControls(BOQuestion question)
        {
            separateInput.Visible = PanelNoOfLines.Visible = txtMaxChars.Visible = true;

            if (question.FirstAnswerKey.HasValue &&
                question.Answers[question.FirstAnswerKey.Value] != null)
            {
                if (question.Answers[question.FirstAnswerKey.Value].MaxChars.HasValue)
                {
                    txtMaxChars.Text = question.Answers[question.FirstAnswerKey.Value].MaxChars.Value.ToString();
                }
                if (question.Answers[question.FirstAnswerKey.Value].NumberOfRows.HasValue)
                {
                    txtNumberOfRows.Text = question.Answers[question.FirstAnswerKey.Value].NumberOfRows.Value.ToString();
                }
            }
        }

        private void ShowMenuToChooseFromControls(BOQuestion question, FormHelper.FrontEndMenuTypes presentationType)
        {
            separateInput.Visible = chkFirstAnswerIsFake.Visible = PanelAnswersList.Visible = chkAllowBlankAnswersInMenu.Visible = divAnswerPresentationTypes.Visible = true;

            if (radAnswerPresentationTypes.Items.Count == 0)
            {
                radAnswerPresentationTypes.DataSource = FormHelper.ListFrontEndMenuTypes();
                radAnswerPresentationTypes.DataValueField = "Key";
                radAnswerPresentationTypes.DataTextField = "Value";
                radAnswerPresentationTypes.DataBind();
                radAnswerPresentationTypes.SelectedIndex = 0;
            }

            chkAllowBlankAnswersInMenu.Checked = chkFirstAnswerIsFake.Checked = false;
            radAnswerPresentationTypes.SelectedValue = ((int)FormHelper.FrontEndMenuTypes.Radio).ToString();

            // Now load Session values
            if (question.FirstAnswerKey.HasValue)
            {
                if (radAnswerPresentationTypes.Items.FindByValue(((int)presentationType).ToString()) != null)
                {
                    radAnswerPresentationTypes.SelectedValue = ((int)presentationType).ToString();
                }
            }

            if (question.FirstAnswerKey.HasValue)
            {
                chkFirstAnswerIsFake.Checked = (question.Answers[question.FirstAnswerKey.Value]).IsFake;
            }

            chkFirstAnswerIsFake.Visible = presentationType == FormHelper.FrontEndMenuTypes.DropDown;

            if (question.LastAnswerKey.HasValue)
            {
                chkAllowBlankAnswersInMenu.Checked = (question.Answers[question.LastAnswerKey.Value].AdditionalFieldType == AdditionalFieldTypes.Text ? true : false);
            }

            if (presentationType == FormHelper.FrontEndMenuTypes.Radio ||
                presentationType == FormHelper.FrontEndMenuTypes.CheckBox)
            {
                chkAllowBlankAnswersInMenu.Visible = true;
            }
            else
            {
                chkAllowBlankAnswersInMenu.Visible = false;
            }

            txtAnswers.Text = "";
            foreach (BOAnswer answer in question.Answers.Values)
            {
                txtAnswers.Text += answer.Title + "\n";
            }
        }

        private void ShowFileUploadControls(BOQuestion question)
        {
            separateInput.Visible = PanelMaxFileSize.Visible = divAllowedMimeTypes.Visible = true;
            if (chkAllowedMimeTypes.Items.Count == 0)
            {
                chkAllowedMimeTypes.DataSource = FormHelper.ListAllowedFileTypes();
                chkAllowedMimeTypes.DataValueField = "Value";
                chkAllowedMimeTypes.DataTextField = "Key";
                chkAllowedMimeTypes.DataBind();
            }

            // Load Session values
            if (question.FirstAnswerKey.HasValue && question.Answers[question.FirstAnswerKey.Value] != null)
            {
                if (question.Answers[question.FirstAnswerKey.Value].MaxFileSize.HasValue)
                {
                    txtMaximumFileSize.Text = question.Answers[question.FirstAnswerKey.Value].MaxFileSize.Value.ToString();
                }

                string[] mimeTypes = question.Answers[question.FirstAnswerKey.Value].AllowedMimeTypes.Split('|');
                Dictionary<string, string> typeList = FormHelper.ListAllowedFileTypes();

                foreach (ListItem item in chkAllowedMimeTypes.Items)
                {
                    item.Selected = false;
                }

                foreach (string s in mimeTypes)
                {
                    foreach (ListItem item in chkAllowedMimeTypes.Items)
                    {
                        if (s.Contains(item.Value))
                        {
                            item.Selected = true;
                        }
                    }
                }
            }
        }

        #endregion Initialization

        #region First tab methods

        protected void ddlFormTypes_DataBound(object sender, EventArgs e)
        {
            foreach (ListItem item in ddlFormTypes.Items)
            {
                item.Text = item.Text;
            }
        }

        protected void ddlUpdateSectionTypes_DataBound(object sender, EventArgs e)
        {
            foreach (ListItem item in ddlUpdateSectionTypes.Items)
            {
                item.Text = item.Text;
            }
        }

        protected void cmdShowAddForm_Click(object sender, EventArgs e)
        {
            ClearSessionValues();
            FormTree_DataBind();

            MultiView1.ActiveViewIndex = 1;
        }

        protected void radFrontEndQuestionTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            int questionId = Int32.Parse(ElementStringId.Replace("Question", ""));
            BOQuestion question = BOForm.FindQuestion(SessionForm, questionId);
            FormHelper.FrontEndQuestionTypes userQuestionType = (FormHelper.FrontEndQuestionTypes)Enum.Parse(typeof(FormHelper.FrontEndQuestionTypes), radFrontEndQuestionTypes.SelectedValue);
            LoadQuestionControls(question, userQuestionType);
        }

        protected void rptAggregateAnswers_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            HtmlGenericControl spanInclusivePercentage = e.Item.FindControl("spanInclusivePercentage") as HtmlGenericControl;
            BOAnswer answer = e.Item.DataItem as BOAnswer;
            if (spanInclusivePercentage != null && answer != null )
            {
                spanInclusivePercentage.Visible = (answer.AnswerType != AnswerTypes.SingleFile && answer.AnswerType != AnswerTypes.SingleText);
            }
        }

        protected void rptSSSections_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if ((e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem) && e.Item.DataItem != null)
            {
                BOSection section = e.Item.DataItem as BOSection;
                Repeater rptSSQuestions = e.Item.FindControl("rptSSQuestions") as Repeater;

                rptSSQuestions.DataSource = section.Questions.Values;
                rptSSQuestions.DataBind();
            }
        }

        protected void rptSSQuestions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if ((e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem) && e.Item.DataItem != null)
            {
                BOQuestion question = e.Item.DataItem as BOQuestion;
                Repeater rptSSAnswers = e.Item.FindControl("rptSSAnswers") as Repeater;

                if (question.FirstAnswerKey.HasValue)
                {
                    if (question.Answers[question.FirstAnswerKey.Value].IsFake)
                    {
                        question.Answers.Remove(question.FirstAnswerKey.Value);
                    }
                }

                rptSSAnswers.DataSource = question.Answers.Values;
                rptSSAnswers.DataBind();
            }
        }

        protected void rptSSAnswers_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            HtmlTableRow multiRow = e.Item.FindControl("multiRow") as HtmlTableRow;
            HtmlTableRow singleRow = e.Item.FindControl("singleRow") as HtmlTableRow;
            HtmlTableCell answerValue = e.Item.FindControl("answerValue") as HtmlTableCell;
            HtmlGenericControl spanAdditionalField = e.Item.FindControl("spanAdditionalField") as HtmlGenericControl;

            BOAnswer answer = e.Item.DataItem as BOAnswer;

            if (spanAdditionalField != null && answerValue != null && singleRow != null && multiRow != null && answer != null && SessionSubmission != null)
            {
                multiRow.Visible = false;
                singleRow.Visible = false;
                spanAdditionalField.Visible = false;

                if (answer.AnswerType == AnswerTypes.SingleText)
                {
                    singleRow.Visible = true;
                    if (SessionSubmission.SubmittedQuestions.ContainsKey(answer.ParentId.Value))
                    {
                        if (SessionSubmission.SubmittedQuestions[answer.ParentId.Value].SubmittedAnswers.ContainsKey(answer.Id.Value))
                        {
                            answerValue.InnerText = SessionSubmission.SubmittedQuestions[answer.ParentId.Value].SubmittedAnswers[answer.Id.Value].SubmittedText;
                        }
                    }
                }
                else if (answer.AnswerType == AnswerTypes.SingleFile)
                {
                    singleRow.Visible = true;
                    if (SessionSubmission.SubmittedQuestions.ContainsKey(answer.ParentId.Value))
                    {
                        if (SessionSubmission.SubmittedQuestions[answer.ParentId.Value].SubmittedAnswers.ContainsKey(answer.Id.Value) &&
                            SessionSubmission.SubmittedQuestions[answer.ParentId.Value].SubmittedAnswers[answer.Id.Value].SubmittedFile != null )
                        {
                            answerValue.InnerText = SessionSubmission.SubmittedQuestions[answer.ParentId.Value].SubmittedAnswers[answer.Id.Value].SubmittedFile.Name;
                            if (SessionSubmission.SubmittedQuestions[answer.ParentId.Value].SubmittedAnswers[answer.Id.Value].SubmittedFile.Folder == null )
                            {
                                answerValue.InnerText = "$file_removed_from_folder" + " " + answerValue.InnerText;
                            }
                        }
                    }
                }
                else
                {
                    if (SessionSubmission.SubmittedQuestions.ContainsKey(answer.ParentId.Value))
                    {
                        if (SessionSubmission.SubmittedQuestions[answer.ParentId.Value].SubmittedAnswers.ContainsKey(answer.Id.Value))
                        {
                            multiRow.Visible = true;
                            if (answer.AdditionalFieldType == AdditionalFieldTypes.Text)
                            {
                                spanAdditionalField.Visible = true;
                                spanAdditionalField.InnerText = ": " + SessionSubmission.SubmittedQuestions[answer.ParentId.Value].SubmittedAnswers[answer.Id.Value].SubmittedText;
                            }
                        }
                    }
                }
            }
        }

        protected void radAnswerPresentationTypes_SelectIndexChanged(object sender, EventArgs e)
        {
            int questionId = Int32.Parse(ElementStringId.Replace("Question", ""));
            BOQuestion question = BOForm.FindQuestion(SessionForm, questionId);
            FormHelper.FrontEndMenuTypes presentationType = (FormHelper.FrontEndMenuTypes)Enum.Parse(typeof(FormHelper.FrontEndMenuTypes), radAnswerPresentationTypes.SelectedValue);
            ShowMenuToChooseFromControls(question, presentationType);
        }

        protected void formGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.DataItem != null)
            {
                var HyperLinkExport1 = e.Row.FindControl("HyperLinkExport1") as HyperLink;
                var HyperLinkExport2 = e.Row.FindControl("HyperLinkExport2") as HyperLink;

                var form = e.Row.DataItem as BOForm;

                if (HyperLinkExport1 != null && HyperLinkExport2 != null && form != null)
                {
                    HyperLinkExport1.Visible = form.SubmissionCount > 0;
                    HyperLinkExport2.Visible = form.SubmissionCount > 0;

                    HyperLinkExport1.NavigateUrl = string.Format(HyperLinkExport1.NavigateUrl, form.Id);
                    HyperLinkExport2.NavigateUrl = string.Format(HyperLinkExport2.NavigateUrl, form.Id);

                }
            }
        }

        protected void formGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int formId = 0;
            Int32.TryParse(e.CommandArgument.ToString(), out formId);

            if (formId > 0)
            {
                string command = e.CommandName;

                switch (command)
                {
                    case "EditForm":
                        {
                            ElementStringId = "Form" + formId;
                            SessionForm = helper.Get(formId);
                            PopulateElementMap(SessionForm);
                            FormTree_DataBind();
                            MultiView1.ActiveViewIndex = 1;
                            break;
                        }

                    case "CopyAsNew":
                        {
                            BOForm form = helper.Get(formId);
                            BOForm newForm = null;

                            ElementMap = null;

                            if (form != null)
                            {
                                newForm = new BOForm();
                                newForm.LoadContent(form);
                                newForm.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
                                newForm.MissingTranslation = false;

                                if (form.MissingTranslation)
                                    newForm.Title = form.Title;

                                newForm.ContentId = null;
                                newForm.FormType = form.FormType;
                                newForm.ParentId = null;
                                newForm.SendToString = form.SendToString;
                                newForm.AllowModifyInSubmission = form.AllowModifyInSubmission;
                                newForm.AllowMultipleSubmissions = form.AllowMultipleSubmissions;

                                Random rand = new Random((int)DateTime.Now.Ticks);
                                int newFormId = rand.Next();
                                newForm.Id = newFormId;
                                ElementMap = new Dictionary<string, BOAdminFormElement>();
                                ElementMap.Add("Form" + newFormId, new BOAdminFormElement("Form" + newFormId, "", false, true, form.Title));

                                foreach (int i in form.Sections.Keys)
                                {
                                    BOSection section = form.Sections[i];
                                    BOSection newSection = new BOSection();
                                    newSection.LoadContent(section);
                                    newSection.MissingTranslation = false;

                                    newSection.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
                                    if (section.MissingTranslation)
                                        newSection.Title = section.Title;

                                    newSection.ContentId = null;
                                    newSection.Idx = section.Idx;
                                    newSection.ParentId = newFormId;
                                    newSection.SectionType = section.SectionType;

                                    rand = new Random(section.Id.Value + newFormId);
                                    int newSectionId = rand.Next();
                                    newSection.Id = newSectionId;
                                    ElementMap.Add("Section" + newSectionId, new BOAdminFormElement("Section" + newSectionId, "Form" + newFormId, false, true, section.Title));

                                    foreach (int j in section.Questions.Keys)
                                    {
                                        BOQuestion question = section.Questions[j];
                                        BOQuestion newQuestion = new BOQuestion();
                                        newQuestion.LoadContent(question);
                                        newQuestion.MissingTranslation = false;

                                        newQuestion.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
                                        if (question.MissingTranslation)
                                            newQuestion.Title = question.Title;

                                        newQuestion.ContentId = null;
                                        newQuestion.Idx = question.Idx;
                                        newQuestion.IsAnswerRequired = question.IsAnswerRequired;
                                        newQuestion.ParentId = newSectionId;
                                        newQuestion.ValidationType = question.ValidationType;

                                        rand = new Random(question.Id.Value + newSectionId + newFormId);
                                        int newQuestionId = rand.Next();
                                        newQuestion.Id = newQuestionId;
                                        ElementMap.Add("Question" + newQuestionId, new BOAdminFormElement("Question" + newQuestionId, "Section" + newSectionId, false, true, question.Title));

                                        foreach (int k in question.Answers.Keys)
                                        {
                                            BOAnswer answer = question.Answers[k];
                                            BOAnswer newAnswer = new BOAnswer();

                                            newAnswer.LoadContent(answer);
                                            newAnswer.MissingTranslation = false;

                                            newAnswer.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
                                            if (answer.MissingTranslation)
                                                newAnswer.Title = answer.Title;

                                            newAnswer.ContentId = null;
                                            newAnswer.ParentId = newQuestionId;
                                            newAnswer.AdditionalFieldType = answer.AdditionalFieldType;
                                            newAnswer.AllowedMimeTypes = answer.AllowedMimeTypes;
                                            newAnswer.AnswerType = answer.AnswerType;
                                            newAnswer.Idx = answer.Idx;
                                            newAnswer.MaxChars = answer.MaxChars;
                                            newAnswer.MaxFileSize = answer.MaxFileSize;
                                            newAnswer.NumberOfRows = answer.NumberOfRows;
                                            newAnswer.IsFake = answer.IsFake;

                                            rand = new Random(answer.Id.Value + newQuestionId + newSectionId + newFormId);
                                            int newAnswerId = rand.Next();
                                            newAnswer.Id = newAnswerId;

                                            newQuestion.Answers.Add(newAnswerId, newAnswer);
                                        }

                                        newSection.Questions.Add(newQuestionId, newQuestion);
                                    }

                                    newForm.Sections.Add(newSectionId, newSection);
                                }
                            }

                            SessionForm = newForm;
                            ElementStringId = "Form" + SessionForm.Id.Value;
                            FormTree_DataBind();
                            MultiView1.ActiveViewIndex = 1;

                            break;
                        }
                }
            }
        }

        #endregion First tab methods

        private void ClearSessionValues()
        {
            SessionSubmission = null;
            SessionForm = null;
            ElementStringId = null;
            ElementMap = null;
            FormSubmissions = null;
        }

        #region Second tab methods

        protected void cmdCancelButton_Click(object sender, EventArgs e)
        {
            ClearSessionValues();

            MultiView1.ActiveViewIndex = 0;
        }

        protected void cmdSaveForm_Click(object sender, EventArgs e)
        {
            if (SessionForm != null)
            {
                BOForm formForSaving = PrepareSessionFormForSaving();
                helper.Change(formForSaving);

                ClearSessionValues();
                SessionForm = helper.Get(formForSaving.Id.Value);
                ElementStringId = "Form" + SessionForm.Id.Value;

                PopulateElementMap(SessionForm);
                FormTree_DataBind();
                LoadFormTabControls();
            }
        }

        private BOForm PrepareSessionFormForSaving()
        {
            BOForm formForSaving = new BOForm();

            formForSaving.LoadContent(SessionForm);
            formForSaving.FormType = SessionForm.FormType;
            formForSaving.ParentId = null;
            formForSaving.SendToString = SessionForm.SendToString;
            formForSaving.Id = (ElementMap["Form" + SessionForm.Id.Value].NewlyAdded ? null : SessionForm.Id);
            formForSaving.AllowModifyInSubmission = SessionForm.AllowModifyInSubmission;
            formForSaving.AllowMultipleSubmissions = SessionForm.AllowMultipleSubmissions;
            formForSaving.CompletionRedirect = SessionForm.CompletionRedirect;
            formForSaving.SubmissionCount = SessionForm.SubmissionCount;

            foreach (BOSection section in SessionForm.Sections.Values)
            {
                BOSection sectionForSaving = new BOSection();
                sectionForSaving.LoadContent(section);
                sectionForSaving.Idx = section.Idx;
                sectionForSaving.ParentId = formForSaving.Id;
                sectionForSaving.SectionType = section.SectionType;

                sectionForSaving.OnClientClick = section.OnClientClick;

                sectionForSaving.Id = (ElementMap["Section" + section.Id.Value].NewlyAdded ? null : section.Id);

                foreach (BOQuestion question in section.Questions.Values)
                {
                    BOQuestion questionForSaving = new BOQuestion();
                    questionForSaving.LoadContent(question);
                    questionForSaving.Idx = question.Idx;
                    questionForSaving.IsAnswerRequired = question.IsAnswerRequired;
                    questionForSaving.ParentId = sectionForSaving.Id;
                    questionForSaving.ValidationType = question.ValidationType;
                    questionForSaving.Id = (ElementMap["Question" + question.Id.Value].NewlyAdded ? null : question.Id);

                    foreach (BOAnswer answer in question.Answers.Values)
                    {
                        BOAnswer answerForSaving = new BOAnswer();
                        answerForSaving.LoadContent(answer);
                        answerForSaving.AdditionalFieldType = answer.AdditionalFieldType;
                        answerForSaving.AllowedMimeTypes = answer.AllowedMimeTypes;
                        answerForSaving.AnswerType = answer.AnswerType;
                        answerForSaving.Idx = answer.Idx;
                        answerForSaving.MaxChars = answer.MaxChars;
                        answerForSaving.MaxFileSize = answer.MaxFileSize;
                        answerForSaving.NumberOfRows = answer.NumberOfRows;
                        answerForSaving.ParentId = questionForSaving.Id;
                        answerForSaving.Id = null;
                        answerForSaving.IsFake = answer.IsFake;

                        questionForSaving.Answers.Add(answer.Id.Value, answerForSaving);
                    }

                    if (!ElementMap["Question" + question.Id.Value].PendingDelete)
                    {
                        sectionForSaving.Questions.Add(question.Id.Value, questionForSaving);
                    }
                }
                
                if (!ElementMap["Section" + section.Id.Value].PendingDelete)
                {
                    formForSaving.Sections.Add(section.Id.Value, sectionForSaving);
                }
            }

            return formForSaving;
        }

        protected void cmdSaveFormAndClose_Click(object sender, EventArgs e)
        {
            if (SessionForm != null)
            {
                BOForm formForSaving = PrepareSessionFormForSaving();

                helper.Change(formForSaving);

                ClearSessionValues();

                MultiView1.ActiveViewIndex = 0;
            }
        }

        public void FormTree_SelectedNodeChanged(object sender, EventArgs e)
        {
            ElementStringId = FormTree.SelectedNode.Value;
            LoadFormTabControls();
        }

        protected void cmdAddForm_Click(object sender, EventArgs e)
        {
            BOForm form = new BOForm();

            Random rand = new Random();
            form.Id = rand.Next();

            form.Title = txtAddForm.Text;
            form.SendToString = "";
            form.SubTitle = "";
            form.Teaser = "";
            form.Html = "";
            form.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
            form.FormType = FormTypes.Questionaire;
            form.AllowModifyInSubmission = false;
            form.AllowMultipleSubmissions = false;
            form.CompletionRedirect = "";

            SessionForm = form;
            ElementStringId = "Form" + form.Id.Value;

            ElementMap = new Dictionary<string, BOAdminFormElement>();
            ElementMap.Add(ElementStringId, new BOAdminFormElement(ElementStringId, "", false, true, form.Title));

            FormSubmissions = null;
            txtAddForm.Text = "";
            FormTree_DataBind();
            LoadFormTabControls();
        }

        protected void cmdUpdateForm_Click(object sender, EventArgs e)
        {
            if (SessionForm != null)
            {
                ElementMap["Form" + SessionForm.Id.Value].Title = txtFormName.Text;
                SessionForm.Title = txtFormName.Text;
                SessionForm.SubTitle = InputFormPrivateName.Text;
                SessionForm.Html = txtFormThankYouNote.Text;
                SessionForm.Teaser = txtFormDescription.Text;
                SessionForm.SendToString = txtSendTo.Text;
                SessionForm.FormType = (FormTypes)Enum.Parse(typeof(FormTypes), ddlFormTypes.SelectedValue);
                SessionForm.AllowModifyInSubmission = chkAllowModifyInSubmission.Checked;
                SessionForm.AllowMultipleSubmissions = chkAllowMultipleSubmissions.Checked;
                SessionForm.CompletionRedirect = InputCompletionRedirect.Text;

                foreach (BOSection section in SessionForm.Sections.Values)
                {
                    switch (ddlUpdateSectionTypes.SelectedValue)
                    {
                        case BOSection.SECTION_TYPE_MULTI_PAGE: section.SectionType = SectionTypes.MultiPage; break;
                        case BOSection.SECTION_TYPE_SINGLE_PAGE: section.SectionType = SectionTypes.SinglePage; break;
                    }
                }

                FormTree_DataBind();
                LoadFormTabControls();
            }
        }

        protected void cmdAddSection_Click(object sender, EventArgs e)
        {
            BOSection section = new BOSection();

            Random rand = new Random();
            section.Id = rand.Next();
            section.ParentId = SessionForm.Id;
            section.Idx = SessionForm.Sections.Count + 1;
            section.Title = txtAddSection.Text;
            section.SubTitle = "";
            section.Teaser = "";
            section.Html = "";
            section.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
            section.SectionType = SectionTypes.MultiPage;
            section.OnClientClick = "";

            switch (ddlUpdateSectionTypes.SelectedValue)
            {
                case BOSection.SECTION_TYPE_MULTI_PAGE: section.SectionType = SectionTypes.MultiPage; break;
                case BOSection.SECTION_TYPE_SINGLE_PAGE: section.SectionType = SectionTypes.SinglePage; break;
            }

            ElementMap.Add("Section" + section.Id.Value, new BOAdminFormElement("Section" + section.Id.Value, "Form" + SessionForm.Id.Value, false, true, section.Title));
            SessionForm.Sections.Add(section.Id.Value, section);
            txtAddSection.Text = "";
            FormTree_DataBind();
            LoadFormTabControls();
        }

        protected void cmdUpdateSection_Click(object sender, EventArgs e)
        {
            int sectionId = Int32.Parse(ElementStringId.Replace("Section", ""));

            if (SessionForm.Sections[sectionId] != null)
            {
                BOSection section = SessionForm.Sections[sectionId];

                section.Title = txtSectionName.Text;
                section.Teaser = InputSectionDescription.Text;
                section.OnClientClick = InputSectionOnClientClick.Text;
                ElementMap["Section" + section.Id.Value].Title = txtSectionName.Text;

                FormTree_DataBind();
                LoadFormTabControls();
            }
        }

        protected void cmdAddQuestion_Click(object sender, EventArgs e)
        {
            int sectionId = Int32.Parse(ElementStringId.Replace("Section", ""));

            if (SessionForm.Sections[sectionId] != null)
            {
                BOSection section = SessionForm.Sections[sectionId];
                BOQuestion question = new BOQuestion();

                Random rand = new Random();
                question.Id = rand.Next();
                question.ParentId = sectionId;
                question.Idx = section.Questions.Count + 1;
                question.Title = txtAddQuestion.Text;
                question.SubTitle = "";
                question.Teaser = "";
                question.Html = "";
                question.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
                question.IsAnswerRequired = false;

                ElementMap.Add("Question" + question.Id.Value, new BOAdminFormElement("Question" + question.Id.Value, "Section" + section.Id.Value, false, true, question.Title));

                rand = new Random(question.Id.Value);

                // add default answer
                BOAnswer answer = new BOAnswer(rand.Next(), question.Id, 1, "","", "", "", Thread.CurrentThread.CurrentCulture.LCID, AnswerTypes.SingleText, 255, 0, AdditionalFieldTypes.None, false);
                question.Answers.Add(answer.Id.Value, answer);
                section.Questions.Add(question.Id.Value, question);
                txtAddQuestion.Text = "";
                FormTree_DataBind();
                LoadFormTabControls();
            }
        }

        protected void cmdUnDeleteSection_Click(object sender, EventArgs e)
        {
            int sectionId = Int32.Parse(ElementStringId.Replace("Section", ""));

            if (SessionForm.Sections[sectionId] != null)
            {
                BOSection section = SessionForm.Sections[sectionId];
                ElementMap[ElementStringId].PendingDelete = false;

                FormTree_DataBind();
                LoadFormTabControls();
            }
        }

        protected void cmdUnDeleteQuestion_Click(object sender, EventArgs e)
        {
            int questionId = Int32.Parse(ElementStringId.Replace("Question", ""));
            BOQuestion question = BOForm.FindQuestion(SessionForm, questionId);
            ElementMap[ElementStringId].PendingDelete = false;

            FormTree_DataBind();
            LoadFormTabControls();
        }

        protected void cmdDeleteSection_Click(object sender, EventArgs e)
        {
            int sectionId = Int32.Parse(ElementStringId.Replace("Section", ""));

            if (SessionForm.Sections[sectionId] != null)
            {
                BOSection section = SessionForm.Sections[sectionId];
                ElementMap[ElementStringId].PendingDelete = true;

                FormTree_DataBind();
                LoadFormTabControls();
            }
        }

        protected void cmdDeleteQuestion_Click(object sender, EventArgs e)
        {
            int questionId = Int32.Parse(ElementStringId.Replace("Question", ""));
            BOQuestion question = BOForm.FindQuestion(SessionForm, questionId);
            ElementMap[ElementStringId].PendingDelete = true;

            FormTree_DataBind();
            LoadFormTabControls();
        }

        protected void cmdUpdateQuestion_Click(object sender, EventArgs e)
        {
            int questionId = Int32.Parse(ElementStringId.Replace("Question", ""));
            BOQuestion question = BOForm.FindQuestion(SessionForm, questionId);

            if (question != null && question.Id.HasValue && question.ParentId.HasValue)
            {
                ElementMap["Question" + question.Id.Value].Title = txtQuestionText.Text;
                question.Title = txtQuestionText.Text;
                question.Teaser = txtQuestionDescription.Text;
                question.IsAnswerRequired = chkAnswerIsRequired.Checked;
                question.Answers.Clear();

                FormHelper.FrontEndQuestionTypes userQuestionType;

                if (SessionForm.FormType == FormTypes.Questionaire)
                {
                    userQuestionType = (FormHelper.FrontEndQuestionTypes)Enum.Parse(typeof(FormHelper.FrontEndQuestionTypes), radFrontEndQuestionTypes.SelectedValue);
                }
                else
                {
                    userQuestionType = FormHelper.FrontEndQuestionTypes.MenuToChooseFrom;
                }

                Random rand = new Random();

                switch (userQuestionType)
                {
                    case FormHelper.FrontEndQuestionTypes.MenuToChooseFrom:
                        {
                            question.ValidationType = ValidationTypes.None;
                            if (radAnswerPresentationTypes.SelectedValue != null && !string.IsNullOrEmpty(txtAnswers.Text))
                            {
                                FormHelper.FrontEndMenuTypes selectedType = (FormHelper.FrontEndMenuTypes)Enum.Parse(typeof(FormHelper.FrontEndMenuTypes), radAnswerPresentationTypes.SelectedValue);
                                AnswerTypes answerType = AnswerTypes.Radio;

                                switch (selectedType)
                                {
                                    case FormHelper.FrontEndMenuTypes.Radio: answerType = AnswerTypes.Radio; break;
                                    case FormHelper.FrontEndMenuTypes.DropDown: answerType = AnswerTypes.DropDown; break;
                                    case FormHelper.FrontEndMenuTypes.CheckBox: answerType = AnswerTypes.Checkbox; break;
                                }

                                string[] answerStrings = txtAnswers.Text.Trim().Split('\n');
                                foreach (string answerText in answerStrings)
                                {
                                    rand = (question.LastAnswerKey.HasValue ? new Random(question.LastAnswerKey.Value) : new Random());
                                    BOAnswer answer = new BOAnswer(rand.Next(), question.Id, question.Answers.Count + 1, answerText, "", "", "", Thread.CurrentThread.CurrentCulture.LCID, answerType, 0, 0, AdditionalFieldTypes.None, false);
                                    question.Answers.Add(answer.Id.Value, answer);
                                }

                                if (question.FirstAnswerKey.HasValue && answerType == AnswerTypes.DropDown)
                                {
                                    question.Answers[question.FirstAnswerKey.Value].IsFake = chkFirstAnswerIsFake.Checked;
                                }
                                else
                                {
                                    question.Answers[question.FirstAnswerKey.Value].IsFake = false;
                                }

                                if ((answerType == AnswerTypes.Checkbox || answerType == AnswerTypes.Radio) &&
                                    chkAllowBlankAnswersInMenu.Checked &&
                                    question.LastAnswerKey.HasValue &&
                                    question.Answers[question.LastAnswerKey.Value] != null)
                                {
                                    question.Answers[question.LastAnswerKey.Value].AdditionalFieldType = AdditionalFieldTypes.Text;
                                }
                            }
                        } break;
                    case FormHelper.FrontEndQuestionTypes.SingleLineOfText:
                        {
                            question.ValidationType = ValidationTypes.None;
                            BOAnswer answer = new BOAnswer(rand.Next(), question.Id, 1, "", "", "", "", Thread.CurrentThread.CurrentCulture.LCID, AnswerTypes.SingleText, FormatTool.GetInteger(txtMaxChars.Text), 0, AdditionalFieldTypes.None, false);
                            question.Answers.Add(answer.Id.Value, answer);
                        } break;
                    case FormHelper.FrontEndQuestionTypes.Captcha:
                        {
                            question.ValidationType = ValidationTypes.Captcha;
                            BOAnswer answer = new BOAnswer(rand.Next(), question.Id, 1, "", "", "", "", Thread.CurrentThread.CurrentCulture.LCID, AnswerTypes.SingleText, 10, 1, AdditionalFieldTypes.None, false);
                            question.Answers.Add(answer.Id.Value, answer);
                            break;
                        }
                    case FormHelper.FrontEndQuestionTypes.MultiLineText:
                        {
                            question.ValidationType = ValidationTypes.None;
                            BOAnswer answer = new BOAnswer(rand.Next(), question.Id, 1, "", "", "", "", Thread.CurrentThread.CurrentCulture.LCID, AnswerTypes.SingleText, FormatTool.GetInteger(txtMaxChars.Text), FormatTool.GetInteger(txtNumberOfRows.Text), AdditionalFieldTypes.None, false);
                            question.Answers.Add(answer.Id.Value, answer);
                        } break;
                    case FormHelper.FrontEndQuestionTypes.FileUpload:
                        {
                            question.ValidationType = ValidationTypes.None;
                            BOAnswer answer = new BOAnswer(rand.Next(), question.Id, 1, "", "", "", "", Thread.CurrentThread.CurrentCulture.LCID, AnswerTypes.SingleFile, 0, 0, AdditionalFieldTypes.None, false);
                            answer.MaxFileSize = (FormatTool.GetInteger(txtMaximumFileSize.Text) <= 0 ? (int?)null : FormatTool.GetInteger(txtMaximumFileSize.Text));
                            foreach (ListItem item in chkAllowedMimeTypes.Items)
                            {
                                if (item.Selected)
                                {
                                    answer.AllowedMimeTypes += item.Value + "|";
                                }
                            }

                            question.Answers.Add(answer.Id.Value, answer);
                        } break;
                    case FormHelper.FrontEndQuestionTypes.VAT:
                        {
                            question.ValidationType = ValidationTypes.VAT;
                            BOAnswer answer = new BOAnswer(rand.Next(), question.Id, 1, "", "", "", "", Thread.CurrentThread.CurrentCulture.LCID, AnswerTypes.SingleText, 0, 0, AdditionalFieldTypes.None, false);
                            question.Answers.Add(answer.Id.Value, answer);
                            break;
                        }
                    case FormHelper.FrontEndQuestionTypes.Telephone :
                        {
                            question.ValidationType = ValidationTypes.Telephone;
                            BOAnswer answer = new BOAnswer(rand.Next(), question.Id, 1, "", "", "", "", Thread.CurrentThread.CurrentCulture.LCID, AnswerTypes.SingleText, 0, 0, AdditionalFieldTypes.None, false);
                            question.Answers.Add(answer.Id.Value, answer);
                            break;
                        }
                    case FormHelper.FrontEndQuestionTypes.NumericalValue:
                        {
                            question.ValidationType = ValidationTypes.Numeric;
                            BOAnswer answer = new BOAnswer(rand.Next(), question.Id, 1, "", "", "", "", Thread.CurrentThread.CurrentCulture.LCID, AnswerTypes.SingleText, 0, 0, AdditionalFieldTypes.None, false);
                            question.Answers.Add(answer.Id.Value, answer);
                        } break;
                    case FormHelper.FrontEndQuestionTypes.DateTime:
                        {
                            question.ValidationType = ValidationTypes.DateTime;
                            BOAnswer answer = new BOAnswer(rand.Next(), question.Id, 1, "", "", "", "", Thread.CurrentThread.CurrentCulture.LCID, AnswerTypes.SingleText, 0, 0, AdditionalFieldTypes.None, false);
                            question.Answers.Add(answer.Id.Value, answer);
                        } break;
                    case FormHelper.FrontEndQuestionTypes.Time:
                        {
                            question.ValidationType = ValidationTypes.Time;
                            BOAnswer answer = new BOAnswer(rand.Next(), question.Id, 1, "", "", "", "", Thread.CurrentThread.CurrentCulture.LCID, AnswerTypes.SingleText, 0, 0, AdditionalFieldTypes.None, false);
                            question.Answers.Add(answer.Id.Value, answer);
                        } break;
                    case FormHelper.FrontEndQuestionTypes.Email:
                        {
                            question.ValidationType = ValidationTypes.Email;
                            BOAnswer answer = new BOAnswer(rand.Next(), question.Id, 1, "", "", "", "", Thread.CurrentThread.CurrentCulture.LCID, AnswerTypes.SingleText, 0, 0, AdditionalFieldTypes.None, false);
                            question.Answers.Add(answer.Id.Value, answer);
                        } break;
                    case FormHelper.FrontEndQuestionTypes.Integer:
                        {
                            question.ValidationType = ValidationTypes.Integer;
                            BOAnswer answer = new BOAnswer(rand.Next(), question.Id, 1, "", "", "", "", Thread.CurrentThread.CurrentCulture.LCID, AnswerTypes.SingleText, 0, 0, AdditionalFieldTypes.None, false);
                            question.Answers.Add(answer.Id.Value, answer);
                        } break;
                }

                FormTree_DataBind();
                LoadFormTabControls();
            }
        }

        public void FormTree_DataBind()
        {
            if (ElementMap != null)
            {
                TreeNodeCollection treeNodes = this.GenerateTreeNodes(ElementMap);
                FormTree.Nodes.Clear();
                if (treeNodes.Count > 0)
                {
                    FormTree.Nodes.Add(treeNodes[0]);
                    FormTree.ExpandAll();
                    string rootStringId = treeNodes[0].Value;

                    if (ElementStringId == null && !string.IsNullOrEmpty(rootStringId))
                    {
                        ElementStringId = rootStringId;
                        TreeNode rootNode = treeNodes[0];
                        if (rootNode != null)
                        {
                            rootNode.Select();
                        }
                    }
                }
            }
            else
            {
                FormTree.Nodes.Clear();
            }
        }

        protected void PopulateElementMap(BOForm form)
        {
            if (form != null)
            {
                ElementMap = new Dictionary<string, BOAdminFormElement>();
                ElementMap.Add("Form" + form.Id.Value, new BOAdminFormElement("Form" + form.Id.Value, "", false, false, form.Title));

                foreach (BOSection section in form.Sections.Values)
                {
                    ElementMap.Add("Section" + section.Id.Value, new BOAdminFormElement("Section" + section.Id.Value, "Form" + form.Id.Value, false, false, section.Title));

                    foreach (BOQuestion question in section.Questions.Values)
                    {
                        ElementMap.Add("Question" + question.Id.Value, new BOAdminFormElement("Question" + question.Id.Value, "Section" + section.Id.Value, false, false, question.Title));
                    }
                }
            }
        }

        public TreeNodeCollection GenerateTreeNodes(Dictionary<string, BOAdminFormElement> elements)
        {
            TreeNodeCollection nodeColl = new TreeNodeCollection();

            TreeNode root = null;
            
            foreach (BOAdminFormElement element in elements.Values)
            {
                if (string.IsNullOrEmpty(element.ParentStringId))
                {
                    root = new TreeNode(element.Title, element.StringId);

                    if (element.PendingDelete)
                        root.ImageUrl = "/Res/brisanje.gif";
                    else
                        root.ImageUrl = "/Res/objavljeno.gif";

                    nodeColl.Add(root);
                }
            }

            AddChildren(root, elements);

            return nodeColl;
        }

        private static void AddChildren(TreeNode node, IDictionary<string, BOAdminFormElement> elements)
        {
            foreach (BOAdminFormElement element in elements.Values)
            {
                if (element.ParentStringId == node.Value)
                {
                    node.ChildNodes.Add(new TreeNode(element.Title, element.StringId));
                }
            }

            foreach (TreeNode childNode in node.ChildNodes)
            {
                AddChildren(childNode, elements);
            }
        }

        //private static void AddTreeNode(TreeNodeCollection coll, string parentNodeValue, TreeNode node)
        //{
        //    if (coll != null)
        //    {
        //        if (string.IsNullOrEmpty(parentNodeValue))
        //        {
        //            coll.Add(node);
        //            return;
        //        }
        //        else
        //        {
        //            foreach (TreeNode tn in coll)
        //            {
        //                if (tn.Value == parentNodeValue)
        //                {
        //                    tn.ChildNodes.Add(node);
        //                    break;
        //                }
        //                else
        //                {
        //                    AddTreeNode(tn.ChildNodes, parentNodeValue, node);
        //                    continue;
        //                }
        //            }
        //        }
        //    }
        //}

        #endregion Second tab methods
    }


    public class FormHelper
    {
        public enum FrontEndQuestionTypes { SingleLineOfText = 1, MultiLineText, MenuToChooseFrom, NumericalValue, DateTime, Time, FileUpload, Integer, Email, Captcha, VAT, Telephone }
        public enum FrontEndMenuTypes { Radio = 1, CheckBox, DropDown }

        protected static BForm formB = new BForm();

        public int SelectCount()
        {
            return (int)HttpContext.Current.Items["rowCount"];
        }

        public List<BOFormSubmission> ListFormSubmissions(int formId, int recordsPerPage, int firstRecordIndex, string sortBy)
        {
            PagedList<BOFormSubmission> submissions = formB.ListFormSubmissions(formId, new ListingState(recordsPerPage, firstRecordIndex, SortDir.Descending, sortBy));
            HttpContext.Current.Items["submissionCount"] = submissions.AllRecords;
            return submissions;
        }

        public List<BOFormSubmission> ListFormSubmissions(int formId)
        {
            return formB.ListFormSubmissions(formId, new ListingState());
        }

        public int FormSubmissionCount()
        {
            return (int)HttpContext.Current.Items["submissionCount"];
        }

        public PagedList<BOForm> Select(int recordsPerPage, int firstRecordIndex, string sortDirection, string sortBy)
        {
            PagedList<BOForm> forms = formB.ListUnCached(new ListingState(recordsPerPage, firstRecordIndex, (sortDirection.ToLower() == "asc" ? SortDir.Ascending : SortDir.Descending), sortBy), false, Thread.CurrentThread.CurrentCulture.LCID);
            HttpContext.Current.Items["rowCount"] = forms.AllRecords;
            return forms;
        }

        public void Change(BOForm form)
        {
            formB.Change(form);
        }

        public void DeleteForm(int Id)
        {
            formB.DeleteForm(Id);
        }

        public BOForm Get(int Id)
        {
            return formB.GetUnCached(Id, true);
        }

        public BOFormSubmission GetFormSubmission(int submissionId)
        {
            return formB.GetFormSubmission(submissionId);
        }

        public static List<string> ListFormTypes()
        {
            return BForm.ListFormTypes();
        }

        public static List<string> ListSectionTypes()
        {
            return BForm.ListSectionTypes();
        }

        public static Dictionary<int, string> ListFrontEndQuestionTypes()
        {
            Dictionary<int, string> types = new Dictionary<int, string>();

            foreach (int val in Enum.GetValues(typeof(FrontEndQuestionTypes)))
            {
                types.Add(val, Enum.GetName(typeof(FrontEndQuestionTypes), val));
            }

            return types;
        }

        public static Dictionary<int, string> ListFrontEndMenuTypes()
        {
            Dictionary<int, string> types = new Dictionary<int, string>();

            foreach (int val in Enum.GetValues(typeof(FrontEndMenuTypes)))
            {
                types.Add(val, Enum.GetName(typeof(FrontEndMenuTypes), val));
            }

            return types;
        }

        public static Dictionary<string, string> ListAllowedFileTypes()
        {
            Dictionary<string, string> types = new Dictionary<string, string>();

            types.Add("gif", "image/gif");
            types.Add("jpg", "image/jpeg;image/pjpeg");
            types.Add("png", "image/png");
            types.Add("psd", "image/psd");
            types.Add("bmp", "image/bmp");
            types.Add("tif", "image/tiff");
            types.Add("doc", "application/msword");
            types.Add("docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            types.Add("pdf", "application/pdf");
            types.Add("rtf", "text/richtext");
            types.Add("mp3", "audio/mpeg;audio/mpeg3;audio/x-mpeg-3");
            types.Add("ppt", "application/vnd.ms-powerpoint;application/ppt");
            types.Add("html", "text/html");
            types.Add("mpeg", "video/mpeg");
            types.Add("zip", "application/zip");
            types.Add("xml", "application/xml");
            types.Add("xls", "application/excel");

            return types;
        }
    }

    [Serializable]
    public class BOAdminFormElement
    {
        private string stringId = "";
        private string parentStringId = "";
        private bool newlyAdded;
        private bool pendingDelete;
        private string title;

        public BOAdminFormElement(string stringId, string parentStringId, bool pendingDelete, bool newlyAdded, string title) 
        {
            this.stringId = stringId;
            this.parentStringId = parentStringId;
            this.pendingDelete = pendingDelete;
            this.newlyAdded = newlyAdded;
            this.title = title;
        }

        public bool PendingDelete { get { return pendingDelete; } set { pendingDelete = value; } }
        public bool NewlyAdded { get { return newlyAdded; } set { newlyAdded = value; } }
        public string StringId { get { return stringId; } set { stringId = value; } }
        public string ParentStringId { get { return parentStringId; } set { parentStringId = value; } }
        public string Title { get { return title; } set { title = value; } }
    }

    public class FormOverallTrend
    {
        int submittedFormCount = 0;
        string date;

        public FormOverallTrend(int submittedFormCount, string date)
        {
            this.submittedFormCount = submittedFormCount;
            this.date = date;
        }

        public int SubmittedFormCount
        {
            get { return submittedFormCount; }
            set { submittedFormCount = value; }
        }

        public string Date
        {
            get { return date; }
            set { date = value; }
        }
    }
}
