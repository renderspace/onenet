using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services;
using NLog;
using One.Net.BLL;
using One.Net.BLL.Scaffold;

namespace OneMainWeb.Utils
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    public class BambooExcelExport : IHttpHandler
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();

        public void ProcessRequest(HttpContext context)
        {
            int virtualTableId = 0;

            var sb = new StringBuilder();
            var error = false;
            try
            {
                virtualTableId = int.Parse(context.Request.QueryString["virtualTableId"]);

                var state = new ListingState
                {
                    RecordsPerPage = int.MaxValue,
                    FirstRecordIndex = 0,
                    SortDirection = SortDir.Descending,
                    SortField = ""
                };

                var items = Data.ListItems(virtualTableId, state);

                sb.Append("<html xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:x=\"urn:schemas-microsoft-com:office:excel\" xmlns=\"http://www.w3.org/TR/REC-html40\">");
                sb.Append("<head>");
                sb.Append("<meta http-equiv=Content-Type content=\"text/html; charset=windows-1250\">");
                sb.Append("<meta name=ProgId content=Excel.Sheet>");
                sb.Append("<meta name=Generator content=\"Microsoft Excel 11\">");
                sb.Append("<style id=\"STI_5961_Styles\">");
                sb.Append("<!-- ");
                sb.Append("table ");
                sb.Append("{mso-displayed-decimal-separator:\"\\,\";");
                sb.Append(" mso-displayed-thousand-separator:\"\\.\";}");
                sb.Append(".xlGeneral ");
                sb.Append(" {padding-top:1px; padding-right:1px; padding-left:1px; mso-ignore:padding; color:windowtext; font-size:10.0pt; font-weight:400; font-style:normal; text-decoration:none; font-family:Arial; mso-generic-font-family:auto; mso-font-charset:0; mso-background-source:auto; mso-pattern:auto; white-space:nowrap;}");
                sb.Append("</style>");
                sb.Append("</head><body><div id=\"STI_5961\" align=center x:publishsource=\"Excel\">");

                sb.Append("<table border=\"1px\">");
                sb.Append("<tr>");
                foreach (DataColumn col in items.Columns)
                {
                    sb.Append("<th style=\"color:#A10F38;\">" + col.Caption + "</th>");
                }
                sb.Append("</tr>");

                foreach (DataRow row in items.Rows)
                {
                    sb.Append("<tr>");
                    foreach (DataColumn col in items.Columns)
                    {
                        sb.Append("<td class=\"xlGeneral\" align=\"right\">");
                        sb.Append(row[col]);
                        sb.Append("</td>");
                    }
                    sb.Append("</tr>");
                }

                sb.Append("</table>");
                sb.Append("</div>");
                sb.Append("</body>");
                sb.Append("</html>");

            }
            catch (Exception ex)
            {
                log.Error("ExcelExport", ex);
                error = true;
            }

            if (!error)
            {
                context.Response.Clear();
                context.Response.Buffer = true;
                context.Response.ContentType = "application/vnd.ms-excel";
                context.Response.AddHeader("Content-Disposition", "attachment; filename=\"VirtualTable" + virtualTableId + "-" + DateTime.Now.ToShortDateString() + ".xls\";");
                context.Response.ContentEncoding = Encoding.GetEncoding(1250);
                context.Response.Charset = "";
                context.Response.Write(sb.ToString());
            }
            else
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("error on export to excel");
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
