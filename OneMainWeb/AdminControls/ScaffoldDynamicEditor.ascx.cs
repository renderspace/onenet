using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Threading;

using One.Net.BLL.Scaffold;
using One.Net.BLL.Scaffold.Model;

using DropDownList = System.Web.UI.WebControls.DropDownList;
using One.Net.BLL.Web;
using One.Net.BLL;
using One.Net.BLL.WebControls;

namespace OneMainWeb.AdminControls
{
    public class DynamicEditorEventArgs : EventArgs
    {
        public int InsertedId { get; set; }
        public bool IsInsert { get { return InsertedId > 0; } }
    }
    public enum SubmissionStatus { None, Inserted, Updated, ValidationFailed, InternalEvent }

    [Serializable]
    public class EditorSubmission
    {
        public SubmissionStatus Status { get; set; }
    }

    public partial class ScaffoldDynamicEditor : System.Web.UI.UserControl
    {
        static readonly BInternalContent intContentB = new BInternalContent();
        private const int SUGGEST_ENTRIES_IN_DROPDOWN_LIMIT = 40;
        private const string NULL = "NULL";

        public event EventHandler<EventArgs> Exit;
        public event EventHandler<DynamicEditorEventArgs> Saved;
        public event EventHandler<EventArgs> Warning;

        protected EditableItem Item
        {
            get { return ViewState["Item"] as EditableItem; }
            set { ViewState["Item"] = value; }
        }

        public IOrderedDictionary PrimaryKeys
        {
            get { return ViewState["PrimaryKeys"] as IOrderedDictionary; }
            set { ViewState["PrimaryKeys"] = value; }
        }

        public int VirtualTableId
        {
            get { return ViewState["VirtualTableId"] != null ? (int)ViewState["VirtualTableId"] : 0; }
            set { ViewState["VirtualTableId"] = value; }
        }

        protected EditorSubmission Submission
        {
            get { return ViewState["Submission"] as EditorSubmission; }
            set { ViewState["Submission"] = value; }
        }

        public bool IsInsert
        {
            get { return PrimaryKeys == null; }
        }

        public void Clear()
        {
            PanelFieldsHolder.Controls.Clear();
            Submission = null;
            VirtualTableId = 0;
            PrimaryKeys = null;
            Item = null;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
            }
        }
        
        protected override void OnPreRender(EventArgs e)
        {
            if (!IsInsert && (Submission == null || Submission.Status == SubmissionStatus.Inserted || Submission.Status == SubmissionStatus.Updated))
            {
                foreach (var column in Item.Columns.Values)
                {
                    int primaryKey = int.Parse(PrimaryKeys[0].ToString());

                    switch (column.BackendType)
                    {
                        case FieldType.MultiLanguageText:

                            break;
                        case FieldType.ManyToMany:
                            var ManyToManyJoinsListBox = PanelFieldsHolder.FindControl("MML" + column.Ordinal) as ListBox;

                            if (PrimaryKeys.Count != 1)
                                throw new Exception("can make N:M join on tables with multiple primary keys.");

                            if (ManyToManyJoinsListBox != null)
                            {
                                ManyToManyJoinsListBox.Items.Clear();

                                var manyToManyJoinsForThisPrimaryKey = Data.GetManyToMany(column.PartOfRelationId, primaryKey);
                                foreach (var pair in manyToManyJoinsForThisPrimaryKey)
                                {
                                    ManyToManyJoinsListBox.Items.Add(new ListItem(pair.Value, pair.Key.ToString()));
                                }
                            }
                            break;
                        case FieldType.OneToMany:

                            var DropDownListRegular = PanelFieldsHolder.FindControl("FI" + column.Ordinal) as DropDownList;
                            if (DropDownListRegular != null)
                            {

                            }
                            var TextBoxPrimaryKey = PanelFieldsHolder.FindControl("FI" + column.Ordinal) as TextBox;
                            if (TextBoxPrimaryKey != null)
                            {

                            }

                            var TextBoxSuggest = PanelFieldsHolder.FindControl("TBFI" + column.Ordinal) as TextBox;

                            if (TextBoxSuggest != null)
                            {
                                TextBoxSuggest.Text = Data.GetForeignKeySelectedOption(column.PartOfRelationId, primaryKey);
                            }
                            break;
                    }
                }
            }

            ButtonSave.Text = "Save";
            ButtonSaveAndClose.Text = "Save and close";
            base.OnPreRender(e);
        }

        /// <summary>
        /// INamingContainer required manual control tree rebuilding
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Item = Data.GetItem(VirtualTableId, PrimaryKeys);

            if (Item == null)
                return;

            var validationJQueryRules = "";
            var datepickerJQueryCall = "";

            foreach (var column in Item.Columns.Values)
            {
                var PanelField = new Panel();
                PanelField.ID = "PanelField" + column.Ordinal;
                PanelField.CssClass = "field";
                var LabelInfo = new Label
                {
                    CssClass = "info",
                    ID = "LabFI" + column.Ordinal
                };
                // hide primary key on insert
                if (IsInsert && (column.IsPartOfPrimaryKey && !column.IsPartOfForeignKey))
                {
                    PanelField.CssClass += " hidden";
                }
                var LabelDescription = new Label { Text = column.FriendlyName };

                PanelFieldsHolder.Controls.Add(PanelField);

                // Important: all the data is read here, but overwritten when loading viewstate.
                // The most logical thing is probably to move all data reading to prerender event,
                // as it is done for ManyToMany BackendType
                switch (column.BackendType)
                {
                    case FieldType.Integer:
                        PrepareIntegerInput(column, PanelField, LabelDescription, ref validationJQueryRules);
                        break;
                    case FieldType.Decimal:
                        PrepareDecimalInput(column, PanelField, LabelDescription, ref validationJQueryRules);
                        break;
                    case FieldType.SingleText:
                        PrepareTextBoxInput(column, PanelField, LabelDescription, ref validationJQueryRules);
                        break;
                    case FieldType.OneToMany:
                        var v1 = Data.GetForeignKeyOptions(column.PartOfRelationId, SUGGEST_ENTRIES_IN_DROPDOWN_LIMIT);
                        if (v1.Values.Count >= SUGGEST_ENTRIES_IN_DROPDOWN_LIMIT)
                        {
                            PrepareRelationSuggestOptions(column, PanelField, LabelDescription,
                                                          ref validationJQueryRules, false);
                        }
                        else
                        {
                            PrepareRelationOptions(column, PanelField, LabelDescription, v1, ref validationJQueryRules);
                        }
                        break;
                    case FieldType.ManyToMany:
                        PanelField.CssClass += " manyToMany";

                        var v2 = Data.GetForeignKeyOptions(column.PartOfRelationId, SUGGEST_ENTRIES_IN_DROPDOWN_LIMIT);

                        if (v2.Values.Count >= SUGGEST_ENTRIES_IN_DROPDOWN_LIMIT)
                        {
                            PrepareRelationSuggestOptions(column, PanelField, LabelDescription,
                                                          ref validationJQueryRules, true);
                        }
                        else
                        {
                            PrepareRelationOptions(column, PanelField, LabelDescription, v2, ref validationJQueryRules);
                        }

                        var buttonsPanel = new Panel();
                        buttonsPanel.CssClass = "addRemove";
                        var ButtonAddManyToManyRelation = new Button();
                        ButtonAddManyToManyRelation.Text = ">";
                        ButtonAddManyToManyRelation.CommandName = "Add relation";
                        ButtonAddManyToManyRelation.CommandArgument = column.Ordinal.ToString();
                        ButtonAddManyToManyRelation.Click += ButtonAddManyToManyRelation_Click;
                        ButtonAddManyToManyRelation.ValidationGroup = "ADDREMOVE";
                        buttonsPanel.Controls.Add(ButtonAddManyToManyRelation);

                        var ButtonRemoveManyToManyRelation = new Button();
                        ButtonRemoveManyToManyRelation.Text = "<";
                        ButtonRemoveManyToManyRelation.CommandName = "Remove relation";
                        ButtonRemoveManyToManyRelation.CommandArgument = column.Ordinal.ToString();
                        ButtonRemoveManyToManyRelation.Click += ButtonRemoveManyToManyRelation_Click;
                        ButtonRemoveManyToManyRelation.ValidationGroup = "ADDREMOVE";
                        buttonsPanel.Controls.Add(ButtonRemoveManyToManyRelation);

                        PanelField.Controls.Add(buttonsPanel);

                        var ManyToManyJoinsListBox = new ListBox();
                        ManyToManyJoinsListBox.ID = "MML" + column.Ordinal;
                        PanelField.Controls.Add(ManyToManyJoinsListBox);
                        break;
                    case FieldType.Checkbox:
                        var CheckBox1 = new CheckBox();
                        CheckBox1.ID = "FI" + column.Ordinal;
                        CheckBox1.Checked = column.ValueBoolean;
                        LabelDescription.AssociatedControlID = CheckBox1.ID;
                        PanelField.Controls.Add(LabelDescription);
                        PanelField.Controls.Add(CheckBox1);
                        break;
                    case FieldType.Calendar:
                        var DatePicker1 = new TextBox();
                        DatePicker1.ID = "FI" + column.Ordinal;
                        if (column.ValueDateTime != System.Data.SqlTypes.SqlDateTime.MinValue)
                            DatePicker1.Text = ((DateTime)column.ValueDateTime).ToShortDateString();
                        LabelDescription.AssociatedControlID = DatePicker1.ID;
                        PanelField.Controls.Add(LabelDescription);
                        PanelField.Controls.Add(DatePicker1);



                        //datepickerJQueryCall += @"$.datepicker.setDefaults($.datepicker.regional['" + currentCulture.TwoLetterISOLanguageName + "']);" + "\n";
                        //datepickerJQueryCall += @"$.datepicker.setDefaults({ dateFormat: 'd.mm.yy' });" + "\n";


                        datepickerJQueryCall += @"$(""#" + DatePicker1.ClientID + @""").datepicker();" + "\n";
                        //{ dateFormat: 'dd/mm/yy' }
                        //datepickerJQueryCall += @"$(""#" + DatePicker1.ClientID + @""").datepicker('option', { dateFormat: 'd.mm.yy' });";
                        //datepickerJQueryCall += @"$(""#" + DatePicker1.ClientID + @""").datepicker('option', $.extend({showMonthAfterYear: false}));" + "\n";

                        //validationJQueryRules += CreateValidateRule(!column.IsNullable, "date:true", DatePicker1.UniqueID);
                        break;
                    case FieldType.Display:
                        LabelInfo.Text = column.Value;
                        PanelField.Controls.Add(new Label { Text = column.Description, AssociatedControlID = LabelInfo.ID });
                        PanelField.Controls.Add(LabelInfo);
                        break;
                    case FieldType.MultiLanguageText:
                        PrepareMultiLanguageInput(column, PanelField, LabelDescription, ref validationJQueryRules);
                        break;
                    default:
                        LabelInfo.Text = "unsupported type:" + column.DbType.Name + ";requested display type:" +
                                         column.BackendType;
                        PanelField.Controls.Add(new Label { Text = column.Description, AssociatedControlID = LabelInfo.ID });
                        PanelField.Controls.Add(LabelInfo);
                        break;
                }

                var requiredLabel = new Label { Text = "*", CssClass = column.IsRequiredOnInsert ? "req marker" : "marker" };
                PanelField.Controls.Add(requiredLabel);
            }

            var JQueryCode = "";

            if (datepickerJQueryCall.Length > 0)
            {
                var currentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
                JQueryCode += "<script src=\"/_js/jquery.ui.datepicker-sl.js\"></script>";
                // JQueryCode += "<script type=\"text/javascript\" src=\"/_js/i18n/ui.datepicker-" + currentCulture.TwoLetterISOLanguageName + ".js\"></script>\n";
            }
            /*
            JQueryCode = 
                @" <script type=""text/javascript"" charset=""utf-8""> 
jQuery.validator.addMethod( 
                                ""dropdownRequired"", 
                                    function(value, element) { 
                                        if (element.value == ""none"") 
                                            return false; 
                                        else 
                                            return true; 
                                    }, 
	                                ""Please select an option."" 
                                );

                                $(document).ready(function() {
                                    " +
                datepickerJQueryCall + "}); " +
                @" $(""#aspnetForm"").validate({
                                    rules: { 
	                                    " +
                validationJQueryRules.TrimEnd(',') +
                @"}, 
	                                    messages: { 
	                                    comment: ""Please enter a comment.""
	                                } 
                            });    
                     });
</script>
";*/
            JQueryCode += "<script type=\"text/javascript\" charset=\"utf-8\">\n";
            JQueryCode +=
                @"$.validator.addMethod( 
                                ""dropdownRequired"", 
                                    function(value, element) { 
                                        if (element.value == ""none"") 
                                            return false; 
                                        else 
                                            return true; 
                                    }, 
	                                ""Please select an option."" 
                                );";

            JQueryCode += @" $(""#aspnetForm"").validate({
                                    rules: { 
	                                    " + validationJQueryRules.TrimEnd(',') + @"
                                    }, 
                                    messages: { 
                                        comment: ""Please enter a comment.""
                                    } 
                            });";

            JQueryCode += "\n$(document).ready(function() {" + datepickerJQueryCall + "}); ";
            JQueryCode += "</script>";

            LiteralJQuery.Text = JQueryCode;
        }

        private void MarkCurrentStatus(SubmissionStatus status)
        {
            if (Submission == null)
                Submission = new EditorSubmission();

            Submission.Status = status;
        }

        private void ButtonAddManyToManyRelation_Click(object sender, EventArgs e)
        {
            MarkCurrentStatus(SubmissionStatus.InternalEvent);
            var ButtonAddManyToManyRelation = sender as Button;
            if (ButtonAddManyToManyRelation != null)
            {
                var DropDownRelationOptions = PanelFieldsHolder.FindControl("FI" + ButtonAddManyToManyRelation.CommandArgument) as DropDownList;
                var ManyToManyJoinsListBox = PanelFieldsHolder.FindControl("MML" + ButtonAddManyToManyRelation.CommandArgument) as ListBox;
                var TextBoxPrimaryKey = PanelFieldsHolder.FindControl("FI" + ButtonAddManyToManyRelation.CommandArgument) as TextBox;
                var TextBoxSuggest = PanelFieldsHolder.FindControl("TBFI" + ButtonAddManyToManyRelation.CommandArgument) as TextBox;

                string selectedPrimaryKeyValue = NULL;
                ListItem selectedItem = null;

                if (DropDownRelationOptions != null)
                {
                    selectedItem = DropDownRelationOptions.SelectedItem;
                    selectedPrimaryKeyValue = selectedItem.Value;
                }
                else if (TextBoxPrimaryKey != null)
                {
                    if (TextBoxPrimaryKey.Text == NULL)
                    {
                        return;
                    }
                    else
                    {
                        selectedPrimaryKeyValue = TextBoxPrimaryKey.Text;
                        selectedItem = new ListItem();
                        selectedItem.Text = TextBoxSuggest.Text;
                        selectedItem.Value = TextBoxPrimaryKey.Text;
                    }
                }

                var selectedItemInTarget = ManyToManyJoinsListBox.Items.FindByValue(selectedPrimaryKeyValue);

                var intPrimaryKeyValue = 0;
                var hasIntPrimaryKeyValue = int.TryParse(selectedPrimaryKeyValue, out intPrimaryKeyValue);

                if (selectedItemInTarget == null && selectedItem != null && hasIntPrimaryKeyValue)
                {
                    selectedItem.Selected = false;
                    ManyToManyJoinsListBox.Items.Add(selectedItem);

                    // clearing selection on the left after successfull adding
                    if (TextBoxPrimaryKey != null && TextBoxSuggest != null)
                    {
                        TextBoxPrimaryKey.Text = "";
                        TextBoxSuggest.Text = "";
                    }
                }
            }
        }

        private void ButtonRemoveManyToManyRelation_Click(object sender, EventArgs e)
        {
            MarkCurrentStatus(SubmissionStatus.InternalEvent);
            var ButtonAddManyToManyRelation = sender as Button;
            if (ButtonAddManyToManyRelation != null)
            {
                var ManyToManyJoinsListBox =
                    PanelFieldsHolder.FindControl("MML" + ButtonAddManyToManyRelation.CommandArgument) as ListBox;

                if (ManyToManyJoinsListBox != null)
                {
                    var selectedItem = ManyToManyJoinsListBox.SelectedItem;
                    ManyToManyJoinsListBox.Items.Remove(selectedItem);
                }
            }
        }

        private static void PrepareIntegerInput(VirtualColumn column, Panel PanelField, Label LabelDescription, ref string validationJQueryRules)
        {
            var TextBox4 = new TextBox
            {
                ID = ("FI" + column.Ordinal),
                Text = column.Value,
                MaxLength = 11 //(-2,147,483,648 to 2,147,483,647)
            };
            LabelDescription.AssociatedControlID = TextBox4.ID;
            PanelField.Controls.Add(LabelDescription);
            PanelField.Controls.Add(TextBox4);

            validationJQueryRules += CreateValidateRule(!column.IsNullable, "digits:true", TextBox4.UniqueID);
        }

        private static string CreateValidateRule(bool required, string rules, string uniqueId)
        {
            var requiredString = string.IsNullOrEmpty(rules.Trim()) ? "required:true" : "required:true, ";
            return " " + uniqueId + ": {" + (required ? requiredString : "") + rules + "},";
        }

        private static void PrepareDecimalInput(VirtualColumn column, Panel PanelField, Label LabelDescription, ref string validationJQueryRules)
        {
            var TextBox4 = new TextBox
            {
                ID = ("FI" + column.Ordinal),
                Text = column.Value,
                MaxLength = 16
            };
            LabelDescription.AssociatedControlID = TextBox4.ID;
            PanelField.Controls.Add(LabelDescription);
            PanelField.Controls.Add(TextBox4);

            validationJQueryRules += CreateValidateRule(!column.IsNullable, "decimal:true", TextBox4.UniqueID);
        }

        private static void PrepareMultiLanguageInput(VirtualColumn column, Panel panelField, Label descriptionLabel, ref string validationJQueryRules)
        {
            var languages = intContentB.ListLanguages();

            var LabelInfo = new Label { Text = column.Value, CssClass = "info" };
            panelField.Controls.Add(new Label { Text = column.Description, AssociatedControlID = LabelInfo.ID, CssClass = "label" });
            panelField.Controls.Add(LabelInfo);

            foreach (var l in languages)
            {
                var multiLanguageContent = intContentB.Get(column.ValueInteger, l);
                var InputMultiLanguage = new TextBox
                {
                    ID = ("FI" + column.Ordinal + "_" + l),
                    MaxLength = column.Precision,
                    Width = 430,
                    Rows = 4,
                    TextMode = TextBoxMode.MultiLine
                };

                var content = multiLanguageContent;
                if (content != null)
                    InputMultiLanguage.Text = content.Html;

                var label = new Label();
                var ci = new CultureInfo(l);
                label.AssociatedControlID = InputMultiLanguage.ID;
                label.Text = descriptionLabel.Text + " [" + ci.Name + "]";
                panelField.Controls.Add(label);
                panelField.Controls.Add(InputMultiLanguage);

                validationJQueryRules += CreateValidateRule(!column.IsNullable, "", InputMultiLanguage.UniqueID);
            }
        }

        private static void PrepareTextBoxInput(VirtualColumn column, Panel panelField, Label descriptionLabel, ref string validationJQueryRules)
        {
            var Input6 = new TextBox
            {
                ID = ("FI" + column.Ordinal),
                Text = column.Value,
                MaxLength = column.Precision,
                Width = 430
            };

            if (column.Precision < 60)
            {
                int width = (int)Math.Round(((double)column.Precision / 55) * 430);
                Input6.Width = width;
            }
            else if (column.Precision >= 60)
            {
                Input6.TextMode = TextBoxMode.MultiLine;

                if (column.Precision > 4000)
                    Input6.Rows = 6;
                else if (column.Precision > 255)
                    Input6.Rows = 4;
                else
                    Input6.Rows = 1;
            }
            descriptionLabel.AssociatedControlID = Input6.ID;
            panelField.Controls.Add(descriptionLabel);
            panelField.Controls.Add(Input6);

            validationJQueryRules += CreateValidateRule(!column.IsNullable, "", Input6.UniqueID);
        }

        private static void PrepareRelationSuggestOptions(VirtualColumn column, Panel panelField, Label descriptionLabel, ref string validationJQueryRules, bool skipSelectedLookup)
        {
            var TextBoxSuggestIdentification = "FI" + column.Ordinal;

            var TextBoxSuggest = new TextBox();
            TextBoxSuggest.ID = "TB" + TextBoxSuggestIdentification;
            TextBoxSuggest.CssClass = TextBoxSuggestIdentification + " suggest";
            //   if (!skipSelectedLookup)
            //       TextBoxSuggest.Text = BData.GetForeignKeySelectedOption(column.PartOfRelationId, column.ValueInteger);

            var TextBoxPrimaryKey = new TextBox();
            //TextBoxPrimaryKey.ReadOnly = true;
            TextBoxPrimaryKey.CssClass = "PK" + TextBoxSuggestIdentification + " suggest_pk";
            TextBoxPrimaryKey.ID = TextBoxSuggestIdentification;
            TextBoxPrimaryKey.Text = column.ValueInteger.ToString();
            TextBoxPrimaryKey.Attributes.Add("onfocus", "blur()");

            validationJQueryRules += CreateValidateRule(!column.IsNullable, "digits:true", TextBoxPrimaryKey.UniqueID);

            //TextBoxPrimaryKey.Attributes.Add("style", "display: none;");

            var LiteralSuggest = new Literal();
            LiteralSuggest.Text = "<script type=\"text/javascript\">$(document).ready(function() {";
            LiteralSuggest.Text += " $(\"." + TextBoxSuggestIdentification + "\").autocomplete();";
            LiteralSuggest.Text += " $(\"." + TextBoxSuggestIdentification;
            LiteralSuggest.Text += @""").autocomplete({
    source: ""/Utils/BambooOneToManyData.ashx?limit=10&relationId=" + column.PartOfRelationId + @""",
    minLength: 3,
    select: function( event, ui ) {
        console.log(ui.item);
                    console.log(ui.item.value);
                    console.log(ui.item.label);
                    $("".PK" + TextBoxSuggestIdentification + @""").val(ui.item.value);
                    $(""input." + TextBoxSuggestIdentification + @""").val(ui.item.label);
    }
});";
            LiteralSuggest.Text += "}); </script>";

            descriptionLabel.AssociatedControlID = TextBoxSuggest.ID;
            panelField.Controls.Add(descriptionLabel);
            panelField.Controls.Add(TextBoxPrimaryKey);
            panelField.Controls.Add(TextBoxSuggest);
            panelField.Controls.Add(LiteralSuggest);
        }

        private static void PrepareRelationOptions(VirtualColumn column, Panel panelField, Label descriptionLabel, Dictionary<int, string> values, ref string validationJQueryRules)
        {
            var dropDown = new DropDownList
            {
                ID = ("FI" + column.Ordinal),
                Text = column.Description,
                EnableViewState = false
            };

            var nullItem = new ListItem(NULL, NULL);

            if (column.IsNullable)
            {
                nullItem.Attributes.Add("class", NULL);
                dropDown.Items.Add(nullItem);
            }

            foreach (var pair in values)
            {
                var item = new ListItem(pair.Value, pair.Key.ToString());
                if (pair.Key == column.ValueInteger)
                {
                    nullItem.Selected = false;
                    item.Selected = true;
                }
                dropDown.Items.Add(item);
            }

            descriptionLabel.AssociatedControlID = dropDown.ID;
            panelField.Controls.Add(descriptionLabel);
            panelField.Controls.Add(dropDown);

            validationJQueryRules +=
                " " + dropDown.UniqueID
                + ": {"
                + (!column.IsNullable ? "dropdownRequired:true" : "")
                + "},";
        }

        private bool RetreiveSubmittedFields()
        {
            int invalidFields = 0;

            var debug = "<div class=\"debug\"><h4>RetreiveSubmittedFields<h4>";

            foreach (var field in Item.Columns.Values)
            {
                var PanelField = PanelFieldsHolder.FindControl("PanelField" + field.Ordinal) as Panel;
                if (PanelField != null)
                {
                    switch (field.BackendType)
                    {
                        case FieldType.Integer:
                            var TextBoxInteger = PanelField.FindControl("FI" + field.Ordinal) as TextBox;
                            if (TextBoxInteger != null)
                            {
                                field.NewValueInteger = int.Parse(TextBoxInteger.Text);
                            }
                            break;
                        case FieldType.Decimal:
                            var TextBoxDecimal = PanelField.FindControl("FI" + field.Ordinal) as TextBox;
                            if (TextBoxDecimal != null)
                            {
                                field.NewValueDecimal = decimal.Parse(TextBoxDecimal.Text);
                                field.NewValueDouble = double.Parse(TextBoxDecimal.Text);
                            }
                            break;
                        case FieldType.SingleText:
                            var TextBoxSingleText = PanelField.FindControl("FI" + field.Ordinal) as TextBox;
                            if (TextBoxSingleText != null)
                            {
                                field.NewValueString = TextBoxSingleText.Text;
                            }
                            break;
                        case FieldType.Display:
                            var InfoLabelDisplay = PanelField.FindControl("FI" + field.Ordinal) as InfoLabel;
                            if (InfoLabelDisplay != null)
                            {
                                field.NewValueString = InfoLabelDisplay.Value;
                            }
                            break;
                        case FieldType.Checkbox:
                            var CheckBox2 = PanelField.FindControl("FI" + field.Ordinal) as CheckBox;
                            if (CheckBox2 != null)
                            {
                                field.NewValueBoolean = CheckBox2.Checked;
                            }
                            break;
                        case FieldType.Calendar:
                            var DatePickerCalendar = PanelField.FindControl("FI" + field.Ordinal) as TextBox;
                            if (DatePickerCalendar != null)
                            {
                                if (!string.IsNullOrEmpty(DatePickerCalendar.Text))
                                    field.NewValueDateTime = DateTime.Parse(DatePickerCalendar.Text, CultureInfo.CurrentCulture);
                                else
                                    field.NewValueDateTime = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
                            }
                            break;
                        case FieldType.OneToMany:
                            var DropDownListRegular = PanelField.FindControl("FI" + field.Ordinal) as DropDownList;
                            if (DropDownListRegular != null)
                            {
                                if(DropDownListRegular.SelectedValue == NULL)
                                {
                                    field.NewValueIsNull = true;
                                } 
                                else 
                                {
                                    field.NewValueInteger = int.Parse(DropDownListRegular.SelectedValue);
                                }
                                break;
                            }
                            var TextBoxPrimaryKey = PanelField.FindControl("FI" + field.Ordinal) as TextBox;
                            if (TextBoxPrimaryKey != null)
                            {
                                if (TextBoxPrimaryKey.Text == "0" || TextBoxPrimaryKey.Text == NULL || string.IsNullOrWhiteSpace(TextBoxPrimaryKey.Text))
                                {
                                    field.NewValueIsNull = true;
                                }
                                else
                                {
                                    field.NewValueInteger = int.Parse(TextBoxPrimaryKey.Text);
                                }
                            }
                            break;
                        case FieldType.ManyToMany:
                            var ManyToManyJoinsListBox = PanelFieldsHolder.FindControl("MML" + field.Ordinal) as ListBox;
                            if (ManyToManyJoinsListBox != null)
                            {
                                field.NewValueIntegerList = new List<int>();
                                foreach (ListItem c in ManyToManyJoinsListBox.Items)
                                {
                                    int primaryKey = 0;
                                    if (int.TryParse(c.Value, out primaryKey))
                                        field.NewValueIntegerList.Add(primaryKey);
                                }
                            }
                            break;
                        case FieldType.MultiLanguageText:
                            var languages = intContentB.ListLanguages();
                            var multiLanguageContent = new Dictionary<int, BOInternalContent>();
                            foreach (var l in languages)
                            {
                                var InputMultiLanguage = PanelFieldsHolder.FindControl("FI" + field.Ordinal + "_" + l) as TextBox;
                                if (InputMultiLanguage != null)
                                    multiLanguageContent.Add(l, new BOInternalContent { LanguageId = l, Html = InputMultiLanguage.Text, ContentId = field.ValueInteger });
                            }
                            field.NewValueContent = multiLanguageContent;
                            field.NewValueInteger = field.ValueInteger; // this was missing previously and was causing contentId to be 0 on update
                            break;
                        default:
                            break;
                    }
                    debug += "<div class=\"field\">";
                    debug += "<label>" + field.Description + "</label>";
                    if (field.BackendType == FieldType.ManyToMany)
                    {
                        debug += "<span class=\"info\">";
                        var pks = field.NewValue as List<int>;
                        foreach (var s in pks)
                        {
                            debug += s + ", ";
                        }
                        debug += "</span>";
                    }
                    else
                    {
                        debug += "<span class=\"info\">" + field.NewValue + "</span>";
                    }
                    debug += "</div>";
                }
            }
            LiteralResultsDebug.Text += debug + "</div>";
            return invalidFields == 0;
        }

        private bool Save()
        {
            var fieldsRetreived = RetreiveSubmittedFields();

            if (fieldsRetreived)
            {
                var primaryKeys = PrimaryKeys;
                Data.ChangeMultiLanguageContent(Item);
                var result = Data.ChangeItem(VirtualTableId, Item, ref primaryKeys);
                Data.UpdateItemManyToManyFields(VirtualTableId, Item, primaryKeys);
                if (result)
                {
                    if (Saved != null)
                    {
                        if (IsInsert)
                        {
                            MarkCurrentStatus(SubmissionStatus.Inserted);
                            Saved(this, new DynamicEditorEventArgs { InsertedId = (int)primaryKeys[0] });
                            PrimaryKeys = primaryKeys;
                        }
                        else
                        {
                            MarkCurrentStatus(SubmissionStatus.Updated);
                            Saved(this, new DynamicEditorEventArgs());
                        }
                    }
                    Item = Data.GetItem(VirtualTableId, PrimaryKeys);
                    // TODO: find a way to inject PK info into controls
                    return true;
                }

            }
            if (Warning != null)
            {
                Warning(this, new EventArgs());
            }
            return false;
        }

        protected void ButtonSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        protected void ButtonSaveAndClose_Click(object sender, EventArgs e)
        {
            var saved = Save();
            if (saved && Exit != null)
            {
                Clear();
                Exit(this, new EventArgs());
            }
        }

        protected void ButtonCancel_Click(object sender, EventArgs e)
        {
            Clear();
            if (Exit != null)
            {
                Clear();
                Exit(this, new EventArgs());
            }
        }
    }
}