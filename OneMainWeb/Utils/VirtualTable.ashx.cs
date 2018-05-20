using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using One.Net.BLL;
using System.Web.Script.Serialization;
using System.Data;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using One.Net.BLL.Scaffold;
using System.Threading;

namespace OneMainWeb.Utils
{
    /// <summary>
    /// Summary description for VirtualTable
    /// </summary>
    public class VirtualTable : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.QueryString["vtId"] != null)
            {
                var vtId = FormatTool.GetInteger(context.Request.QueryString["vtId"]);

                var lcid = Thread.CurrentThread.CurrentCulture.LCID;
                if (context.Request.QueryString["lcid"] != null)
                {
                    lcid = FormatTool.GetInteger(context.Request.QueryString["lcid"]);
                }
                if (lcid != Thread.CurrentThread.CurrentCulture.LCID)
                {
                    Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(lcid);
                }

                var p = FormatTool.GetInteger(context.Request.QueryString["p"]);
                p = p > 0 ? p : 1;

                var c = FormatTool.GetInteger(context.Request.QueryString["c"]);
                c = c > 0 ? c : 10000;

                var state = new ListingState
                {
                    RecordsPerPage = c,
                    FirstRecordIndex = (p - 1) * c
                };

                var table = Data.ListItems(vtId, state, null, true);

                if (table != null)
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        col.ColumnName = col.ColumnName.Replace("[dbo].", "").Replace("[", "").Replace("]", "").Replace('.', '_').Replace('-', '_');
                    }

                    var output = Serialize(table);
                    context.Response.ContentType = "application/json";
                    var origin = context.Request.Headers.Get("Origin");
                    if (origin != null)
                    {
                        var isLocal = origin.Contains("://localhost");
                        if (!string.IsNullOrWhiteSpace(origin))
                        {
                            if (isLocal)
                            {
                                context.Response.Headers.Add("Access-Control-Allow-Origin", origin);
                            }
                            else
                            {
                                BWebsite website = new BWebsite();
                                var websites = website.List();
                                var address = (from w in websites select w.PreviewUrl).Concat(from w in websites select w.ProductionUrl).ToList();
                                if (address.Contains(origin.Trim()))
                                {
                                    context.Response.Headers.Add("Access-Control-Allow-Origin", origin);
                                }
                            }
                        }
                    }
                    context.Response.Write(output);
                }
            }
        }

        private string Serialize(DataTable table)
        {
            Newtonsoft.Json.JsonSerializer json = new Newtonsoft.Json.JsonSerializer();

            json.NullValueHandling = NullValueHandling.Ignore;

            json.ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace;
            json.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
            json.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            json.Converters.Add(new DataTableConverter());

            StringWriter sw = new StringWriter();
            Newtonsoft.Json.JsonTextWriter writer = new JsonTextWriter(sw);
            writer.Formatting = Formatting.Indented;

            writer.QuoteChar = '"';

            var columns2Remove = new List<string>();

            foreach(DataColumn c in table.Columns)
            {
                if (!(bool) c.ExtendedProperties["ShowOnList"])
                {
                    columns2Remove.Add(c.ColumnName);
                }
            }

            foreach (var c in columns2Remove)
            {
                table.Columns.Remove(c);
            }   

            json.Serialize(writer, table);

            string output = sw.ToString();
            writer.Close();
            sw.Close();

            return output;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private static IEnumerable<DataRow> TableEnumerable(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                yield return row;
            }
        }
    }
    /*
    public static class MyExtensions
    {
        public static object RowsToDictionary(this DataTable table)
        {
            var columns = table.Columns.Cast<DataColumn>().ToArray();
            return table.Rows.Cast<DataRow>().Select(r => columns.ToDictionary(c => c.ColumnName, c => r[c]));
        }

        public static Dictionary<string, object> ToDictionary(this DataTable table)
        {
            return new Dictionary<string, object> { { table.TableName, table.RowsToDictionary() } };
        }

        public static string GetJSONString(DataTable table)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(table.ToDictionary());
        }
    }*/
}
