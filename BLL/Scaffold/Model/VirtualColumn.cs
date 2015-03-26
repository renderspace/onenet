using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SqlTypes;

namespace One.Net.BLL.Scaffold.Model
{
    public enum FieldType { SingleText, Display, OneToMany, ManyToMany, Checkbox, Calendar, Integer, MultiLanguageText, Decimal, CalendarWithTime, Time, ToMany };

    public enum DataType { Int, DateTime, Date, Time, Decimal, String, Boolean, Double, Binary };

    [Serializable]
    public class VirtualColumn
    {
         public int Id { get; set; }
        public int VirtualTableId { get; set; }
        public string VirtualTableName { get; set; }
        public string PrimaryKeyTableName { get; set; }
        public int PartOfRelationId { get; set; }

        public bool ShowOnList { get; set; }
        public bool IsWysiwyg { get; set; }

        
        
        public string Name { get; set; }



        private string friendlyName = "";
        public string FriendlyName
        {
            get
            {
                return string.IsNullOrEmpty(friendlyName) ? Name : friendlyName;
            }
            set { friendlyName = value; }
        }


        public DataType DbType { get; set; }
        public int Precision { get; set; }
        public string DefaultValue { get; set; }
        public bool IsNullable { get; set; }
        public bool HasDefaultValue { get { return !string.IsNullOrEmpty(DefaultValue); } }

        public bool IsRequiredOnInsert
        {
            get
            {
                return !(IsNullable || HasDefaultValue);
            }
        }


        public bool IsPartOfPrimaryKey { get; set; }
        public bool IsPartOfUserView { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsMultiLanguageContent { get; set; }

        public int Ordinal { get; set; }

        #region Editable Parts

        private FieldType backendType = FieldType.SingleText;
        public FieldType BackendType
        {
            get
            {
                if (backendType == FieldType.Display || backendType == FieldType.OneToMany || backendType == FieldType.ManyToMany || backendType == FieldType.MultiLanguageText || backendType == FieldType.ToMany)
                    return backendType;

                /*
                if (DbType == null)
                {
                    // this happens when field doesn't exist in given table at all
                    // e.g. when field is displaying n:m relation
                    return FieldType.ManyToMany;
                }*/

                switch (DbType)
                {
                    case DataType.Int: 
                        return FieldType.Integer;
                    case DataType.DateTime:
                        return FieldType.CalendarWithTime;
                    case DataType.Date:
                        return FieldType.Calendar;
                    case DataType.Time:
                        return FieldType.Time;
                    case DataType.Decimal:
                        return FieldType.Decimal;
                    case DataType.Double:
                        return FieldType.Decimal;
                    case DataType.String:
                        return backendType;
                    case DataType.Boolean:
                        return FieldType.Checkbox;
                    default:
                        return backendType;
                }
 
            }
            set { backendType = value; }
        }

        

        public string ValueString { get; set; }
        public int ValueInteger { get; set; }
        public List<int> ValueIntegerList { get; set; }
        public SqlDateTime ValueDateTime { get; set; }
        public TimeSpan ValueTime { get; set; }
        public decimal ValueDecimal { get; set; }
        public double ValueDouble { get; set; }
        public bool ValueBoolean { get; set; }
        public bool ValueIsNull { get; set; }

        public string Value
        {
            get
            {
                if (DbType == null)
                {
                    // this happens when field doesn't exist in given table at all
                    // e.g. when field is displaying n:m relation
                    if (ValueIntegerList == null)
                        return null;
                    if (ValueIntegerList.Count == 0)
                        return "";
                    var result = "";
                    foreach (var c in ValueIntegerList)
                    {
                        result += c + ",";
                    }
                    return result.Substring(0, result.Length - 1);
                }

                switch (DbType)
                {
                    case DataType.Int:
                        return ValueInteger.ToString();
                    case DataType.DateTime:
                    case DataType.Date:
                        return ValueDateTime.ToString();
                    case DataType.Time:
                        return ValueTime.ToString();
                    case DataType.String:
                        return ValueString;
                    case DataType.Decimal:
                        return ValueDecimal.ToString();
                    case DataType.Double:
                        return ValueDouble.ToString();
                    case DataType.Boolean:
                        return ValueBoolean.ToString();
                    default:
                        return ValueString;
                }
            }
        }

        public object NewValue
        {
            get
            {
                if (NewValueIsNull)
                {
                    return DBNull.Value;
                }
                if (DbType == null)
                {
                    return NewValueIntegerList;
                }
                if (IsMultiLanguageContent)
                {
                    return NewValueContent;
                }
                switch (DbType)
                {
                    case DataType.Int:
                        return NewValueInteger;
                    case DataType.Date:
                    case DataType.DateTime:
                        return NewValueDateTime;
                    case DataType.Time:
                        return NewValueTime;
                    case DataType.String:
                        return NewValueString;
                    case DataType.Decimal:
                        return NewValueDecimal;
                    case DataType.Boolean:
                        return NewValueBoolean;
                    case DataType.Double:
                        return NewValueDouble;
                    default:
                        return NewValueString;
                }
            }
        }

        public string NewValueString { get; set; }
        public int NewValueInteger { get; set; }
        public List<int> NewValueIntegerList { get; set; }
        public DateTime NewValueDateTime { get; set; }
        public TimeSpan NewValueTime { get; set; }
        public decimal NewValueDecimal { get; set; }
        public double NewValueDouble { get; set; }
        public bool NewValueBoolean { get; set; }
        public Dictionary<int, BOInternalContent> NewValueContent { get; set; }

        public bool NewValueIsNull { get; set; }

        #endregion


        public VirtualColumn()
        {
            Ordinal = -1;
        }

        public bool IsPartOfForeignKey
        {
            get { return !string.IsNullOrEmpty(PrimaryKeyTableName); }
        }

        public string Description 
        {
            get
            {
                if (DbType == null)
                    return "[" + Name + "]";
                return "[" + Name + "]" + " (" + DbType.ToString().Replace("System.", "") + (IsNullable ? " NULL" : "") + (IsPartOfPrimaryKey ? " PK" : "") + ")";
            }
        }

        public string FQName
        {
            get
            {
                if (!string.IsNullOrEmpty(VirtualTableName))
                {
                    var name = "";
                    if (!Name.StartsWith("["))
                    {
                        name = "[";
                    }
                    name += Name;
                    if (!Name.EndsWith("["))
                    {
                        name = name + "]";
                    }
                    return VirtualTableName + "." + name;
                }
                throw new InvalidDataException("either DbTable or DbColumn is not defined.");
            }
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Description))
                return Description;
            return base.ToString();
        }
    }
}
