using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MsSqlDBUtility;

namespace One.Net.BLL.Utility
{
    public class DatabaseHelper
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(DatabaseHelper));

        public static List<string> ListDatabases(string connectionString)
        {
            List<string> list = new List<string>();
            var sql = @"SELECT name
                        FROM [sys].[databases]
                        WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb')";

            using (var reader = SqlHelper.ExecuteReader(connectionString, CommandType.Text, sql))
            {
                while (reader.Read())
                {
                    list.Add((string)reader["name"]);
                }
            }

            return list;
        }

        public static string GetFileContentFromResource(string fileName)
        {
            string result = "";
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("One.Net.BLL." + fileName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }

        public static bool CheckDbCredentials(string connectionString)
        {
            var success = false;

            var connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();
                success = true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                connection.Close();
            }

            return success;
        }

        public static string BuildConnectionString(string serverName, string dbName, string dbUsername, string dbPassword)
        {
            var connBuilder = new SqlConnectionStringBuilder();

            connBuilder.UserID = dbUsername;
            connBuilder.Password = dbPassword;
            connBuilder.DataSource = serverName;
            connBuilder.InitialCatalog = dbName;
            connBuilder.Pooling = true;

            return connBuilder.ConnectionString;
        }

        public static bool RunSqlScript(string connectionString, string sql)
        {
            var success = false;
            try
            {
                SqlHelper.ExecuteNonQuery(connectionString, CommandType.Text, sql);
                success = true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return success;
        }
    }
}
