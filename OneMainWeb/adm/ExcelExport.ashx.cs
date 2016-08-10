using One.Net.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using One.Net.Forms;

namespace Forms.adm
{
    /// <summary>
    /// Summary description for ExcelExport
    /// </summary>
    public class ExcelExport : IHttpHandler
    {
        protected static BForm formB = new BForm();

        public void ProcessRequest(HttpContext context)
        {

            var id = int.Parse(context.Request["id"].ToString());

            var form = formB.Get(id);
            if (form != null)
            {
                var type = context.Request["type"].ToString();

                var filename = form.Title + DateTime.Now.ToString();

                context.Response.ContentType = "text/plain";
                context.Response.Write("Hello World");

                context.Response.Clear();
                context.Response.Buffer = true;
                context.Response.ContentType = "application/vnd.ms-excel";
                context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + filename + ".xls\";");
                context.Response.ContentEncoding = System.Text.Encoding.GetEncoding(1250);
                context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                context.Response.Charset = "";

                string result = "";
                switch (type)
                {
                    case "form_all_submissions":
                        result = ExportAllSubmissions(id);
                        break;
                    case "form_agregate":
                        result = ExportAgregate(id);
                        break;
                }
                context.Response.Write(result);
                context.Response.End();
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private string ExportAgregate(int formId)
        {
            var form = formB.Get(formId);
            // Prepare to export the data
            System.IO.StringWriter strw = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter htmlw = new System.Web.UI.HtmlTextWriter(strw);

            // START head
            strw.GetStringBuilder().Append(
                @"<html xmlns:o=""urn:schemas-microsoft-com:office:office"" xmlns:x=""urn:schemas-microsoft-com:office:excel"" xmlns=""http://www.w3.org/TR/REC-html40"">
                      <head>
                            <meta http-equiv=Content-Type content=""text/html; charset=windows-1250"">
                            <meta name=""ProgId"" content=""Excel.Sheet"">
                            <meta name=""Generator"" content=""Microsoft Excel 11"">
                            <style>
                                <!-- 
                                
                                .general {color:black; font-size:13.0pt; font-weight:400;}
                                .generalsmall {color:black; font-size:9.0pt; font-weight:bold;}
                                .question { background:#CCCCFF; color:black; font-size:13.0pt; font-weight:400; }
                                .openAnswer { 	background:lime; color:black; font-size:13.0pt; font-weight:400; }
                                .singleAnswer { background:#FF9900; color:black; font-size:13.0pt; font-weight:400; }
                                .multipleChoiceAnswer { background:#FF6600; color:black; font-size:13.0pt; font-weight:400; }

                                -->
                            </style>
                      </head>
                      <body><div id=""STI_5961"" align=""center"" x:publishsource=""Excel"">");
            // END head

            // START DETAIL

            strw.GetStringBuilder().Append(
                @"<table border=""1px"">
                        <tr>    
                            <th class=""generalsmall"" colspan=""2"">Export details</th>
                        </tr>
                        <tr>
                            <td class=""generalsmall"" align=""center"">Export date</td>
                            <td class=""general"" align=""right"">" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + @"</td>
                        </tr>
                        <tr>
                            <td class=""generalsmall"" align=""center"">Export time</td>
                            <td class=""general"" align=""right"">" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + @"</td>
                        </tr>
                        <tr>
                            <td class=""generalsmall"" align=""center"">Export principal</td>
                            <td class=""general"" align=""right"">" +  @"</td>
                        </tr>
                    </table><br />");

            strw.GetStringBuilder().Append(
                @"<table border=""1px"">
                        <tr>
                            <th class=""generalsmall"" colspan=""2"">Form details</th>
                        </tr>
                        <tr>
                            <td class=""generalsmall"" align=""center"">Form title</td>
                            <td class=""general"" align=""right"">" + HttpUtility.HtmlEncode(form.Title.StripHtmlTags()) + @"</td>
                        </tr>
                        <tr>
                            <td class=""generalsmall"" align=""center"">Form type</td>
                            <td class=""general"" align=""right"">" + form.FormType + @"</td>
                        </tr>
                        <tr>
                            <td class=""generalsmall"" align=""center"">Submission count</td>
                            <td class=""general"" align=""right"">" + form.SubmissionCount + @"</td>
                        </tr>");

            if (form.FirstSubmissionDate.HasValue)
            {
                strw.GetStringBuilder().Append(
                    @"<tr>
                            <td class=""generalsmall"" align=""center"">First submission date</td>
                            <td class=""general"" align=""right"">" + form.FirstSubmissionDate.Value.ToString("MM/dd/yyyy HH:mm:ss") + @"</td>
                          </tr>");
            }

            if (form.LastSubmissionDate.HasValue)
            {
                strw.GetStringBuilder().Append(
                    @"<tr>
                            <td class=""generalsmall"" align=""center"">Last submission date</td>
                            <td class=""general"" align=""right"">" + form.LastSubmissionDate.Value.ToString("MM/dd/yyyy HH:mm:ss") + @"</td>
                          </tr>");
            }

            strw.GetStringBuilder().Append("</table><br />");

            strw.GetStringBuilder().Append(
                @"<table border=""1px"">
                        <tr>
                            <th class=""generalsmall"">Answer legend</th>
                        </tr>
                        <tr>
                            <td class=""singleAnswer"">Single answer</td>
                        </tr>                        
                        <tr>
                            <td class=""openAnswer"">Open answer</td>
                        </tr>
                        <tr>
                            <td class=""multipleChoiceAnswer"">Multiple choice answer</td>
                        </tr></table><br />");


            strw.GetStringBuilder().Append(
                @"<table border=""1px"">
                        <tr>
                            <th class=""generalsmall"">Question idx</th>
                            <th class=""generalsmall"">Question</th>
                            <th class=""generalsmall"">Total answers</th>
                            <th class=""generalsmall"">Answers</th>
                            <th class=""generalsmall"">Total individual answers</th>
                            <th class=""generalsmall"">Percentage for all submissions</th>
                        </tr>");


            foreach (BOSection section in form.Sections.Values)
            {

                foreach (BOQuestion question in section.Questions.Values)
                {
                    strw.GetStringBuilder().Append(
                        @"<tr>
                                <td class=""general"" align=""center"">[" + section.Idx.ToString() + @"]/[" + question.Idx.ToString() + @"]</td>
                                <td class=""question"" align=""center"">" + HttpUtility.HtmlEncode(question.Title.StripHtmlTags()) + @"</td>
                                <td class=""general"" align=""center"">" + question.TimesAnswered + @"</td>
                                <td align=""center"">");

                    strw.GetStringBuilder().Append(@"<table border=""1px"">");
                    foreach (BOAnswer answer in question.Answers.Values)
                    {
                        string answerTitle = "";
                        if (answer.AnswerType == AnswerTypes.SingleText)
                        {
                            answerTitle = "Textual answer";
                        }
                        else if (answer.AnswerType == AnswerTypes.SingleFile)
                        {
                            answerTitle = "File answer";
                        }
                        else
                        {
                            answerTitle = answer.Title.StripHtmlTags();
                        }

                        string answerCssClass = "";

                        if (answer.AnswerType == AnswerTypes.Checkbox || answer.AnswerType == AnswerTypes.DropDown || answer.AnswerType == AnswerTypes.Radio)
                        {
                            answerCssClass = @"multipleChoiceAnswer";
                            if (answer.AdditionalFieldType == AdditionalFieldTypes.Text)
                            {
                                answerCssClass = @"openAnswer";
                            }
                        }
                        else if (answer.AnswerType == AnswerTypes.SingleText)
                        {
                            answerCssClass = @"singleAnswer";
                        }

                        if (!answer.IsFake)
                        {
                            strw.GetStringBuilder().Append(@"<tr><td class=""" + answerCssClass + @""">" + HttpUtility.HtmlEncode(answerTitle) + @"</td></tr>");
                        }
                    }
                    strw.GetStringBuilder().Append("</table>");

                    strw.GetStringBuilder().Append(@"</td><td class=""general"" align=""center"">");

                    strw.GetStringBuilder().Append(@"<table border=""1px"">");
                    foreach (BOAnswer answer in question.Answers.Values)
                    {
                        strw.GetStringBuilder().Append(@"<tr><td class=""general"" align=""center"">" + answer.TimesAnswered + @"</td></tr>");
                    }
                    strw.GetStringBuilder().Append("</table>");

                    strw.GetStringBuilder().Append(@"</td><td class=""general"" align=""center"">");

                    strw.GetStringBuilder().Append(@"<table border=""1px"">");
                    foreach (BOAnswer answer in question.Answers.Values)
                    {
                        strw.GetStringBuilder().Append(@"<tr><td class=""general"" align=""center"">");
                        if (answer.AnswerType != AnswerTypes.SingleText && answer.AnswerType != AnswerTypes.SingleFile)
                        {
                            strw.GetStringBuilder().Append(string.Format("{0:#0.00'%}", answer.PercentageAnswered));
                        }
                        strw.GetStringBuilder().Append(@"</td></tr>");
                    }
                    strw.GetStringBuilder().Append("</table>");

                    strw.GetStringBuilder().Append(@"</td><td class=""general"" align=""center"">");

                    strw.GetStringBuilder().Append(@"<table border=""1px"">");
                    foreach (BOAnswer answer in question.Answers.Values)
                    {
                        strw.GetStringBuilder().Append(@"<tr><td class=""general"" align=""center"">" + string.Format("{0:#0.00'%}", answer.OverallPercentageAnswered) + @"</td></tr>");
                    }
                    strw.GetStringBuilder().Append("</table>");

                    strw.GetStringBuilder().Append(@"</td></tr>");
                }
            }

            strw.GetStringBuilder().Append("</table><br />");
            // END DETAIL

            // START tail
            strw.GetStringBuilder().Append("</div></body></html>");
            // END tail
            return strw.ToString();
        }
        
        private string ExportAllSubmissions(int formId)
        {
            var submissions = formB.ListFormSubmissions(formId, new ListingState());
            var form = formB.Get(formId);
            

            // Prepare to export the data
            System.IO.StringWriter strw = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter htmlw = new System.Web.UI.HtmlTextWriter(strw);

            // START head
            strw.GetStringBuilder().Append(
                @"<html xmlns:o=""urn:schemas-microsoft-com:office:office"" xmlns:x=""urn:schemas-microsoft-com:office:excel"" xmlns=""http://www.w3.org/TR/REC-html40"">
                      <head>
                            <meta http-equiv=""Content-Type"" content=""text/html; charset=windows-1250"">
                            <meta name=""ProgId"" content=""Excel.Sheet"">
                            <meta name=""Generator"" content=""Microsoft Excel 11"">
                            <style>
                                <!-- 

                                .general {mso-number-format:\@; color:black; font-size:13.0pt; font-weight:400;}
                                .generalsmall {mso-number-format:\@; color:black; font-size:9.0pt; font-weight:bold;}
                                .question {mso-number-format:\@; background:#CCCCFF; color:black; font-size:13.0pt; font-weight:400; }
                                .openAnswer {mso-number-format:\@; background:lime; color:black; font-size:13.0pt; font-weight:400; }
                                .singleAnswer {mso-number-format:\@; background:#FF9900; color:black; font-size:13.0pt; font-weight:400; }
                                .multipleChoiceAnswer { background:#FF6600; color:black; font-size:13.0pt; font-weight:400; }

                                -->
                            </style>
                      </head>
                      <body><div id=""STI_5961"" align=""center"" x:publishsource=""Excel"">");
            // END head

            // START DETAIL

            strw.GetStringBuilder().Append(
                @"<table border=""1px"">
                        <tr>
                            <th class=""generalsmall"">Answer legent</th>
                        </tr>
                        <tr>
                            <td class=""singleAnswer"">Single answer</td>
                        </tr>                        
                        <tr>
                            <td class=""openAnswer"">Open answer</td>
                        </tr>
                        <tr>
                            <td class=""multipleChoiceAnswer"">Multiple choice answer</td>
                        </tr></table><br />");

            strw.GetStringBuilder().Append(
                @"<table border=""1px""><tr>");
            strw.GetStringBuilder().Append(@"<th class=""generalsmall"">Id</th>");
            strw.GetStringBuilder().Append(@"<th class=""generalsmall"">Date submitted</th>");
            foreach (var question in form.Questions)
            {
                strw.GetStringBuilder().Append(@"<th class=""generalsmall"">" + HttpUtility.HtmlEncode(question.Title.StripHtmlTags()) + @"</th>");
            }
            strw.GetStringBuilder().Append("</tr>");

            foreach (var submission in submissions)
            {
                strw.GetStringBuilder().Append("<tr>");
                strw.GetStringBuilder().Append(@"<td class=""general"" align=""center"">" + submission.Id.Value + @"</td>");
                strw.GetStringBuilder().Append(@"<td class=""general"" align=""center"">" + (submission.Finished.HasValue ? submission.Finished.Value.ToString("MM/dd/yyyy HH:mm:ss") : "") + @"</td>");
                foreach (var question in form.Questions)
                {
                    strw.GetStringBuilder().Append(@"<td align=""center"">");

                    if (submission.SubmittedQuestions.ContainsKey(question.Id.Value))
                    {
                        strw.GetStringBuilder().Append(@"<table border=""1px"">");
                        foreach (BOSubmittedAnswer submittedAnswer in submission.SubmittedQuestions[question.Id.Value].SubmittedAnswers.Values)
                        {
                            string answerCssClass = "";

                            if (submittedAnswer.Answer.AnswerType == AnswerTypes.Checkbox || submittedAnswer.Answer.AnswerType == AnswerTypes.DropDown || submittedAnswer.Answer.AnswerType == AnswerTypes.Radio)
                            {
                                answerCssClass = @"multipleChoiceAnswer";
                                if (submittedAnswer.Answer.AdditionalFieldType == AdditionalFieldTypes.Text)
                                {
                                    answerCssClass = @"openAnswer";
                                }
                            }
                            else if (submittedAnswer.Answer.AnswerType == AnswerTypes.SingleText)
                            {
                                answerCssClass = @"singleAnswer";
                            }

                            strw.GetStringBuilder().Append(@"<tr><td align=""center"" class=""" + answerCssClass + @""">");

                            if (submittedAnswer.Answer.AnswerType == AnswerTypes.SingleText ||
                                submittedAnswer.Answer.AdditionalFieldType == AdditionalFieldTypes.Text)
                            {
                                strw.GetStringBuilder().Append(HttpUtility.HtmlEncode(submittedAnswer.SubmittedText));
                            }
                            else if (submittedAnswer.Answer.AnswerType == AnswerTypes.SingleFile && submittedAnswer.SubmittedFile != null)
                            {
                                strw.GetStringBuilder().Append(submittedAnswer.SubmittedFile.Name);
                            }
                            else
                            {
                                strw.GetStringBuilder().Append(HttpUtility.HtmlEncode(submittedAnswer.Answer.Title.StripHtmlTags()));
                            }

                            strw.GetStringBuilder().Append(@"</td></tr>");
                        }
                        strw.GetStringBuilder().Append(@"</table>");
                    }
                    strw.GetStringBuilder().Append(@"</td>");
                }
                strw.GetStringBuilder().Append("</tr>");
            }

            strw.GetStringBuilder().Append("</table><br />");
            // END DETAIL

            // START tail
            strw.GetStringBuilder().Append("</div></body></html>");
            // END tail

            return strw.ToString();
        }
    }
}