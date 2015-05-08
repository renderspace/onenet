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
using System.Data.SqlTypes;

namespace OneMainWeb.AdminControls
{
    public class DynamicEditorEventArgs : EventArgs
    {
        public DynamicEditorEventArgs()
        {
            Errors = new List<string>();
        }
        public int InsertedId { get; set; }
        public bool IsInsert { get { return InsertedId > 0; } }
        public List<string> Errors { get; set; }
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

        public string DATE_FORMAT = "M/d/yyyy";
        public string TIME_FORMAT = @"h\:mm";

        public string DATE_TIME_FORMAT = @"M/d/yyyy H\:mm";

        public event EventHandler<EventArgs> Exit;
        public event EventHandler<DynamicEditorEventArgs> Saved;
        public event EventHandler<DynamicEditorEventArgs> Error;

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
                            var HiddenFieldPrimaryKey = PanelFieldsHolder.FindControl("FI" + column.Ordinal) as HiddenField;
                            if (HiddenFieldPrimaryKey != null)
                            {
                                HiddenFieldPrimaryKey.Value = primaryKey.ToString();
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

            var PanelFieldT = new Panel();
            PanelFieldT.ID = "PanelTableName";
            PanelFieldT.CssClass = "row";
            PanelFieldT.Attributes.Add("noid", "True");
            PanelFieldsHolder.Controls.Add(PanelFieldT);
            PanelFieldT.Controls.Add(new Literal { Text = "<h3 class=\"col-sm-3 text-right\">" + Item.FriendlyName + "</h3>" });

            foreach (var column in Item.Columns.Values)
            {
                var PanelField = new Panel();
                PanelField.ID = "PanelField" + column.Ordinal;
                PanelField.CssClass = "form-group";
                PanelField.Attributes.Add("noid", "True");
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

                var PanelRight = new Panel { CssClass = "col-sm-9" };
                PanelFieldsHolder.Controls.Add(PanelField);
                var label = column.FriendlyName;
                if (column.BackendType == FieldType.MultiLanguageText)
                    label += " [" + Thread.CurrentThread.CurrentCulture.ThreeLetterISOLanguageName + "]";
                PanelField.Controls.Add(new Literal { Text = "<label class=\"col-sm-3 control-label\">" + label + "</label>" });

                

                // Important: all the data is read here, but overwritten when loading viewstate.
                // The most logical thing is probably to move all data reading to prerender event,
                // as it is done for ManyToMany BackendType
                switch (column.BackendType)
                {
                    case FieldType.ToMany:
                        var toMany = IsInsert ? "<div class=\"alert alert-warning\" role=\"alert\">You need to save the item before you can edit this field.</div>" :
                            "<div class=\"toMany\" data-relation-id=\"" + column.PartOfRelationId +
                            "\" data-friendly-name=\"" + HttpUtility.HtmlEncode(column.FriendlyName) + 
                            "\" data-pk=\"" + PrimaryKeys[0].ToString() + 
                            "\" data-foreign-key-column-name=\"" + column.ForeignKeyColumnName + 
                            "\" data-foreign-key-table-name=\"" + column.ForeignTableName + 
                            "\" data-virtual-table-id=\"" + column.VirtualTableId + "\"></div>";

                        
                        
                        var literalForToMany = new Literal { Text =  toMany };

                        
                        PanelRight.Controls.Add(literalForToMany);
                        PanelRight.CssClass += " jumbotron jumbo-less-padding";
                        PanelField.Controls.Add(PanelRight);
                        break;
                    case FieldType.Integer:
                        PrepareIntegerInput(column, PanelRight, ref validationJQueryRules);
                        PanelField.Controls.Add(PanelRight);
                        break;
                    case FieldType.Decimal:
                        PrepareDecimalInput(column, PanelRight, ref validationJQueryRules);
                        PanelField.Controls.Add(PanelRight);
                        break;
                    case FieldType.SingleText:
                        PrepareTextBoxInput(column, PanelRight, column.Value, ref validationJQueryRules);
                        PanelField.Controls.Add(PanelRight);
                        break;
                    case FieldType.OneToMany:
                        var v1 = Data.GetForeignKeyOptions(column.PartOfRelationId, SUGGEST_ENTRIES_IN_DROPDOWN_LIMIT);
                        if (v1.Values.Count >= SUGGEST_ENTRIES_IN_DROPDOWN_LIMIT)
                        {
                            PrepareRelationSuggestOptions(column, PanelRight, ref validationJQueryRules, false);
                        }
                        else
                        {
                            PrepareRelationOptions(column, PanelRight, v1, ref validationJQueryRules);
                        }
                        PanelField.Controls.Add(PanelRight);
                        break;
                    case FieldType.ManyToMany:
                        var PanelSubLeft = new Panel { CssClass = "col-sm-5" };
                        var PanelSubCenter = new Panel { CssClass = "col-sm-1" };
                        var PanelSubRight = new Panel { CssClass = "col-sm-5" };


                        var l1 = new Label { Text = "Choose category" };
                        PanelSubLeft.Controls.Add(l1);

                        var v2 = Data.GetForeignKeyOptions(column.PartOfRelationId, SUGGEST_ENTRIES_IN_DROPDOWN_LIMIT);
                        if (v2.Values.Count >= SUGGEST_ENTRIES_IN_DROPDOWN_LIMIT)
                        {
                            PrepareRelationSuggestOptions(column, PanelSubLeft, ref validationJQueryRules, true);
                        }
                        else
                        {
                            PrepareRelationOptions(column, PanelSubLeft, v2, ref validationJQueryRules);
                        }

                        var ButtonAddManyToManyRelation = new LinkButton { CssClass = "btn btn-default" };
                        ButtonAddManyToManyRelation.Text = "<span class=\"glyphicon glyphicon-chevron-right\"></span>";
                        ButtonAddManyToManyRelation.CommandName = "Add relation";
                        ButtonAddManyToManyRelation.CommandArgument = column.Ordinal.ToString();
                        ButtonAddManyToManyRelation.Click += ButtonAddManyToManyRelation_Click;
                        ButtonAddManyToManyRelation.ValidationGroup = "ADDREMOVE";
                        PanelSubCenter.Controls.Add(ButtonAddManyToManyRelation);

                        var ButtonRemoveManyToManyRelation = new LinkButton { CssClass = "btn btn-default" };
                        ButtonRemoveManyToManyRelation.Text = "<span class=\"glyphicon glyphicon-chevron-left\"></span>";
                        ButtonRemoveManyToManyRelation.CommandName = "Remove relation";
                        ButtonRemoveManyToManyRelation.CommandArgument = column.Ordinal.ToString();
                        ButtonRemoveManyToManyRelation.Click += ButtonRemoveManyToManyRelation_Click;
                        ButtonRemoveManyToManyRelation.ValidationGroup = "ADDREMOVE";
                        PanelSubCenter.Controls.Add(ButtonRemoveManyToManyRelation);

                        PanelRight.Controls.Add(PanelSubCenter);

                        var ManyToManyJoinsListBox = new ListBox { CssClass = "form-control", ID = "MML" + column.Ordinal };
                        var l2 = new Label { Text = "Selected categories" };
                        PanelSubRight.Controls.Add(l2);
                        PanelSubRight.Controls.Add(ManyToManyJoinsListBox);

                        PanelRight.Controls.Add(PanelSubLeft);
                        PanelRight.Controls.Add(PanelSubCenter);
                        PanelRight.Controls.Add(PanelSubRight);

                        PanelRight.CssClass += " jumbotron jumbo-less-padding";

                        PanelField.Controls.Add(PanelRight);

                        
                        break;
                    case FieldType.Checkbox:
                        var CheckBox1 = new CheckBox();
                        CheckBox1.ID = "FI" + column.Ordinal;
                        CheckBox1.Checked = column.ValueBoolean;

                        var Panel11 = new Panel { CssClass = "col-sm-offset-3 col-sm-9" };
                        var PanelC = new Panel { CssClass = "checkbox" };
                        var LabelC = new Label();
                        var Literal1 = new Literal();
                        var Literal2 = new Literal();

                        Literal1.Text = "<label>";
                        Literal2.Text = column.FriendlyName + "</label>";

                        PanelC.Controls.Add(Literal1);
                        PanelC.Controls.Add(CheckBox1);
                        PanelC.Controls.Add(Literal2);
                        Panel11.Controls.Add(PanelC);

                        PanelField.Controls.RemoveAt(0);
                        PanelField.Controls.Add(Panel11);
                        break;
                    case FieldType.CalendarWithTime:
                        PanelRight.Controls.Add(new Literal { Text = "<div class='input-group date' id='cal-" + column.Ordinal + "'>" });
                        var DatePicker2 = new TextBox { ID = "FI" + column.Ordinal, CssClass = "form-control" };
                        if (column.ValueDateTime != System.Data.SqlTypes.SqlDateTime.MinValue)
                        {
                            DatePicker2.Text = ((DateTime)column.ValueDateTime).ToString(DATE_TIME_FORMAT, CultureInfo.InvariantCulture);
                        }
                        PanelRight.Controls.Add(DatePicker2);
                        PanelRight.Controls.Add(new Literal { Text = "<span class=\"input-group-addon\"><span class=\"glyphicon glyphicon-hourglass\"></span></span></div>" });
                        PanelField.Controls.Add(PanelRight);
                        datepickerJQueryCall += @"$(""#cal-" + column.Ordinal + @""").datetimepicker({format: 'MM/DD/YYYY H:mm'});" + "\n";
                        break;
                    case FieldType.Time:
                        PanelRight.Controls.Add(new Literal { Text = "<div class='input-group date' id='cal-" + column.Ordinal + "'>" });
                        var DatePicker3 = new TextBox { ID = "FI" + column.Ordinal, CssClass = "form-control" };
                        if (!column.ValueIsNull)
                        {
                            DatePicker3.Text = column.ValueTime.ToString(TIME_FORMAT, CultureInfo.InvariantCulture);
                        }
                        PanelRight.Controls.Add(DatePicker3);
                        PanelRight.Controls.Add(new Literal { Text = "<span class=\"input-group-addon\"><span class=\"glyphicon glyphicon-time\"></span></span></div>" });
                        PanelField.Controls.Add(PanelRight);
                        datepickerJQueryCall += @"$(""#cal-" + column.Ordinal + @""").datetimepicker({format: 'H:mm'});" + "\n";
                        break;
                    case FieldType.Calendar:
                        PanelRight.Controls.Add(new Literal { Text = "<div class='input-group date' id='cal-" + column.Ordinal + "'>" });
                        var DatePicker1 = new TextBox { ID = "FI" + column.Ordinal, CssClass = "form-control" };
                        if (column.ValueDateTime != System.Data.SqlTypes.SqlDateTime.MinValue)
                        {
                            DatePicker1.Text = ((DateTime)column.ValueDateTime).ToString(DATE_FORMAT, CultureInfo.InvariantCulture);
                        }
                        PanelRight.Controls.Add(DatePicker1);
                        PanelRight.Controls.Add(new Literal { Text = "<span class=\"input-group-addon\"><span class=\"glyphicon glyphicon-calendar\"></span></span></div>" });
                        PanelField.Controls.Add(PanelRight);
                        datepickerJQueryCall += @"$(""#" + DatePicker1.ClientID + @""").datetimepicker({format: 'MM/DD/YYYY'});" + "\n";
                        break;
                    case FieldType.Display:
                        LabelInfo.Text = column.Value;
                        PanelRight.Controls.Add(LabelInfo);
                        PanelField.Controls.Add(PanelRight);
                        break;
                    case FieldType.MultiLanguageText:
                        PrepareMultiLanguageInput(column, PanelRight, ref validationJQueryRules);
                        PanelField.Controls.Add(PanelRight);
                        break;
                    default:
                        LabelInfo.Text = "unsupported type:" + column.DbType + ";requested display type:" + column.BackendType;
                        PanelRight.Controls.Add(LabelInfo);
                        PanelField.Controls.Add(PanelRight);
                        break;
                }

                //var requiredLabel = new Label { Text = "*", CssClass = column.IsRequiredOnInsert ? "req marker" : "marker" };
                //PanelField.Controls.Add(requiredLabel);
            }

            var JQueryCode = "";

            if (datepickerJQueryCall.Length > 0)
            {
                var currentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
                JQueryCode += "";
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
            var ButtonAddManyToManyRelation = sender as IButtonControl;
            if (ButtonAddManyToManyRelation != null)
            {
                var DropDownRelationOptions = PanelFieldsHolder.FindControl("FI" + ButtonAddManyToManyRelation.CommandArgument) as DropDownList;
                var ManyToManyJoinsListBox = PanelFieldsHolder.FindControl("MML" + ButtonAddManyToManyRelation.CommandArgument) as ListBox;
                var HiddenFieldPrimaryKey = PanelFieldsHolder.FindControl("PKFI" + ButtonAddManyToManyRelation.CommandArgument) as HiddenField;
                var TextBoxSuggest = PanelFieldsHolder.FindControl("TBFI" + ButtonAddManyToManyRelation.CommandArgument) as TextBox;

                string selectedPrimaryKeyValue = NULL;
                ListItem selectedItem = null;

                if (DropDownRelationOptions != null)
                {
                    selectedItem = DropDownRelationOptions.SelectedItem;
                    selectedPrimaryKeyValue = selectedItem.Value;
                }
                else if (HiddenFieldPrimaryKey != null)
                {
                    if (HiddenFieldPrimaryKey.Value == NULL)
                    {
                        return;
                    }
                    else
                    {
                        selectedPrimaryKeyValue = HiddenFieldPrimaryKey.Value;
                        selectedItem = new ListItem();
                        selectedItem.Text = TextBoxSuggest.Text;
                        selectedItem.Value = HiddenFieldPrimaryKey.Value;
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
                    if (HiddenFieldPrimaryKey != null && TextBoxSuggest != null)
                    {
                        HiddenFieldPrimaryKey.Value = "";
                        TextBoxSuggest.Text = "";
                    }
                }
            }
        }

        private static string CreateValidateRule(bool required, string rules, string uniqueId)
        {
            return "";
            /*
            var requiredString = string.IsNullOrEmpty(rules.Trim()) ? "required:true" : "required:true, ";
            return " " + uniqueId + ": {" + (required ? requiredString : "") + rules + "},";
             * */
        }

        private void ButtonRemoveManyToManyRelation_Click(object sender, EventArgs e)
        {
            MarkCurrentStatus(SubmissionStatus.InternalEvent);
            var ButtonAddManyToManyRelation = sender as IButtonControl;
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

        private static void PrepareIntegerInput(VirtualColumn column, Panel PanelField, ref string validationJQueryRules)
        {
            var TextBox4 = new TextBox
            {
                ID = ("FI" + column.Ordinal),
                Text = column.Value,
                CssClass = "form-control",
                MaxLength = 11 //(-2,147,483,648 to 2,147,483,647)
            };
            TextBox4.Attributes.Add("type", "number");
            TextBox4.CssClass += " digits";
            if (!column.IsNullable)
                TextBox4.CssClass += " required";
            PanelField.Controls.Add(TextBox4);
        }

        private static void PrepareDecimalInput(VirtualColumn column, Panel PanelField, ref string validationJQueryRules)
        {
            var TextBox4 = new TextBox
            {
                ID = ("FI" + column.Ordinal),
                Text = column.Value.ToString(CultureInfo.InvariantCulture).Replace(",","."),
                CssClass = "form-control",
                MaxLength = 16
            };
            TextBox4.CssClass += " number";
            if (!column.IsNullable)
                TextBox4.CssClass += " required";
            PanelField.Controls.Add(TextBox4);
        }

        private static void PrepareMultiLanguageInput(VirtualColumn column, Panel panelField, ref string validationJQueryRules)
        {
            var languages = intContentB.ListLanguages();
            var multiLanguageContent = intContentB.Get(column.ValueInteger, Thread.CurrentThread.CurrentCulture.LCID);

            var value = "";
            if (multiLanguageContent != null)
                value = multiLanguageContent.Html;

            var Input6 = new TextBox
            {
                ID = ("FI" + column.Ordinal + "_" + Thread.CurrentThread.CurrentCulture.LCID),
                Text = value,
                TextMode = TextBoxMode.MultiLine,
                CssClass = "form-control",
                Rows = 6
            };

            if (column.IsWysiwyg)
                Input6.CssClass += " ckeditor4";

            panelField.Controls.Add(Input6);
            validationJQueryRules += CreateValidateRule(false, "", Input6.UniqueID);
        }

        private static void PrepareTextBoxInput(VirtualColumn column, Panel panelField, string value, ref string validationJQueryRules)
        {
            var Input6 = new TextBox
            {
                ID = ("FI" + column.Ordinal),
                Text = value,
                MaxLength = column.Precision,
            };

            if (column.Precision < 257)
            {
                //int width = (int)Math.Round(((double)column.Precision / 55) * 430);
                Input6.Style.Add("width", "100%");
            }
            else
            {
                Input6.TextMode = TextBoxMode.MultiLine;

                if (column.Precision > 4000)
                    Input6.Rows = 6;
                else 
                    Input6.Rows = 4;
            }
            Input6.CssClass = "form-control";
            if (column.IsWysiwyg)
                Input6.CssClass += " ckeditor4";
            panelField.Controls.Add(Input6);
            validationJQueryRules += CreateValidateRule(false, "", Input6.UniqueID);
        }

        private static void PrepareRelationSuggestOptions(VirtualColumn column, Panel panelField, ref string validationJQueryRules, bool skipSelectedLookup)
        {
            var suggestIdentification = "FI" + column.Ordinal;

            panelField.CssClass += " " + suggestIdentification;

            var TextBoxSuggest = new TextBox();
            TextBoxSuggest.ID = "TB" + suggestIdentification;
            if(!column.IsNullable)
                TextBoxSuggest.CssClass += " required";

            TextBoxSuggest.CssClass += " form-control suggest ";
            //   if (!skipSelectedLookup)
            //       TextBoxSuggest.Text = BData.GetForeignKeySelectedOption(column.PartOfRelationId, column.ValueInteger);

            var HiddenFieldPrimaryKey = new HiddenField();
            HiddenFieldPrimaryKey.ID = "PK" + suggestIdentification;
            //TextBoxPrimaryKey.ReadOnly = true;
            // HiddenFieldPrimaryKey.CssClass = "PK" + TextBoxSuggestIdentification + " suggest_pk";

            
            HiddenFieldPrimaryKey.Value = column.ValueInteger.ToString();
            // HiddenFieldPrimaryKey.Attributes.Add("onfocus", "blur()");

            // validationJQueryRules += CreateValidateRule(!column.IsNullable, "digits:true", HiddenFieldPrimaryKey.UniqueID);

            //TextBoxPrimaryKey.Attributes.Add("style", "display: none;");
            var LiteralSuggest = new Literal();
            LiteralSuggest.Text = "<script type=\"text/javascript\">$(document).ready(function() {";
            LiteralSuggest.Text += " $(\"." + suggestIdentification + @" .suggest"").autocomplete({
    source: ""/Utils/BambooOneToManyData.ashx?limit=10&relationId=" + column.PartOfRelationId + @""",
    minLength: 2,
    select: function( event, ui ) {
        trace(ui.item);
                    $(""." + suggestIdentification + @" input[type=hidden]"").val(ui.item.value);
                    $(""." + suggestIdentification + @" .suggest"").val(ui.item.label);
                    return false;
    }
});";
            LiteralSuggest.Text += "}); </script>";

            panelField.Controls.Add(HiddenFieldPrimaryKey);
            panelField.Controls.Add(TextBoxSuggest);
            panelField.Controls.Add(LiteralSuggest);
        }

        private static void PrepareRelationOptions(VirtualColumn column, Panel panelField, Dictionary<int, string> values, ref string validationJQueryRules)
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
            panelField.Controls.Add(dropDown);
        }

        private List<string> RetreiveSubmittedFields()
        {
            var errors = new List<string>();
            var defaultCulture = new CultureInfo(1033);
            var debug = "<div class=\"debug hidden\"><h4>RetreiveSubmittedFields</h4>";

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
                                var parsedDecimal = 0m;
                                var hasDecimal = decimal.TryParse(TextBoxDecimal.Text.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out parsedDecimal);
                                if (hasDecimal)
                                {
                                    field.NewValueDecimal = parsedDecimal;
                                    field.NewValueDouble = (double) parsedDecimal;
                                }
                                else if (field.IsNullable)
                                    field.NewValueIsNull = true;
                                else
                                    errors.Add("Non-nullable decimal field " + field.FriendlyName + " is missing a value or value is not in correct format.");
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
#warning WTF? Display? Save?
                            var InfoLabelDisplay = PanelField.FindControl("FI" + field.Ordinal) as Label;
                            if (InfoLabelDisplay != null)
                            {
                                field.NewValueString = InfoLabelDisplay.Text;
                            }
                            break;
                        case FieldType.Checkbox:
                            var CheckBox2 = PanelField.FindControl("FI" + field.Ordinal) as CheckBox;
                            if (CheckBox2 != null)
                            {
                                field.NewValueBoolean = CheckBox2.Checked;
                            }
                            break;
                        case FieldType.Time:
                            var DatePickerTime = PanelField.FindControl("FI" + field.Ordinal) as TextBox;
                            if (DatePickerTime != null)
                            {
                                TimeSpan parsedTime = new TimeSpan(0);
                                var hasTime = TimeSpan.TryParseExact(DatePickerTime.Text, TIME_FORMAT, defaultCulture, TimeSpanStyles.None, out parsedTime);
                                if (hasTime)
                                {
                                    field.NewValueTime = parsedTime;
                                }
                                else if (field.IsNullable)
                                {
                                    field.NewValueIsNull = true;
                                }
                                else
                                {
                                    errors.Add("Non-nullable date field " + field.FriendlyName + " is missing a value or value is not in correct format.");
                                }
                            }
                            break;
                        case FieldType.CalendarWithTime:
                        case FieldType.Calendar:
                            var DatePickerCalendar = PanelField.FindControl("FI" + field.Ordinal) as TextBox;
                            if (DatePickerCalendar != null)
                            {
                                DateTime parsedDate = (DateTime) SqlDateTime.MinValue;
                                TimeSpan parsedTime = new TimeSpan(0);
                                var dateStringToParse = "";
                                var hasTime = false;
                                if (DatePickerCalendar.Text.Trim().Contains(" "))
                                {
                                    var split = DatePickerCalendar.Text.Trim().Split(' ');
                                    if (split.Count() > 0)
                                    {
                                        dateStringToParse = split[0];
                                    }
                                    if (split.Count() > 1)
                                    {
                                        hasTime = TimeSpan.TryParseExact(split[1], TIME_FORMAT, CultureInfo.InvariantCulture, TimeSpanStyles.None, out parsedTime);
                                    } 
                                }
                                else
                                {
                                    dateStringToParse = DatePickerCalendar.Text;
                                }

                                var hasDate = DateTime.TryParseExact(dateStringToParse, DATE_FORMAT, defaultCulture, DateTimeStyles.None, out parsedDate);

                                if (hasDate)
                                {
                                    field.NewValueDateTime = parsedDate;
                                    if (hasTime)
                                    {
                                        field.NewValueDateTime += parsedTime;
                                    }
                                }
                                else if (field.IsNullable)
                                {
                                    field.NewValueIsNull = true;
                                }
                                else
                                {
                                    errors.Add("Non-nullable date field " + field.FriendlyName + " is missing a value or value is not in correct format.");
                                }
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
                            var HiddenFieldPrimaryKey = PanelField.FindControl("PKFI" + field.Ordinal) as HiddenField;
                            if (HiddenFieldPrimaryKey != null)
                            {
                                if (HiddenFieldPrimaryKey.Value == "0" || HiddenFieldPrimaryKey.Value == NULL || string.IsNullOrWhiteSpace(HiddenFieldPrimaryKey.Value))
                                {
                                    field.NewValueIsNull = true;
                                }
                                else
                                {
                                    field.NewValueInteger = int.Parse(HiddenFieldPrimaryKey.Value);
                                }
                            }
                            break;
                        case FieldType.ManyToMany:
                            var ManyToManyJoinsListBox = PanelFieldsHolder.FindControl("MML" + field.Ordinal) as ListBox;
                            if (ManyToManyJoinsListBox != null)
                            {
                                field.BackendType = FieldType.ManyToMany;
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
                        var pks = field.NewValueIntegerList;
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
            return errors;
        }

        private bool Save()
        {
            var errors = RetreiveSubmittedFields();

            if (errors.Count == 0)
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
            else
            {
                if (Error != null)
                {
                    var result = new DynamicEditorEventArgs();
                    result.Errors = errors;
                    Error(this, result);
                }
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
    }
}