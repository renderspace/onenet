using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Data.SqlTypes;

using NLog;
using One.Net.BLL;
using One.Net.BLL.Scaffold;

namespace OneMainWeb.Utils
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    public class BambooCsvExport : IHttpHandler
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

                var columns = "";
                foreach (DataColumn col in items.Columns)
                {
                    columns += col.Caption + ",";
                }
                sb.Append(columns.TrimEnd(',') + Environment.NewLine);

                var temp = "";
                foreach (DataRow row in items.Rows)
                {
                    temp = "";
                    foreach (DataColumn col in items.Columns)
                    {
                        temp += SanitizeValuesForCSV(row[col], ",") + ",";
                    }
                    sb.Append(temp.TrimEnd(',') + Environment.NewLine);
                }

            }
            catch (Exception ex)
            {
                log.Error(ex, "CsvExport");
                error = true;
            }

            if (!error)
            {
                context.Response.Clear();
                context.Response.Buffer = true;
                context.Response.ContentType = "text/csv";
                context.Response.AddHeader("Content-Disposition", "attachment; filename=\"VirtualTable" + virtualTableId + "-" + DateTime.Now.ToShortDateString() + ".csv\";");
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.Charset = "";
                context.Response.Write(sb.ToString());
            }
            else
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("error on export to csv");
            }
        }

        private static string SanitizeValuesForCSV(object value, string delimeter)
        {
            string output;

            if (value == null) return "";

            if (value is DateTime)
            {
                output = ((DateTime)value).ToLongDateString();
            }
            else
            {
                output = value.ToString();
            }

            if (output.Contains(delimeter) || output.Contains("\""))
                output = '"' + output.Replace("\"", "\"\"") + '"';

            output = output.Replace("\n", " ");
            output = output.Replace("\r", "");

            if (output.Length > 30000) //cropping value for stupid Excel
            {
                if (output.EndsWith("\""))
                    output = output.Substring(0, 30000) + "\"";
                else
                    output = output.Substring(0, 30000);
            }

            return output.Length <= 32767 ? output : output.Substring(0, 32767);
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
