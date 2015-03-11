using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using NLog;
using One.Net.BLL.DAL;
using One.Net.BLL.Utility;
using System.Threading;
using System.Globalization;
using Microsoft.Web.Administration;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Data.SqlClient;
using MsSqlDBUtility;
using System.Data;
using System.Text;
using System.Xml;
using System.Configuration;

namespace One.Net.BLL
{
    public class BContentTemplate : BusinessBaseClass
    {
        readonly BWebsite webSiteB = new BWebsite();

        public BContentTemplate()
        {
            bool.TryParse(ConfigurationManager.AppSettings["PublishFlag"], out publishFlag);
        }

        public void ChangeContentTemplate(int moduleInstanceId, BOContentTemplate contentTemplate)
        {
            if (publishFlag)
            {
                throw new ApplicationException("invalid configuration: admin application should be using publish=false");
            }

            this.ChangeContentTemplate(contentTemplate);
            
            var instance = webSiteB.GetModuleInstance(moduleInstanceId);

            if (instance != null && instance.Settings != null && !instance.Settings.ContainsKey("ContentTemplateId"))
            {
                instance.Settings["ContentTemplateId"] = new BOSetting("ContentTemplateId", "Int", contentTemplate.Id.Value.ToString(), BOSetting.USER_VISIBILITY_SPECIAL);
                webSiteB.ChangeModuleInstance(instance);
            }
        }

        private void ChangeContentTemplate(BOContentTemplate contentTemplate)
        {
            var paramsToPass = new List<SqlParameter>();

            var idParameter = contentTemplate.Id.HasValue ? new SqlParameter("@Id", contentTemplate.Id) : new SqlParameter("@Id", DBNull.Value);
            
            paramsToPass.Add(idParameter);

            string sql;
            if (contentTemplate.Id.HasValue)
            {
                paramsToPass.Add(new SqlParameter("@DateModified", DateTime.Now));
                paramsToPass.Add(new SqlParameter("@PrincipalModified", Thread.CurrentPrincipal.Identity.Name));

                sql = @"UPDATE [dbo].[content_template] 
                        SET date_modified=@DateModified, 
                            principal_modified_by=@PrincipalModified 
                        WHERE id=@Id";
            }
            else
            {
                paramsToPass.Add(new SqlParameter("@DateCreated", DateTime.Now));
                paramsToPass.Add(new SqlParameter("@PrincipalCreated", Thread.CurrentPrincipal.Identity.Name));

                idParameter.Direction = ParameterDirection.InputOutput;
                idParameter.SqlDbType = SqlDbType.Int;

                sql = @"INSERT INTO [dbo].[content_template]  
                        (date_created, principal_created_by)
                        VALUES 
                        (@DateCreated, @PrincipalCreated); 
                        SET @Id=SCOPE_IDENTITY();";
            }

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass.ToArray());
            
            if (!contentTemplate.Id.HasValue)
                contentTemplate.Id = (int)idParameter.Value;

            sql = @"DELETE FROM  [dbo].[content_template_data] WHERE id=@ContentTemplateId ";

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, new SqlParameter("@ContentTemplateId", contentTemplate.Id.Value));

            foreach (var field in contentTemplate.ContentFields)
            {
                sql = @"INSERT INTO [dbo].[content_template_data]
                        (content_template_fk_id, field_name, field_content)
                        VALUES
                        (@ContentTemplateId, @FieldName, @FieldContent);";

                paramsToPass.Clear();

                paramsToPass.Add(new SqlParameter("@ContentTemplateId", contentTemplate.Id));
                paramsToPass.Add(new SqlParameter("@FieldName", field.Key));
                paramsToPass.Add(new SqlParameter("@FieldContent", field.Value));

                SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass.ToArray());
            }
        }

        public BOContentTemplate GetContentTemplate(int moduleInstanceId)
        {
            return this.GetContentTemplate(moduleInstanceId, publishFlag);
        }

        public BOContentTemplate GetContentTemplate(int moduleInstanceId, bool publishFlag)
        {
            BOContentTemplate answer = new BOContentTemplate();

            answer.ContentFields = new Dictionary<string, string>();

            var webSiteB = new BWebsite();
            var instance = webSiteB.GetModuleInstance(moduleInstanceId, publishFlag);

            if (instance != null && instance.Settings != null && instance.Settings.ContainsKey("ContentTemplateId"))
            {
                int contentTemplateId = int.Parse(instance.Settings["ContentTemplateId"].Value);
                if (contentTemplateId > 0)
                {
                    var sql = @"SELECT * FROM [dbo].[content_template] WHERE id=@ContentTemplateId";

                    using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, new SqlParameter("@ContentTemplateId", contentTemplateId)))
                    {
                        if (reader.Read())
                        {
                            answer.Id = contentTemplateId;
                            answer.DateCreated = (DateTime)reader["date_created"];
                            answer.DateModified = reader["date_modified"] != DBNull.Value ? (DateTime)reader["date_modified"] : (DateTime?)null;
                            answer.PrincipalCreated = (string)reader["principal_created_by"];
                            answer.PrincipalModified = reader["principal_modified_by"] != DBNull.Value ? (string)reader["principal_modified_by"] : "";
                        }
                    }

                    sql = @"SELECT * FROM [dbo].[content_template_data] WHERE content_template_fk_id=@ContentTemplateId";

                    using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, new SqlParameter("@ContentTemplateId", contentTemplateId)))
                    {
                        while (reader.Read())
                        {
                            answer.ContentFields.Add((string)reader["field_name"], (string)reader["field_content"]);
                        }
                    }
                }
            }

            return answer;
        }
    }
}
