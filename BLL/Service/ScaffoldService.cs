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
using One.Net.BLL.Scaffold.Model;
using System.Collections;
using System.Collections.Specialized;

namespace One.Net.BLL.Service
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ScaffoldService : IScaffoldService
    {
        public string Ping()
        {
            return "ScaffoldService";
        }

        public DTOItem GetItem(int virtualTableId, int primaryKey)
        {
            var dataKeys = new OrderedDictionary();
            dataKeys.Add("id", primaryKey);
            DTOItem result = null;
            if (primaryKey == 0)
            {
                var virtualTable = Schema.GetVirtualTable(virtualTableId);
                result = new DTOItem { FriendlyName = virtualTable.FriendlyName };
                result.Columns = new List<DTOVirtualColumn>();

                foreach (var c in virtualTable.VirtualColumns)
                {
                    result.Columns.Add(new DTOVirtualColumn(c));
                }
            }
            else
            {
                var item = Data.GetItem(virtualTableId, dataKeys);
                if (item == null)
                    return null;
                result = new DTOItem(item);
            }
            return result;
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

        public bool ChangeItem(DTOItem item, int virtualTableId, int primaryKey)
        {
            var dataKeys = new OrderedDictionary();
            dataKeys.Add("id", primaryKey);

            var itemToSave = Data.GetItem(virtualTableId, dataKeys);
            if (itemToSave == null || item == null)
                return false;

            foreach (var c in item.Columns)
            {
                var field = itemToSave.Columns[c.FQName];
                if ("#" + field.InputClientId == c.InputId)
                {
                    switch (field.BackendType)
                    {
                        case FieldType.Integer:
                            field.ValueInteger = int.Parse(c.Value);
                            field.ValueLong = long.Parse(c.Value);
                            break;
                        case FieldType.Decimal:
                            field.ValueDecimal = decimal.Parse(c.Value);
                            field.ValueDouble = double.Parse(c.Value);
                            break;
                        case FieldType.SingleText:
                            field.ValueString = c.Value;
                            break;
                        case FieldType.Calendar:
                            if (!string.IsNullOrEmpty(c.Value))
                                field.ValueDateTime = DateTime.Parse(c.Value, CultureInfo.CurrentCulture);
                            else
                                field.ValueDateTime = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
                            break;
                        case FieldType.Checkbox:
                            var val = false;
                            if (bool.TryParse(c.Value, out val))
                            {
                                field.ValueBoolean = val;
                            }
                            break;
                        case FieldType.OneToMany:
                            field.ValueInteger = int.Parse(c.Value);
                            break;
                        case FieldType.ManyToMany:
                            var rawValue = c.Value.TrimEnd(',', ' ');
                            var values = rawValue.Split(',');
                            break;
                        default:
                            break;
                    }
                }
            }

            var dk = (IOrderedDictionary)dataKeys;
            var result = Data.ChangeItem(virtualTableId, itemToSave, ref dk);
            //BData.UpdateItemManyToManyFields(VirtualTableId, Item, primaryKeys);
            //InsertedId = (int)primaryKeys[0]
            //Item = BData.GetItem(VirtualTableId, PrimaryKeys);


            return result;
        }

        public List<KeyValuePair<string, string>> GetForeignKeyOptions(int virtualTableId, int columnId, int limit)
        {
#warning we need to load actual item because stupid GetVirtualTable doesn't read relationships.
            var dataKeys = new OrderedDictionary();
            dataKeys.Add("id", 0);
            var virtualTable = Data.GetItem(virtualTableId, dataKeys);
            var columns = virtualTable.Columns.Values.Where(vc => vc.Id == columnId);
            var column = columns.FirstOrDefault();
            var options = Data.GetForeignKeyOptions(column.PartOfRelationId, limit);
            var result = new List<KeyValuePair<string, string>>();
            foreach (var o in options)
            {
                result.Add(new KeyValuePair<string, string>(o.Key.ToString(), o.Value));
            }


            return result;
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

[DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
public class DTOVirtualTable
{
    public DTOVirtualTable()
    { }

    public DTOVirtualTable(VirtualTable virtualTable)
    {

        Id = virtualTable.Id;
        FriendlyName = virtualTable.FriendlyName;
        VirtualColumns = new List<DTOVirtualColumn>();
        Description = "Friendly table usage description... " + virtualTable.Description;
        if (virtualTable.VirtualColumns != null)
        {
            foreach (var vc in virtualTable.VirtualColumns)
            {
                VirtualColumns.Add(new DTOVirtualColumn(vc));
            }
        }
    }


    [DataMember(Order = 1), JsonProperty]
    public int Id { get; set; }

    [DataMember(Order = 2), JsonProperty]
    public string Description { get; set; }

    [DataMember(Order = 2), JsonProperty]
    public string FriendlyName { get; set; }

    public List<DTOVirtualColumn> VirtualColumns { get; set; }
}


[DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
public class DTOVirtualColumn
{
    public DTOVirtualColumn()
    { }

    public DTOVirtualColumn(VirtualColumn virtualColumn)
    {
        Id = virtualColumn.Id;
        FriendlyName = virtualColumn.FriendlyName;
        FQName = virtualColumn.FQName;
        Value = virtualColumn.Value;
        IsRequired = virtualColumn.IsNullable;
        BackendType = virtualColumn.BackendType.ToString();
        Hint = virtualColumn.Hint;
        InputId = "#" + virtualColumn.InputClientId;
    }

    [DataMember(Order = 1), JsonProperty]
    public int Id { get; set; }

    [DataMember(Order = 2), JsonProperty]
    public string FriendlyName { get; set; }
    public string FQName { get; set; }

    [DataMember(Order = 3), JsonProperty]
    public string Value { get; set; }

    [DataMember(Order = 4), JsonProperty]
    public string InputId { get; set; }

    [DataMember(Order = 5), JsonProperty]
    public string BackendType { get; set; }

    [DataMember(Order = 6), JsonProperty]
    public string Hint { get; set; }

    [DataMember(Order = 7), JsonProperty]
    public bool IsRequired { get; set; }
}

[Serializable]
[DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
public class DTOItem
{
    public DTOItem()
    { }

    [DataMember(Order = 1), JsonProperty]
    public string FriendlyName { get; set; }

    // public string StartingTable { get; set; }

    // public List<string> PrimaryKeys { get; set; }

    [DataMember(Order = 2), JsonProperty]
    public List<DTOVirtualColumn> Columns { get; set; }

    public DTOItem(EditableItem item)
    {
        FriendlyName = item.FriendlyName;

        Columns = new List<DTOVirtualColumn>();
        foreach (var c in item.Columns)
        {
            Columns.Add(new DTOVirtualColumn(c.Value));
        }
    }
}

[Serializable]
[DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
public class DataItem
{
    public string Key;
    public string Value;

    public DataItem()
    { }

    public DataItem(string key, string value)
    {
        Key = key;
        Value = value;
    }
}
