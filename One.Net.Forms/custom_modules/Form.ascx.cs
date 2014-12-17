using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using One.Net.BLL;
using One.Net.BLL.Web;
using One.Net.Forms.BLL;

using System.IO;
using System.Text;
using One.Net.BLL.WebControls;
using One.Net.BLL.Utility;
using NLog;

namespace One.Net.Forms
{
    /// <summary>
    /// ****** IMPORTANT NOTE!!! ******
    /// What is INamingContainer?
    /// INamingContainer is a marker interface, meaning it has no methods to implement. 
    /// A control "implements" this interface to let the framework know that it plans on giving it's child controls really specific IDs. 
    /// It's important to the framework, because if two instances of the same control are on the same page, 
    /// and the control gives its child controls some specific ID, there'd end up being multiple controls with the same ID on the page, 
    /// which is going to cause problems. 
    /// So when a Control is a naming container, the UniqueID for all controls within it will have the parent's ID as a prefix. 
    /// This scopes it to that control. So while a child control might have ID "foo", its UniqueID will be "parentID$foo" (where parentID = the ID of the parent). 
    /// Now even if this control exists twice on the page, everyone will still have a UniqueID.
    /// INamingContainer also has the property that any controls within it that do not have a specific ID will have its ID automatically determined based on a counter that is scoped to it. 
    /// So if there were two naming containers, foo and bar, they might have child controls with UniqueIDs foo$ctl01 and bar$ctl01. 
    /// Each naming container gets its own little counter.
    /// ****** END IMPORTANT NOTE ******
    /// </summary>
    public partial class Form : MModule, INamingContainer
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();

        private static readonly Random random = new Random();
        protected static BForm formB = new BForm();

        #region Settings

        protected int FormId { get { return GetIntegerSetting("FormId"); } }
        protected int UploadFolderId { get { return GetIntegerSetting("UploadFolderId"); } }
        protected bool UseLabels { get { return GetBooleanSetting("UseLabels"); } }

        #endregion Settings

        #region Properties

        protected BOForm SessionForm
        {
            get
            {
                if (Session["SessionForm" + FormId + InstanceId] != null && Session["SessionForm" + FormId + InstanceId] is BOForm)
                {
                    return (BOForm)Session["SessionForm" + FormId + InstanceId];
                }

                return null;
            }
            set
            {
                Session["SessionForm" + FormId + InstanceId] = value;
            }
        }

        protected OFormSubmission FormSubmission
        {
            get
            {
                if (Session["FormSubmission" + FormId + InstanceId] != null && Session["FormSubmission" + FormId + InstanceId] is OFormSubmission)
                    return (OFormSubmission)Session["FormSubmission" + FormId + InstanceId];

                return null;
            }
            set
            {
                Session["FormSubmission" + FormId + InstanceId] = value;
            }
        }

        #endregion Properties

        protected void Page_Load(object sender, EventArgs e)
        {
            // http://stackoverflow.com/questions/49547/making-sure-a-web-page-is-not-cached-across-all-browsers/2068407#2068407
            Response.AppendHeader("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
            Response.AppendHeader("Pragma", "no-cache"); // HTTP 1.0.
            Response.AppendHeader("Expires", "0"); // Proxies.

            Type cstype = this.GetType();

            if (!IsPostBack)
            {
                // these values are used to determine validity of POST ( to ignore F5 refresh )
                Session["PostID"] = "1001";
                ViewState["PostID"] = Session["PostID"].ToString();

                cmdSubmit.Text = Translate(cmdSubmit.Text);
                cmdPrev.Text = Translate(cmdPrev.Text);
                cmdNext.Text = Translate(cmdNext.Text);
            }
        }

        /// <summary>
        /// ****** IMPORTANT NOTE!!! ******
        /// Because we are adding controls dynamically, building the tree is our responsibility. 
        /// The controls of the tree must be completely reconstructed upon each and every request to the page.
        /// ViewState is responsible for maintaining the state of the controls in the control tree 
        /// and NOT the state of the control tree itself! 
        /// That responsibility lies wholly on our shoulders. 
        /// Dynamically created controls will not exist on the next postback unless we add them to the control tree once again. 
        /// Once a control is a member of the tree, the framework takes over to maintain the control's ViewState.
        /// ****** END IMPORTANT NOTE ******
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            CheckFormData();
            CreateFormControls(false, false);
        }

        /// <summary>
        /// At PreRender, we can make any changes without them being affected by the framework
        /// This is where we load previous data ( for example when prev button is clicked by user )
        /// And this is where we create captcha data, so that we can check it on next postback 
        /// without the framework overwriting it.
        /// </summary>
        /// <param name="e"></param>
        protected override void  OnPreRender(EventArgs e)
        {
            ModifyFormControls();

 	        base.OnPreRender(e);
        }

        protected void rptPollResults_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var question = e.Item.DataItem as BOSubmittedQuestion;
                var rptPollSubmittedAnswers = e.Item.FindControl("rptPollSubmittedAnswers") as Repeater;

                if (question != null && rptPollSubmittedAnswers != null)
                {
                    rptPollSubmittedAnswers.DataSource = question.SubmittedAnswers.Values;
                    rptPollSubmittedAnswers.DataBind();
                }
            }
        }

        protected void rptPollSubmittedAnswers_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                BOSubmittedAnswer submittedAnswer = e.Item.DataItem as BOSubmittedAnswer;
                ProgressBar progressBar = e.Item.FindControl("progressBar") as ProgressBar;
                Label LabelPercentage = e.Item.FindControl("LabelPercentage") as Label;

                progressBar.Percentage = (int)submittedAnswer.Answer.PercentageAnswered;
                progressBar.Visible = true;
                progressBar.OuterCssClass = "barTotal";
                progressBar.InnerCssClass = "barPart";
                LabelPercentage.Text = string.Format("{0:#0.00'%}", submittedAnswer.Answer.PercentageAnswered);
            }
        }

        private void CheckFormData()
        {
            if (FormId > -1)
            {
                // Load Form data if non existant.
                if (SessionForm == null)
                {
                    SessionForm = formB.Get(FormId);

                    if (SessionForm != null)
                    {
                        // Initialize Submission object if submission object is null and form object has at least one section
                        if (FormSubmission == null)
                        {
                            if (SessionForm.FirstSectionKey.HasValue)
                            {
                                FormSubmission = new OFormSubmission();
                                FormSubmission.FormId = SessionForm.Id.Value;

                                if (SessionForm.Sections[SessionForm.FirstSectionKey.Value].SectionType == SectionTypes.MultiPage)
                                {
                                    FormSubmission.CurrentSectionId = SessionForm.FirstSectionKey.Value;
                                    FormSubmission.PreviousSectionId = BOForm.GetPrevSectionId(SessionForm, FormSubmission.CurrentSectionId.Value);
                                    FormSubmission.NextSectionId = BOForm.GetNextSectionId(SessionForm, FormSubmission.CurrentSectionId.Value);
                                }
                                else
                                {
                                    FormSubmission.CurrentSectionId = SessionForm.FirstSectionKey.Value;
                                    FormSubmission.PreviousSectionId = null;
                                    FormSubmission.NextSectionId = null;
                                }
                            }
                            else
                            {
                                log.Error("Form with formId=" + FormId + " has 0 defined sections");
                                throw new Exception("Form with formId=" + FormId + " has 0 defined sections");
                            }
                        }
                    }
                    else
                    {
                        log.Error("Form with FormId=" + FormId + " does not exist!");
                        throw new Exception("Form with FormId=" + FormId + " does not exist!");
                    }
                }
            }
            else
            {
                log.Error("FormId is not set for module instance with id=" + InstanceId);
                throw new Exception("FormId is not set for module instance with id=" + InstanceId);
            }
        }

        private void CreateFormControls(bool submissionJustCompleted, bool submissionOk)
        {
            if (FormId > -1)
            {
                // Hide both place holders
                plhForm.Visible = plhResults.Visible = false;

                // Hide all buttons
                cmdPrev.Visible = cmdNext.Visible = cmdSubmit.Visible = false;

                // Set validation group
                cmdPrev.ValidationGroup = cmdNext.ValidationGroup = cmdSubmit.ValidationGroup = "FormID" + FormId + InstanceId;

                if (FormSubmission.CurrentSectionId.HasValue)
                {
                    plhForm.Visible = true;

                    List<BOSection> sections = new List<BOSection>();
                    if (SessionForm.Sections[SessionForm.FirstSectionKey.Value].SectionType == SectionTypes.MultiPage)
                    {
                        // if multi page then load only one section
                        sections.Add(SessionForm.Sections[FormSubmission.CurrentSectionId.Value]);
                    }
                    else
                    {
                        // if single page form, then load all sections
                        foreach (BOSection sec in SessionForm.Sections.Values)
                        {
                            sections.Add(sec);
                        }
                    }

                    if (!string.IsNullOrEmpty(SessionForm.Title))
                    {
                        HtmlGenericControl formTitleDiv = new HtmlGenericControl("div");
                        formTitleDiv.Attributes.Add("class", "formTitle");
                        formTitleDiv.InnerText = SessionForm.Title;
                        plhForm.Controls.Add(formTitleDiv);
                    }

                    if (!string.IsNullOrEmpty(SessionForm.Description) && 
                        !FormSubmission.PreviousSectionId.HasValue )
                    {
                        HtmlGenericControl DivDescription = new HtmlGenericControl("div");
                        DivDescription.Attributes.Add("class", "Description");
                        DivDescription.InnerHtml = SessionForm.Description;
                        plhForm.Controls.Add(DivDescription);
                    }

                    CreateSectionControls(sections);

                    // hide show form buttons
                    PanelCommands.Visible = true;
                    cmdPrev.Visible = FormSubmission.PreviousSectionId.HasValue && SessionForm.AllowModifyInSubmission;
                    cmdNext.Visible = FormSubmission.NextSectionId.HasValue;
                    if (FormSubmission.NextSectionId.HasValue)
                    {
                        if (SessionForm.Sections.ContainsKey(FormSubmission.CurrentSectionId.Value) &&
                            !string.IsNullOrEmpty(
                                SessionForm.Sections[FormSubmission.CurrentSectionId.Value].OnClientClick))
                        {
                            cmdNext.OnClientClick =
                                SessionForm.Sections[FormSubmission.CurrentSectionId.Value].OnClientClick;
                        }
                    }
                    else
                    {
                        if (SessionForm.Sections.ContainsKey(FormSubmission.CurrentSectionId.Value) &&
                            !string.IsNullOrEmpty(
                                SessionForm.Sections[FormSubmission.CurrentSectionId.Value].OnClientClick))
                        {
                            cmdSubmit.OnClientClick =
                                SessionForm.Sections[FormSubmission.CurrentSectionId.Value].OnClientClick;
                        }
                    }
                    cmdSubmit.Visible = !cmdNext.Visible;
                }
                else
                {
                    PanelCommands.Visible = false;

                    // show results place holder
                    plhResults.Visible = true;

                    DivFormTitle.Visible = true;

                    // hide both inner placeholders
                    divQuestionaireThankYouNote.Visible = rptPollResults.Visible = false;

                    DivFormTitle.InnerHtml = SessionForm.Title;

                    switch (SessionForm.FormType)
                    {
                        case FormTypes.Questionaire:
                            {
                                divQuestionaireThankYouNote.Visible = true;
                                if (submissionJustCompleted && !submissionOk)
                                {
                                    lblThankYouNote.Text = Translate("form_submission_failed");
                                }
                                else
                                {
                                    // submission completed now or later ( in case of multiple submissions not being allowed )
                                    lblThankYouNote.Text = SessionForm.ThankYouNote;
                                }
                                break;
                            }
                        case FormTypes.Poll:
                            {
                                Dictionary<int, BOSubmittedQuestion> submittedQuestions = new Dictionary<int, BOSubmittedQuestion>();

                                // retrieve data from db so as to pull out the latest percentages
                                SessionForm = formB.Get(FormId);

                                foreach (BOSection section in SessionForm.Sections.Values)
                                {
                                    foreach (BOQuestion question in section.Questions.Values)
                                    {
                                        BOSubmittedQuestion submittedQuestion = new BOSubmittedQuestion();
                                        submittedQuestion.Question = question;

                                        foreach (BOAnswer answer in question.Answers.Values)
                                        {
                                            BOSubmittedAnswer submittedAnswer = new BOSubmittedAnswer();
                                            submittedAnswer.Answer = answer;
                                            submittedQuestion.SubmittedAnswers.Add(answer.Id.Value, submittedAnswer);
                                        }
                                        submittedQuestions.Add(question.Id.Value, submittedQuestion);
                                    }
                                }

                                rptPollResults.Visible = true;
                                rptPollResults.DataSource = submittedQuestions.Values;
                                rptPollResults.DataBind();

                            } break;
                    }

                }
            }
            else
            {
                log.Error("FormId is not set for module instance with id=" + InstanceId);
                throw new Exception("FormId is not set for module instance with id=" + InstanceId);
            }
        }

        private void ModifyFormControls()
        {
            if (FormSubmission != null)
            {
                if (FormSubmission.CurrentSectionId.HasValue && FormSubmission.CurrentSectionId.HasValue)
                {
                    plhForm.Visible = true;

                    List<BOSection> sections = new List<BOSection>();
                    if (SessionForm.Sections[SessionForm.FirstSectionKey.Value].SectionType == SectionTypes.MultiPage)
                    {
                        // if multi page then load only one section
                        sections.Add(SessionForm.Sections[FormSubmission.CurrentSectionId.Value]);
                    }
                    else
                    {
                        // if single page form, then load all sections
                        foreach (BOSection sec in SessionForm.Sections.Values)
                        {
                            sections.Add(sec);
                        }
                    }

                    foreach (BOSection section in sections)
                    {
                        HtmlGenericControl sectionDiv = plhForm.FindControl("SectionDiv" + section.Id) as HtmlGenericControl;

                        if (sectionDiv != null)
                        {
                            foreach (BOQuestion question in section.Questions.Values)
                            {
                                HtmlGenericControl questionDiv = sectionDiv.FindControl("QuestionDiv" + question.Id) as HtmlGenericControl;

                                if (questionDiv != null && question.FirstAnswerKey.HasValue)
                                {
                                    BOAnswer firstAnswer = question.Answers[question.FirstAnswerKey.Value];


                                    switch (firstAnswer.AnswerType)
                                    {
                                        case AnswerTypes.Checkbox: { PreselectCheckOrRadioListAnswers<CheckBox>(questionDiv, question); break; }
                                        case AnswerTypes.DropDown:
                                            {
                                                HtmlGenericControl holderDiv = questionDiv.FindControl("DropDownListHolder" + question.Id) as HtmlGenericControl;

                                                if (holderDiv != null)
                                                {
                                                    var dropDownList = holderDiv.FindControl("DropDownList" + question.Id) as DropDownList;

                                                    if (dropDownList != null)
                                                    {
                                                        // In case user is making changes... 
                                                        // check whether they have already answered this question
                                                        // and select it from the list.
                                                        foreach (BOAnswer answer in question.Answers.Values)
                                                        {
                                                            BOSubmittedAnswer submittedAnswer = null;
                                                            if (FormSubmission.SubmittedQuestions.ContainsKey(question.Id.Value) &&
                                                                FormSubmission.SubmittedQuestions[question.Id.Value] != null &&
                                                                FormSubmission.SubmittedQuestions[question.Id.Value].SubmittedAnswers.ContainsKey(answer.Id.Value) &&
                                                                FormSubmission.SubmittedQuestions[question.Id.Value].SubmittedAnswers[answer.Id.Value] != null)
                                                            {
                                                                submittedAnswer = FormSubmission.SubmittedQuestions[question.Id.Value].SubmittedAnswers[answer.Id.Value];

                                                                // if user has already answered this question... select their answer
                                                                if (submittedAnswer != null && dropDownList.Items.Contains(new ListItem(answer.Title, answer.Id.Value.ToString())))
                                                                {
                                                                    dropDownList.SelectedValue = answer.Id.Value.ToString();
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                break;
                                            }
                                        case AnswerTypes.Radio: { PreselectCheckOrRadioListAnswers<RadioButton>(questionDiv, question); break; }
                                        case AnswerTypes.SingleFile:
                                            {
                                                FileUpload fileInput = questionDiv.FindControl("AnswerFileUpload" + firstAnswer.Id) as FileUpload;

                                                if (fileInput != null)
                                                {

                                                }

                                                break;
                                            }
                                        case AnswerTypes.SingleText:
                                            {
                                                var answerInput = questionDiv.FindControl("AnswerSingleText" + firstAnswer.Id) as TextBox;

                                                if (answerInput != null)
                                                {
                                                    // In case user is making changes... check whether they have already answered this question
                                                    if (FormSubmission.SubmittedQuestions.ContainsKey(firstAnswer.ParentId.Value) &&
                                                        FormSubmission.SubmittedQuestions[firstAnswer.ParentId.Value] != null &&
                                                        FormSubmission.SubmittedQuestions[firstAnswer.ParentId.Value].SubmittedAnswers.ContainsKey(firstAnswer.Id.Value) &&
                                                        FormSubmission.SubmittedQuestions[firstAnswer.ParentId.Value].SubmittedAnswers[firstAnswer.Id.Value] != null &&
                                                        FormSubmission.SubmittedQuestions[firstAnswer.ParentId.Value].SubmittedAnswers[firstAnswer.Id.Value].SubmittedText != null)
                                                    {
                                                        answerInput.Text = FormSubmission.SubmittedQuestions[firstAnswer.ParentId.Value].SubmittedAnswers[firstAnswer.Id.Value].SubmittedText;
                                                    }

                                                    if (question.ValidationType == ValidationTypes.Captcha)
                                                    {
                                                        // create captcha field here... so that it gets to the click event without being modified when
                                                        // tree is recreated, and show it in image while also passing the encrypted value in hidden label
                                                        HtmlGenericControl captchaDiv = questionDiv.FindControl("CaptchaDiv" + firstAnswer.Id) as HtmlGenericControl;
                                                        if (captchaDiv != null)
                                                        {
                                                            System.Web.UI.WebControls.Image captchaImage = captchaDiv.FindControl("CaptchaImage" + firstAnswer.Id) as System.Web.UI.WebControls.Image;
                                                            Label captchaLabel = questionDiv.FindControl("CaptchaLabel" + firstAnswer.Id) as Label;
                                                            if (captchaLabel != null && captchaImage != null)
                                                            {
                                                                string code = Encrypt(GenerateRandomCode(), DateTime.Now.AddMinutes(5));
                                                                captchaImage.ImageUrl = "~/Utils/JpegImage.aspx?code=" + code;
                                                                captchaLabel.Text = code;
                                                            }
                                                        }
                                                    }
                                                }

                                                break;
                                            }
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }

        private void CreateSectionControls(IEnumerable<BOSection> sections)
        {
            foreach (BOSection section in sections)
            {
                HtmlGenericControl sectionDiv = new HtmlGenericControl("div");
                sectionDiv.ID = "SectionDiv" + section.Id;
                sectionDiv.Attributes.Add("class", "section");

                if (!string.IsNullOrEmpty(section.Title))
                {
                    HtmlGenericControl sectionTitleP = new HtmlGenericControl("p");
                    sectionTitleP.Attributes.Add("class", "sectionTitle");
                    sectionTitleP.InnerText = section.Title;
                    sectionDiv.Controls.Add(sectionTitleP);
                }

                if (!string.IsNullOrEmpty(section.Description))
                {
                    HtmlGenericControl DivDescription = new HtmlGenericControl("div");
                    DivDescription.Attributes.Add("class", "Description");
                    DivDescription.InnerHtml = section.Description;
                    sectionDiv.Controls.Add(DivDescription);
                }

                foreach (BOQuestion question in section.Questions.Values)
                {
                    HtmlGenericControl questionDiv = new HtmlGenericControl("div");
                    questionDiv.ID = "QuestionDiv" + question.Id;
                    questionDiv.Attributes.Add("class", question.IsAnswerRequired ? "question isRequired form-group " : "question form-group");

                    if (!string.IsNullOrEmpty(question.Description))
                    {
                        HtmlGenericControl questionTeaserDiv = new HtmlGenericControl("div");
                        questionTeaserDiv.Attributes.Add("class", "description");
                        questionTeaserDiv.InnerHtml = question.Description;
                        questionDiv.Controls.Add(questionTeaserDiv);
                    }

                    if (question.FirstAnswerKey.HasValue)
                    {
                        BOAnswer firstAnswer = question.Answers[question.FirstAnswerKey.Value];
                        switch (firstAnswer.AnswerType)
                        {
                            case AnswerTypes.Checkbox: { AddCheckListToRepeater(questionDiv, question, "check"); break; }
                            case AnswerTypes.DropDown:
                                {
                                    Label lblDropDownList = new Label();
                                    lblDropDownList.Text = question.Title;

                                    var dropDownList = new DropDownList();
                                    dropDownList.DataSource = question.Answers.Values;
                                    dropDownList.DataTextField = "Title";
                                    dropDownList.DataValueField = "Id";
                                    dropDownList.DataBind();
                                    dropDownList.ID = "DropDownList" + question.Id;

                                    HtmlGenericControl holderDiv = new HtmlGenericControl("div");
                                    holderDiv.ID = "DropDownListHolder" + question.Id;
                                    holderDiv.Attributes.Add("class", "select");

                                    lblDropDownList.AssociatedControlID = dropDownList.ID;

                                    holderDiv.Controls.Add(lblDropDownList);
                                    holderDiv.Controls.Add(dropDownList);
                                    questionDiv.Controls.Add(holderDiv);

                                    break;
                                }
                            case AnswerTypes.Radio: { AddRadioListToRepeater(questionDiv, question, "radio"); break; }
                            case AnswerTypes.SingleFile:
                                {
									var holderDiv = new HtmlGenericControl("div");
									holderDiv.ID = "FileInputHolder" + question.Id;
									holderDiv.Attributes.Add("class", "file");

                                    var input = new FileUpload { ID = "AnswerFileUpload" + firstAnswer.Id };
									var fileLabel = new Label { Text = question.Title, AssociatedControlID = input.ID };

									holderDiv.Controls.Add(fileLabel);
									holderDiv.Controls.Add(input);
									questionDiv.Controls.Add(holderDiv);

                                    break;
                                }
                            case AnswerTypes.SingleText:
                                {
                                    var answerInput = new TextBox();
                                    answerInput.ID = "AnswerSingleText" + firstAnswer.Id;
                                    answerInput.ValidationGroup = "FormID" + FormId + InstanceId;

                                    if (UseLabels)
                                    {
                                        // if using labels instead of placeholders.
                                        var answerInputLabel = new Label();
                                        answerInputLabel.Text = question.Title;
                                        answerInputLabel.AssociatedControlID = answerInput.ID;
                                        questionDiv.Controls.Add(answerInput);
                                    }
                                    else
                                    {
                                        answerInput.Attributes.Add("placeholder", question.Title);
                                    }

                                    if (question.IsAnswerRequired)
                                    {
                                        answerInput.CssClass = "required ";
                                    }

                                    switch (question.ValidationType)
                                    {
                                        case ValidationTypes.Time: 
                                            answerInput.CssClass += "time";
                                            break;
                                        case ValidationTypes.DateTime: 
                                            answerInput.CssClass += "date";
                                            break;
                                        case ValidationTypes.Email: 
                                            answerInput.CssClass += "email";
                                            break;
                                        case ValidationTypes.Integer:
                                            answerInput.CssClass += "digits";
                                            break;
                                        case ValidationTypes.Numeric:
                                            answerInput.CssClass += "number";
                                            break;
                                        case ValidationTypes.Captcha:
                                            Label lblCaptcha = new Label();
                                            lblCaptcha.ID = "CaptchaLabel" + firstAnswer.Id;
                                            lblCaptcha.Visible = false;
                                            questionDiv.Controls.Add(lblCaptcha);

                                            // Captcha image is created here, while it is loaded with data in PreRender
                                            System.Web.UI.WebControls.Image captchaImage = new System.Web.UI.WebControls.Image();
                                            captchaImage.ID = "CaptchaImage" + firstAnswer.Id;

                                            HtmlGenericControl captchaDiv = new HtmlGenericControl("div");
                                            captchaDiv.ID = "CaptchaDiv" + firstAnswer.Id;
                                            captchaDiv.Attributes.Add("class", "captcha");
                                            captchaDiv.Controls.Add(captchaImage);
                                            questionDiv.Controls.Add(captchaDiv);
                                            break;
                                    }


                                    if (firstAnswer.MaxChars.HasValue && firstAnswer.MaxChars > 0)
                                    {
                                        answerInput.MaxLength = firstAnswer.MaxChars.Value;
                                    }

                                    if (firstAnswer.NumberOfRows.HasValue && firstAnswer.NumberOfRows > 1)
                                    {
                                        answerInput.TextMode = TextBoxMode.MultiLine;
                                        answerInput.Rows = firstAnswer.NumberOfRows.Value;
                                    }

                                    questionDiv.Controls.Add(answerInput);

                                    break;
                                }
                        }

                        HtmlGenericControl validationDiv = new HtmlGenericControl("div");
                        validationDiv.InnerText = "";
                        validationDiv.Visible = false;
                        validationDiv.Attributes.Add("class", "validationResult");

                        if (FormSubmission.InvalidQuestions.ContainsKey(question.Id.Value))
                        {
                            validationDiv.Visible = true;

                            CustomValidator customValidator = new CustomValidator();
                            customValidator.ValidationGroup = "FormID" + FormId + InstanceId;
                            customValidator.IsValid = false;

                            switch (FormSubmission.InvalidQuestions[question.Id.Value])
                            {
                                case OFormSubmission.UserErrorTypes.FileErrorMimeTypeNotAllowed: customValidator.ErrorMessage = Translate("invalid_file_type"); break;
                                case OFormSubmission.UserErrorTypes.FileErrorOther: customValidator.ErrorMessage = Translate("file_required"); break;
                                case OFormSubmission.UserErrorTypes.FileErrorSizeExceeded: customValidator.ErrorMessage = Translate("file_too_big"); break;
                                case OFormSubmission.UserErrorTypes.DropDownErrorFakeAnswerSelected: customValidator.ErrorMessage = Translate("required"); break;
                                case OFormSubmission.UserErrorTypes.MenuErrorAnswerRequired: customValidator.ErrorMessage = Translate("required"); break;
                                case OFormSubmission.UserErrorTypes.CaptchaErrorMismatch: customValidator.ErrorMessage = Translate("captcha_did_not_match"); break;
                            }

                            validationDiv.Controls.Add(customValidator);
                        }

                        questionDiv.Controls.Add(validationDiv);
                    }
                    sectionDiv.Controls.Add(questionDiv);
                }
                plhForm.Controls.Add(sectionDiv);
            }
        }

        #region Event Handlers

        public bool IsValidPost()
        {
            try
            {
                if (ViewState["PostID"] != null && Session["PostID"] != null && ViewState["PostID"].ToString() == Session["PostID"].ToString())
                {
                    Session["PostID"] = (Convert.ToInt16(Session["PostID"]) + 1).ToString();
                    ViewState["PostID"] = Session["PostID"].ToString();

                    return true;
                }
                else
                {
                    ViewState["PostID"] = Session["PostID"].ToString();
                    return false;
                }
            }
            catch
            {
                return false;
            }   
        }

        protected void cmdNext_Click(object sender, EventArgs e)
        {
            if (IsValidPost())
            {
                if (SessionForm != null && FormSubmission != null)
                {
                    bool valid = RetreiveSubmittedAnswers();

                    if (valid)
                    {
                        // NB. This only applies to MultiPage sections... since for SinglePage sections the next button is replaced with submit button
                        FormSubmission.PreviousSectionId = FormSubmission.CurrentSectionId.Value;
                        FormSubmission.CurrentSectionId =
                            BOForm.GetNextSectionId(SessionForm, FormSubmission.CurrentSectionId.Value);
                        FormSubmission.NextSectionId =
                            BOForm.GetNextSectionId(SessionForm, FormSubmission.CurrentSectionId.Value);
                    }

                    plhForm.Controls.Clear();
                    CreateFormControls(false, false);
                }
            }
        }

        protected void cmdPrev_Click(object sender, EventArgs e)
        {
            if (IsValidPost())
            {
                if (SessionForm != null && FormSubmission != null)
                {
                    FormSubmission.NextSectionId = FormSubmission.CurrentSectionId;
                    FormSubmission.CurrentSectionId = FormSubmission.PreviousSectionId;
                    FormSubmission.PreviousSectionId =
                        BOForm.GetPrevSectionId(SessionForm, FormSubmission.CurrentSectionId.Value);

                    plhForm.Controls.Clear();
                    CreateFormControls(false, false);
                }
            }
        }

        protected void cmdSubmit_Click(object sender, EventArgs e)
        {
            if (IsValidPost())
            {
                if (SessionForm != null && FormSubmission != null)
                {
                    bool valid = RetreiveSubmittedAnswers();
                    bool submissionOk = false;

                    if (valid && FormSubmission.CurrentSectionId.HasValue)
                    {
                        FormSubmission.PreviousSectionId = FormSubmission.CurrentSectionId.Value;
                        FormSubmission.CurrentSectionId = null;
                        FormSubmission.NextSectionId = null;

                        BOFormSubmission submission = FormSubmission;
                        submission.Finished = DateTime.Now;
                        submissionOk = formB.ProcessFormSubmission(submission, SessionForm);
                    }

                    plhForm.Controls.Clear();
                    CreateFormControls(true, submissionOk);

                    if (!string.IsNullOrEmpty(SessionForm.CompletionRedirect) &&
                        (SessionForm.CompletionRedirect.StartsWith("http") ||
                         SessionForm.CompletionRedirect.StartsWith("/")))
                    {
                        UrlBuilder builder = null;
                        if (SessionForm.CompletionRedirect.StartsWith("http"))
                        {
                            builder = new UrlBuilder(SessionForm.CompletionRedirect);
                        }
                        else
                        {
                            builder = new UrlBuilder(Page);
                            builder.Path = SessionForm.CompletionRedirect;
                        }

                        // once you show the results... clear the form and the submission
                        // so if user clicks back they cannot resubmit
                        if (SessionForm.AllowMultipleSubmissions && submissionOk)
                        {
                            FormSubmission = null;
                            SessionForm = null;
                        }

                        Response.Redirect(builder.ToString(), true);
                    }
                    else
                    {
                        // once you show the results... clear the form and the submission
                        // so if user clicks back they cannot resubmit
                        if (SessionForm.AllowMultipleSubmissions && submissionOk)
                        {
                            FormSubmission = null;
                            SessionForm = null;
                        }
                    }
                }
            }
        }

        #endregion Event Handlers

        #region Helper Methods

        private static void AddRadioListToRepeater(HtmlGenericControl div, BOQuestion question, string outerDivCssClass)
        {
            HtmlGenericControl radioListDiv = new HtmlGenericControl("div");
            radioListDiv.ID = "ListDiv" + question.Id;
            radioListDiv.Attributes.Add("class", outerDivCssClass);

            HtmlGenericControl questionTitleSpan = new HtmlGenericControl("span");
            questionTitleSpan.Attributes.Add("class", "radioLabel");
            questionTitleSpan.InnerText = question.Title;
            radioListDiv.Controls.Add(questionTitleSpan);

            foreach (BOAnswer answer in question.Answers.Values)
            {
                HtmlGenericControl radioHolder = new HtmlGenericControl("div");
                radioHolder.Attributes.Add("class", "Holder");

                RadioButton button = new RadioButton();
                button.Text = answer.Title;
                button.ID = "AnswerButton" + answer.Id.Value;
                button.GroupName = radioListDiv.ID + "QuestionGroup" + question.Id;
                radioHolder.Controls.Add(button);

                if (answer.AdditionalFieldType == AdditionalFieldTypes.Text)
                {
                    TextBox textInput = new TextBox();
                    textInput.ID = "AnswerButtonTextInput" + answer.Id.Value;
                    radioHolder.Controls.Add(textInput);
                }
                else if (answer.AdditionalFieldType == AdditionalFieldTypes.File)
                {
                    FileUpload fileInput = new FileUpload();
                    fileInput.ID = "AnswerButtonFileUpload" + answer.Id.Value;
                    radioHolder.Controls.Add(fileInput);
                }

                radioListDiv.Controls.Add(radioHolder);
            }

            div.Controls.Add(radioListDiv);
        }

        private static void AddCheckListToRepeater(HtmlGenericControl div, BOQuestion question, string outerDivCssClass)
        {
            HtmlGenericControl checkListDiv = new HtmlGenericControl("div");
            checkListDiv.ID = "ListDiv" + question.Id;
            checkListDiv.Attributes.Add("class", outerDivCssClass);

            HtmlGenericControl questionTitleSpan = new HtmlGenericControl("span");
            questionTitleSpan.Attributes.Add("class", "checkListLabel");
            questionTitleSpan.InnerText = question.Title;
            checkListDiv.Controls.Add(questionTitleSpan);

            
            

            foreach (BOAnswer answer in question.Answers.Values)
            {
                HtmlGenericControl checkboxHolder = new HtmlGenericControl("div");
                checkboxHolder.Attributes.Add("class", "Holder");

                CheckBox button = new CheckBox();
                button.Text = answer.Title;
                button.ID = "AnswerButton" + answer.Id.Value;
                checkboxHolder.Controls.Add(button);

                if (answer.AdditionalFieldType == AdditionalFieldTypes.Text)
                {
                    TextBox textInput = new TextBox();
                    textInput.ID = "AnswerButtonTextInput" + answer.Id.Value;
                    checkboxHolder.Controls.Add(textInput);
                }
                else if (answer.AdditionalFieldType == AdditionalFieldTypes.File)
                {
                    FileUpload fileUpload = new FileUpload();
                    fileUpload.ID = "AnswerButtonFileUpload" + answer.Id.Value;
                    checkboxHolder.Controls.Add(fileUpload);
                }

                checkListDiv.Controls.Add(checkboxHolder);
            }
            

            div.Controls.Add(checkListDiv);
        }

        private bool RetreiveSubmittedAnswers()
        {
            int invalidQuestionsCount = 0;

            List<BOSection> sections = new List<BOSection>();
            if (SessionForm.Sections[SessionForm.FirstSectionKey.Value].SectionType == SectionTypes.MultiPage)
            {
                // if multi page then retrieve input from only current section
                sections.Add(SessionForm.Sections[FormSubmission.CurrentSectionId.Value]);
            }
            else
            {
                // retrieve data from all sections
                foreach (BOSection sec in SessionForm.Sections.Values)
                {
                    sections.Add(sec);
                }
            }

            // retrieve user input
            foreach (BOSection section in sections)
            {
                HtmlGenericControl sectionDiv = plhForm.FindControl("SectionDiv" + section.Id) as HtmlGenericControl;

                if (sectionDiv != null)
                {
                    foreach (BOQuestion question in section.Questions.Values)
                    {
                        HtmlGenericControl questionDiv = sectionDiv.FindControl("QuestionDiv" + question.Id) as HtmlGenericControl;

                        if (questionDiv != null)
                        {
                            BOAnswer firstAnswer = question.Answers[question.FirstAnswerKey.Value];

                            // temporary storage for submitted answers
                            BOSubmittedQuestion submittedQuestion = new BOSubmittedQuestion();
                            submittedQuestion.Question = question;
                            
                            switch (firstAnswer.AnswerType)
                            {
                                case AnswerTypes.Checkbox: { submittedQuestion.SubmittedAnswers = RetrieveAnswersFromRadioOrCheckList<CheckBox>(questionDiv, question); break; }
                                case AnswerTypes.DropDown: 
                                {
                                    HtmlGenericControl holderDiv = questionDiv.FindControl("DropDownListHolder" + question.Id) as HtmlGenericControl;

                                    if (holderDiv != null)
                                    {
                                        var dropDownList = holderDiv.FindControl("DropDownList" + question.Id) as DropDownList;

                                        if (dropDownList != null)
                                        {
                                            int answerId = FormatTool.GetInteger(dropDownList.SelectedValue);
                                            if (answerId > -1 && question.Answers[answerId] != null)
                                            {
                                                BOSubmittedAnswer submittedAnswer = new BOSubmittedAnswer();
                                                submittedAnswer.Answer = question.Answers[answerId];
                                                submittedAnswer.SubmissionId = null;
                                                submittedAnswer.SubmittedFile = null;
                                                submittedAnswer.SubmittedText = null;

                                                submittedQuestion.SubmittedAnswers.Add(answerId, submittedAnswer);
                                            }
                                        }
                                    }

                                    break;
                                }
                                case AnswerTypes.Radio: { submittedQuestion.SubmittedAnswers = RetrieveAnswersFromRadioOrCheckList<RadioButton>(questionDiv, question); break; }
                                case AnswerTypes.SingleFile: 
                                {
                                    FileUpload fileInput = questionDiv.FindControl("AnswerFileUpload" + firstAnswer.Id) as FileUpload;

                                    if (fileInput != null)
                                    {
                                        BOSubmittedAnswer submittedAnswer = new BOSubmittedAnswer();
                                        submittedAnswer.Answer = firstAnswer;
                                        submittedAnswer.SubmissionId = null;
                                        submittedAnswer.SubmittedFile = RetrieveSubmittedFile(fileInput);
                                        submittedAnswer.SubmittedText = null;
                                        submittedQuestion.SubmittedAnswers.Add(firstAnswer.Id.Value, submittedAnswer);
                                    }

                                    break; 
                                }
                                case AnswerTypes.SingleText:
                                    {
                                        var answerInput = questionDiv.FindControl("AnswerSingleText" + firstAnswer.Id) as TextBox;

                                        if (answerInput != null)
                                        {
                                            BOSubmittedAnswer submittedAnswer = new BOSubmittedAnswer();
                                            submittedAnswer.Answer = firstAnswer;
                                            submittedAnswer.SubmissionId = null;
                                            submittedAnswer.SubmittedFile = null;
                                            submittedAnswer.SubmittedText = answerInput.Text;
                                            submittedQuestion.SubmittedAnswers.Add(firstAnswer.Id.Value, submittedAnswer);
                                        }

                                        break;
                                    }
                            }

                            // For case where user is making changes... first clear this question as invalid, then check whether its invalid
                            if (FormSubmission.InvalidQuestions.ContainsKey(question.Id.Value))
                            {
                                FormSubmission.InvalidQuestions.Remove(question.Id.Value);
                            }

                            // For case where user is making changes... first clear all answers that were previously submitted for this question
                            // then store these new answers
                            if (FormSubmission.SubmittedQuestions.ContainsKey(question.Id.Value))
                            {
                                FormSubmission.SubmittedQuestions.Remove(question.Id.Value);
                            }

                            bool valid = true;

                            // Now store invalid question ( once which could not be checked with validators )

                            if (firstAnswer.AnswerType == AnswerTypes.SingleText)
                            {
                                if (question.ValidationType == ValidationTypes.Captcha)
                                {
                                    // create captcha field here... so that it gets to the click event without being modified when
                                    // tree is recreated, and show it in image while also passing the encrypted value in hidden label
                                    HtmlGenericControl captchaDiv = questionDiv.FindControl("CaptchaDiv" + firstAnswer.Id) as HtmlGenericControl;
                                    if (captchaDiv != null)
                                    {
                                        Label captchaLabel = questionDiv.FindControl("CaptchaLabel" + firstAnswer.Id) as Label;
                                        if (captchaLabel != null)
                                        {
                                            string code = Decrypt(captchaLabel.Text);
                                            if (code != submittedQuestion.SubmittedAnswers[firstAnswer.Id.Value].SubmittedText)
                                            {
                                                // captcha did not match
                                                FormSubmission.InvalidQuestions.Add(question.Id.Value, OFormSubmission.UserErrorTypes.CaptchaErrorMismatch);
                                                invalidQuestionsCount++;
                                                valid = false;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (firstAnswer.AnswerType == AnswerTypes.SingleFile)
                            {
                                if (submittedQuestion.SubmittedAnswers[firstAnswer.Id.Value] == null ||
                                    submittedQuestion.SubmittedAnswers[firstAnswer.Id.Value].SubmittedFile == null)
                                {
                                    if (question.IsAnswerRequired)
                                    {
                                        FormSubmission.InvalidQuestions.Add(question.Id.Value, OFormSubmission.UserErrorTypes.FileErrorOther);
                                        invalidQuestionsCount++;
                                        valid = false;
                                    }
                                }
                                else if (submittedQuestion.SubmittedAnswers[firstAnswer.Id.Value].SubmittedFile.Size > firstAnswer.MaxFileSize * 1024)
                                {
                                    FormSubmission.InvalidQuestions.Add(question.Id.Value, OFormSubmission.UserErrorTypes.FileErrorSizeExceeded);
                                    invalidQuestionsCount++;
                                    valid = false;
                                }
                                else if (!firstAnswer.AllowedMimeTypes.Contains(submittedQuestion.SubmittedAnswers[firstAnswer.Id.Value].SubmittedFile.MimeType))
                                {
                                    FormSubmission.InvalidQuestions.Add(question.Id.Value, OFormSubmission.UserErrorTypes.FileErrorMimeTypeNotAllowed);
                                    invalidQuestionsCount++;
                                    valid = false;
                                }
                            }
                            else if (firstAnswer.AnswerType == AnswerTypes.DropDown && firstAnswer.IsFake)
                            {
                                if (submittedQuestion.SubmittedAnswers.ContainsKey(firstAnswer.Id.Value) && question.IsAnswerRequired)
                                {
                                    FormSubmission.InvalidQuestions.Add(question.Id.Value, OFormSubmission.UserErrorTypes.DropDownErrorFakeAnswerSelected);
                                    invalidQuestionsCount++;
                                    valid = false;
                                }
                            }
                            else if ((firstAnswer.AnswerType == AnswerTypes.Checkbox ||
                                      firstAnswer.AnswerType == AnswerTypes.DropDown ||
                                      firstAnswer.AnswerType == AnswerTypes.Radio) && question.IsAnswerRequired)
                            {
                                if (submittedQuestion.SubmittedAnswers.Count == 0)
                                {
                                    FormSubmission.InvalidQuestions.Add(question.Id.Value, OFormSubmission.UserErrorTypes.MenuErrorAnswerRequired);
                                    invalidQuestionsCount++;
                                    valid = false;
                                }
                            }

                            // Now store newly submitted answers in session
                            if (valid)
                            {
                                FormSubmission.SubmittedQuestions.Add(question.Id.Value, submittedQuestion);
                            }
                        }
                    }
                }
            }

            return invalidQuestionsCount == 0;
        }

        public void PreselectCheckOrRadioListAnswers<T>(HtmlGenericControl div, BOQuestion question) where T : CheckBox
        {
            HtmlGenericControl listDiv = div.FindControl("ListDiv" + question.Id) as HtmlGenericControl;
            if (listDiv != null)
            {
                foreach (BOAnswer answer in question.Answers.Values)
                {
                    T button = listDiv.FindControl("AnswerButton" + answer.Id.Value) as T;

                    if (button != null)
                    {
                        // In case user is making changes... 
                        // check whether they have already answered this question
                        // if user has already answered this question... select their answer
                        BOSubmittedAnswer submittedAnswer = null;
                        if (FormSubmission.SubmittedQuestions.ContainsKey(question.Id.Value) &&
                            FormSubmission.SubmittedQuestions[question.Id.Value] != null &&
                            FormSubmission.SubmittedQuestions[question.Id.Value].SubmittedAnswers.ContainsKey(answer.Id.Value) &&
                            FormSubmission.SubmittedQuestions[question.Id.Value].SubmittedAnswers[answer.Id.Value] != null)
                        {
                            submittedAnswer = FormSubmission.SubmittedQuestions[question.Id.Value].SubmittedAnswers[answer.Id.Value];
                            button.Checked = true;
                        }

                        TextBox textInput = listDiv.FindControl("AnswerButtonTextInput" + answer.Id.Value) as TextBox;
                        FileUpload fileUpload = listDiv.FindControl("AnswerButtonFileInput" + answer.Id.Value) as FileUpload;

                        if (textInput != null)
                        {
                            // if user has already answered this question... apply their answer
                            if (submittedAnswer != null && submittedAnswer.SubmittedText != null)
                            {
                                textInput.Text = submittedAnswer.SubmittedText;
                            }
                        }
                        if (fileUpload != null)
                        {

                        }
                    }
                }
            }
        }

        public Dictionary<int, BOSubmittedAnswer> RetrieveAnswersFromRadioOrCheckList<T>(HtmlGenericControl div, BOQuestion question) where T : CheckBox
        {
            Dictionary<int, BOSubmittedAnswer> answers = new Dictionary<int, BOSubmittedAnswer>();

            HtmlGenericControl listDiv = div.FindControl("ListDiv" + question.Id) as HtmlGenericControl;
            if (listDiv != null)
            {
                foreach (BOAnswer answer in question.Answers.Values)
                {
                    T button = listDiv.FindControl("AnswerButton" + answer.Id.Value) as T;

                    if (button != null)
                    {
                        if (button.Checked)
                        {
                            BOSubmittedAnswer submittedAnswer = new BOSubmittedAnswer();
                            submittedAnswer.Answer = answer;

                            TextBox textInput = listDiv.FindControl("AnswerButtonTextInput" + answer.Id.Value) as TextBox;
                            FileUpload fileUpload = listDiv.FindControl("AnswerButtonFileInput" + answer.Id.Value) as FileUpload;

                            if (textInput != null)
                            {
                                submittedAnswer.SubmittedText = textInput.Text;
                            }
                            if (fileUpload != null && fileUpload.HasFile)
                            {
                                submittedAnswer.SubmittedFile = RetrieveSubmittedFile(fileUpload);
                            }
                            answers.Add(submittedAnswer.Answer.Id.Value, submittedAnswer);
                        }
                    }
                }
            }

            return answers;
        }

        private BOFile RetrieveSubmittedFile(FileUpload fileUpload)
        {
            BOFile file = null;

            if (fileUpload != null && fileUpload.HasFile)
            {
                BOCategory folder = formB.GetUploadFolder(UploadFolderId);
                FileInfo fileInfo = new FileInfo(fileUpload.PostedFile.FileName);

                if (folder != null)
                {
                    byte[] fileData;
                    using (fileUpload.PostedFile.InputStream)
                    {
                        fileData = new Byte[fileUpload.PostedFile.InputStream.Length];
                        fileUpload.PostedFile.InputStream.Read(fileData, 0, (int)fileUpload.PostedFile.InputStream.Length);
                        fileUpload.PostedFile.InputStream.Close();
                    }

                    file = new BOFile();
                    file.File = fileData;
                    file.Id = null;
                    file.Folder = folder;
                    file.Name = fileInfo.Name;
                    file.Extension = fileInfo.Extension;
                    file.MimeType = fileUpload.PostedFile.ContentType;
                    file.Size = fileData.Length;
                }
                else
                    throw new Exception("UploadFolder with ID=" + UploadFolderId + " does not exist.");
            }

            return file;
        }

        public static string GenerateRandomCode()
        {
            string s = "";
            for (int i = 0; i < 6; i++)
                s = String.Concat(s, random.Next(10).ToString());
            return s;
        }

        public static string Encrypt(string content, DateTime expiration)
        {
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1, HttpContext.Current.Request.UserHostAddress, DateTime.Now,
                expiration, false, content);
            return FormsAuthentication.Encrypt(ticket);
        }

        public static string Decrypt(string encryptedContent)
        {
            try
            {
                FormsAuthenticationTicket ticket =
                    FormsAuthentication.Decrypt(encryptedContent);
                if (!ticket.Expired) return ticket.UserData;
            }
            catch (ArgumentException) { }
            return null;
        }

        #endregion Helper Methods

    }

    [Serializable]
    public class OFormSubmission : BOFormSubmission
    {
        public enum UserErrorTypes { FileErrorSizeExceeded, FileErrorMimeTypeNotAllowed, FileErrorOther, DropDownErrorFakeAnswerSelected, MenuErrorAnswerRequired, CaptchaErrorMismatch }

        private int? previousSectionId = null;
        private int? currentSectionId = null;
        private int? nextSectionId = null;
        private Dictionary<int, UserErrorTypes> invalidQuestions = new Dictionary<int, UserErrorTypes>();

        public int? PreviousSectionId { get { return previousSectionId; } set { previousSectionId = value; } }
        public int? CurrentSectionId { get { return currentSectionId; } set { currentSectionId = value; } }
        public int? NextSectionId { get { return nextSectionId; } set { nextSectionId = value; } }
        public Dictionary<int, UserErrorTypes> InvalidQuestions { get { return invalidQuestions; } set { invalidQuestions = value; } }
    }

}
