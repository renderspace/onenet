using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Activation;
using System.Runtime.Serialization;
using One.Net.BLL.DAL;
using Newtonsoft.Json;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;
using One.Net.BLL.Scaffold;
using System.Data;

namespace One.Net.BLL.Service
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ScaffoldService : IScaffoldService
    {
        public string Ping()
        {
            return "ScaffoldService";
        }

        public List<SerializableJsonDictionary<string, string>> ListItemsForRelation(int relationId, int primaryKey)
        {
            var items = Data.ListItemsForRelation(relationId, primaryKey);
            if (items == null)
                return null;

            var tempTable = new DataTable();
            var i = 0;
            foreach (DataColumn col in items.Columns)
            {
                tempTable.Columns.Add(new DataColumn { ColumnName = col.ExtendedProperties["PK"] != null ? "PK" : ("C" + i++) });
            }

            DataRow row = tempTable.NewRow();
            i = 0;
            foreach (DataColumn col in items.Columns)
            {
                row[col.ExtendedProperties["PK"] != null ? "PK" : ("C" + i++)] = col.ColumnName;
            }
            tempTable.Rows.Add(row);

            foreach (DataRow item in items.Rows)
            {
                row = tempTable.NewRow();
                i = 0;
                foreach (DataColumn col in items.Columns)
                {
                    row[col.ExtendedProperties["PK"] != null ? "PK" : ("C" + i++)] = item[col.ColumnName];
                }
                tempTable.Rows.Add(row);
            }

            return ConvertDataTableToDictionary(tempTable);
        }

        private List<SerializableJsonDictionary<string, string>> ConvertDataTableToDictionary(DataTable dtIn)
        {
            var hashList = new List<SerializableJsonDictionary<string, string>>();
            foreach (DataRow drIn in dtIn.Rows)
            {
                var ht = new SerializableJsonDictionary<string, string>();
                foreach (DataColumn dc in dtIn.Columns)
                {
                    ht.Add(dc.ColumnName, drIn[dc.Ordinal].ToString());
                }
                hashList.Add(ht);
            }
            return hashList;
        }

    }
}
