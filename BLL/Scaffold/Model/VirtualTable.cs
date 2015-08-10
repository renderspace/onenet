using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace One.Net.BLL.Scaffold.Model
{
    public class VirtualTable
    {
        public int Id { get; set; }
        public string StartingPhysicalTable { get; set; }
        public string OrderColumn { get; set; }

        private string condition = "";
        public string Condition
        {
            get { return condition.Trim().StartsWith("AND", StringComparison.InvariantCultureIgnoreCase) ? condition : ""; }
            set { condition = value; }
        }

        private string friendlyName = "";
        public string FriendlyName
        {
            get { return string.IsNullOrEmpty(friendlyName) ? Description : friendlyName; }
            set { friendlyName = value; }
        }

        public bool ShowOnMenu { get; set; }

        public bool HasPager { get; set; }

        public string Description
        {
            get
            {
                return StartingPhysicalTable.Replace("[dbo].", "").Trim(new[] { '[', ']' });
            }
        }

        public List<string> PrimaryKeys { get; set; }
        public string Groups { get; set; }

        public List<int> GroupValues
        {
            get
            {
                return StringTool.SplitStringToIntegers(Groups);
            }
        }

        public List<VirtualColumn> VirtualColumns { get; set; }
        public List<Relation> Relations { get; set; }

        // insert is only enabled if all non-nullable columns are included and all foreign keys are in a relation and we have identity on PK
        public bool CanInsert
        {
            get
            {
                if (VirtualColumns == null || Relations == null || VirtualColumns.Count == 0)
                    return false;

                var columns = VirtualColumns;

                // TODO: This doesn't work properly:
                var columnsMissingForInsert = from p in columns
                                              where p.IsRequiredOnInsert && !p.IsPartOfPrimaryKey && !p.IsPartOfUserView && !p.IsPartOfForeignKey
                                              select p;

                var relations = Relations;
                var relationsContainedInInsert = from r in relations
                                                 join c in columns on r.PrimaryKeySourceTableName equals
                                                     c.PrimaryKeyTableName
                                                 select r;

                // check if we have exactly one PK which is int and has identity
                if (PrimaryKeys == null || PrimaryKeys.Count() != 1)
                    return false;
                if (VirtualColumns.Where(c => c.IsPartOfPrimaryKey && c.IsIdentity).Count() != 1)
                    return false;

                return (columnsMissingForInsert.Count() == 0 &&
                        (relationsContainedInInsert.Count() == columns.Where(c => c.IsPartOfForeignKey).Count()));
            }
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(StartingPhysicalTable))
                return "VirtualTable " + StartingPhysicalTable;
            return base.ToString();
        }
    }
}
