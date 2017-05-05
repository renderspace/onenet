using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;

using One.Net.BLL;
using One.Net.BLL.Utility;
using One.Net.BLL.Web;
using One.Net.BLL.Model.Attributes;

namespace OneMainWeb.CommonModules
{
    public partial class TemplateContent : MModule
    {
        [Setting(SettingType.Int, DefaultValue = "-1")]
        public int TemplateId { get { return GetIntegerSetting("TemplateId"); } }

        [Setting(SettingType.Int, DefaultValue = "-1", Visibility=SettingVisibility.SPECIAL)]
        public int ContentTemplateId { get { throw new Exception("not intended for direct access"); } }

        [Setting(SettingType.Bool, DefaultValue = "false")]
        public bool ShowOnlyOnProduction { get { return GetBooleanSetting("ShowOnlyOnProduction"); } }

        protected string CurrentUri
        {
            get
            {
                var builder = new UrlBuilder(Page);
                return builder.ToString();
            }        
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            LiteralTemplateOutput.Text = "";
            var showMe = true;
            if (ShowOnlyOnProduction && !PublishFlag)
            {
                showMe = false;
            }

            if (TemplateId > 0 && showMe)
            {
                var templateFields = new Dictionary<string, string>();

                var t = BWebsite.GetTemplate(TemplateId);
                if (t != null)
                {
                    if (!string.IsNullOrEmpty(t.TemplateContent))
                    {
                        LiteralTemplateOutput.Text = t.TemplateContent;

                        const string pattern = @"\{([^\}]+)\}";
                        foreach (Match match in Regex.Matches(t.TemplateContent, pattern))
                        {
                            var pairString = match.Value.Replace("{", "").Replace("}", "");
                            var pair = StringTool.SplitString(pairString);
                            var key = pair[0];
                            var value = (pair.Count > 1 ? pair[1] : "");

                            if (!templateFields.ContainsKey(key))
                            {
                                templateFields.Add(key, value);
                            }
                        }
                    }
                }

                if (templateFields.Count > 0)
                {
                    var contentTemplateB = new BContentTemplate();
                    var contentTemplate = contentTemplateB.GetContentTemplate(this.InstanceId);

                    if (contentTemplate != null && contentTemplate.ContentFields != null)
                    {
                        foreach (var field in templateFields)
                        {
                            if (field.Value == "builtin")
                            {
                                var valueToReplaceWith = "";
                                if (field.Key == "currenturi")
                                {
                                    valueToReplaceWith = HttpUtility.UrlEncode(CurrentUri);
                                }
                                    
                                LiteralTemplateOutput.Text = LiteralTemplateOutput.Text.Replace("{" + field.Key + "," + field.Value + "}", valueToReplaceWith);
                            }
                            else if (contentTemplate.ContentFields.ContainsKey(field.Key))
                            {
                                if (string.IsNullOrEmpty(field.Value))
                                {
                                    LiteralTemplateOutput.Text = LiteralTemplateOutput.Text.Replace("{" + field.Key + "}", contentTemplate.ContentFields[field.Key]);
                                }
                                else
                                {
                                    if (field.Value == "repeatedinput")
                                    {
                                        var value = contentTemplate.ContentFields[field.Key].Replace("|||", ""); // remove ||| we use for repeated fields.
                                        LiteralTemplateOutput.Text = LiteralTemplateOutput.Text.Replace("{" + field.Key + "," + field.Value + "}", value);
                                    }
                                    else
                                    {
                                        LiteralTemplateOutput.Text = LiteralTemplateOutput.Text.Replace("{" + field.Key + "," + field.Value + "}", contentTemplate.ContentFields[field.Key]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}