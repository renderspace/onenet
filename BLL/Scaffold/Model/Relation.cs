using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace One.Net.BLL.Scaffold.Model
{
    public class Relation
    {
        public int Id { get; set; }

        public int ForeignKeyTableId { get; set; }
        public string ForeignKeyTableName { get; set; }

        public int PrimaryKeySourceTableId { get; set; }
        public string PrimaryKeySourceTableName { get; set; }
        public string PrimaryKeyName { get; set; }

        public int XrefPrimaryKeyTableId { get; set; }
        public string XrefPrimaryKeyTableName { get; set; }

        public bool IsMultiLanguageContent { get; set; }

        public bool IsReverse { get; set; }

        public bool IsManyToMany
        {
            get
            {
                return XrefPrimaryKeyTableId > 0 && !string.IsNullOrEmpty(XrefPrimaryKeyTableName);
            }
        }

        public string PrimaryKeyDisplayColumn { get; set; }

        public string FQPrimaryKeyDisplayColumn
        {
            get
            {
                if (!string.IsNullOrEmpty(PrimaryKeySourceTableName) && !string.IsNullOrEmpty(PrimaryKeyDisplayColumn))
                    return PrimaryKeySourceTableName + "." + PrimaryKeyDisplayColumn;
                else
                {
                    return "";
                }
            }
        }

        public string FQPrimaryKeyName
        {
            get { return PrimaryKeySourceTableName + "." + PrimaryKeyName; }
        }

        public string Description
        {
            get
            {
                if (!string.IsNullOrEmpty(ForeignKeyTableName) && !string.IsNullOrEmpty(PrimaryKeySourceTableName) && !string.IsNullOrEmpty(PrimaryKeyDisplayColumn))
                    return ForeignKeyTableName + "_n:1_" + PrimaryKeySourceTableName + " (" + PrimaryKeyDisplayColumn + ")";
                else
                    return "";
            }
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Description))
                return "Relation " + Description;
            return base.ToString();
        }
    }
}
