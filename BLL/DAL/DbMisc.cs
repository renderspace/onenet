using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using MsSqlDBUtility;

namespace One.Net.BLL.DAL
{
    public class DbMisc
    {
		public static DataTable ExecuteArbitrarySQL(string connectionString, string sql)
        {
			DataTable dataTable;
            using (var reader = SqlHelper.ExecuteReader(connectionString, CommandType.Text, sql, new SqlParameter[0]))
            {
				dataTable = ReaderToTable(reader);
            }
			return dataTable;
        }
        
        private static DataTable ReaderToTable(IDataReader reader)
        {
			var schemaTable = reader.GetSchemaTable();
        	DataTable table = null;
			if (schemaTable != null)
			{
				table = new DataTable();
				var arrayList = new ArrayList();

				for (var i = 0;i < schemaTable.Rows.Count;i++)
				{
					var column = new DataColumn();
					if (!table.Columns.Contains(schemaTable.Rows[i]["ColumnName"].ToString()))
					{
						column.ColumnName = schemaTable.Rows[i]["ColumnName"].ToString();
						column.Unique = Convert.ToBoolean(schemaTable.Rows[i]["IsUnique"]);
						column.AllowDBNull = Convert.ToBoolean(schemaTable.Rows[i]["AllowDBNull"]);
						column.ReadOnly = Convert.ToBoolean(schemaTable.Rows[i]["IsReadOnly"]);
						arrayList.Add(column.ColumnName);
						table.Columns.Add(column);
					}
				}
				if (reader.Read())
				{
					do
					{
						var row = table.NewRow();
						for (var i = 0;i < arrayList.Count;i++)
						{
							row[((string) arrayList[i])] = reader[(string) arrayList[i]];
						}
						table.Rows.Add(row);
					} while (reader.Read());
				}
			}
			return table;
		}
    }
}
