using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Collections.Specialized;
using System.Configuration;

namespace One.Net.BLL.Scaffold.DAL
{
    internal class DbHelper
    {
        public static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["Bamboo"].ConnectionString; }
        }

        public static Type GetDbType(int dbTypeId)
        {
            switch (dbTypeId)
            {
                case -1:    // text
                    return typeof(string);
                case -7:    // bit
                    return typeof(bool);
                case -6:    // tinyint
                    return typeof(byte);
                case 3:     // decimal
                    return typeof(decimal);
                case 6:
                    return typeof(double);
                case 4:     // int
                    return typeof(int);
                case 11:     // int
                    return typeof(DateTime);
                default:
                    return typeof(string);
            }
        }

        public static string GetTableQualifier(string tableIdentifier)
        {
            if (string.IsNullOrEmpty(tableIdentifier))
                return tableIdentifier;

            var temp = tableIdentifier.Split('.');

            if (temp.Count() > 1)
            {
                return temp[1].Trim(new[] { '[', ']' });
            }
            else if (temp.Count() == 1)
            {
                return temp[0].Trim(new[] { '[', ']' });
            }
            return "";
        }

        public static SqlParameter[] PreparePrimaryKeyParameters(IOrderedDictionary dataKeys, ref string sqlPart)
        {
            int primaryKeyOrdinal = 0;
            var parameters = new SqlParameter[dataKeys.Keys.Count];

            foreach (var partOfPrimaryKey in dataKeys.Keys)
            {
                sqlPart += " AND " + partOfPrimaryKey + "=@PK" + primaryKeyOrdinal;
                parameters[primaryKeyOrdinal] = new SqlParameter("@PK" + primaryKeyOrdinal, dataKeys[partOfPrimaryKey]);
                primaryKeyOrdinal++;
            }
            return parameters;
        }
    }
}
