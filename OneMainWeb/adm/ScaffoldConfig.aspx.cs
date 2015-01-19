using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using One.Net.BLL.Scaffold;
using One.Net.BLL.Scaffold.Model;
using One.Net.BLL.Web;

namespace OneMainWeb.adm
{
    public partial class ScaffoldConfig : OneBasePage
    {
        private int? SelectedVirtualTableId
        {
            get
            {
                return ViewState["SelectedVirtualTableId"] != null ? (int)ViewState["SelectedVirtualTableId"] : (int?)null;
            }
            set { ViewState["SelectedVirtualTableId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                GridViewSortExpression = "id";
                if (PhysicalSchema.CheckDatabaseConfiguration())
                {
                    MultiView1.ActiveViewIndex = 0;
                }
                else
                {
                    MultiView1.ActiveViewIndex = 2;
                }
            }
        }

        protected void MultiView1_ActiveViewChanged(object sender, EventArgs e)
        {
            switch (MultiView1.ActiveViewIndex)
            {
                case 0: 
                    DataBindMultiView1Controls();
                    break;
            }
        }

        private void DataBindMultiView1Controls()
        {
            var physicalTables = PhysicalSchema.ListPhysicalTables();

            var physicalTableNames = (from t in physicalTables select t.StartingPhysicalTable);

            DropDownListPhysical.DataSource = from t in physicalTables
                                              join ptn in physicalTableNames on t.StartingPhysicalTable equals ptn
                                              select t;
            DropDownListPhysical.DataBind();

            GridViewVirtualTablesDataBind();
        }

        private void DataBindMultiView2Controls()
        {
            var virtualTable = Schema.GetVirtualTable(SelectedVirtualTableId.Value);

            var physicalColumns =
                (from c in virtualTable.VirtualColumns where !c.IsPartOfUserView select c);

            ListBoxPhysicalColumns.DataSource = physicalColumns;
            ListBoxPhysicalColumns.DataBind();

            ListBoxOneToManyVirtualTables.DataSource = PhysicalSchema.ListPrimaryKeyCandidates(SelectedVirtualTableId.Value);
            ListBoxOneToManyVirtualTables.DataBind();

            ListBoxRelationshipDisplayColumn.Visible = false;

            GridViewItemsDataBind();
        }

        protected void ListBoxOneToManyVirtualTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ListBoxOneToManyVirtualTables.SelectedValue))
            {
                ListBoxRelationshipDisplayColumn.Visible = true;
                int primaryTableId = Int32.Parse(ListBoxOneToManyVirtualTables.SelectedValue);

                var virtualTable = Schema.GetVirtualTable(primaryTableId);
                var virtualColumns =
                    (from c in virtualTable.VirtualColumns where c.IsPartOfUserView select c);

                ListBoxRelationshipDisplayColumn.DataSource = virtualColumns;
                ListBoxRelationshipDisplayColumn.DataBind();
            }
        }

        private void GridViewItemsDataBind()
        {
            GridViewItems.DataSource = ListVirtualColumnsAndRelations(SelectedVirtualTableId.Value);
            GridViewItems.DataBind();
        }

        public static DataTable ListVirtualColumnsAndRelations(int virtualTableId)
        {
            var virtualTable = Schema.GetVirtualTable(virtualTableId);
            var table = (virtualTable == null) ? new DataTable() : new DataTable(virtualTable.StartingPhysicalTable);
            table.ExtendedProperties.Add("AllRecords", 0);
            if (virtualTable == null)
                return table;

            table.Columns.Add(new DataColumn { ColumnName = "Description", Caption = "Description", ReadOnly = false });
            table.Columns.Add(new DataColumn { ColumnName = "FriendlyName", Caption = "FriendlyName", ReadOnly = false });
            table.Columns.Add(new DataColumn { ColumnName = "DbType", Caption = "DbType", ReadOnly = false });
            table.Columns.Add(new DataColumn { ColumnName = "Id", Caption = "Id", ReadOnly = false });
            table.Columns.Add(new DataColumn { ColumnName = "ColumnType", Caption = "ColumnType", ReadOnly = false });
            table.Columns.Add(new DataColumn { ColumnName = "ShowOnList", Caption = "ShowOnList", ReadOnly = false });
            table.Columns.Add(new DataColumn { ColumnName = "IsWysiwyg", Caption = "Wysiwyg", ReadOnly = false });
            table.Columns.Add(new DataColumn { ColumnName = "IsMultiLanguageContent", Caption = "Multilanguage", ReadOnly = false });

            /*
            var dataKeyNames = new List<string>();
            dataKeyNames.Add("Description");
            dataKeyNames.Add("FriendlyName");
            dataKeyNames.Add("DbType");
            dataKeyNames.Add("Id");
            dataKeyNames.Add("ColumnType");*/

            foreach (var column in virtualTable.VirtualColumns.Where(v => v.IsPartOfUserView == true))
            {
                var row = table.NewRow();
                row["Description"] = column.Description;
                row["FriendlyName"] = column.FriendlyName;
                row["DbType"] = column.DbType;
                row["Id"] = column.Id;
                row["ColumnType"] = "virtual_column";
                row["ShowOnList"] = column.ShowOnList;
                row["IsWysiwyg"] = column.IsWysiwyg;
                row["IsMultiLanguageContent"] = column.IsMultiLanguageContent;
                table.Rows.Add(row);
            }

            foreach (var relation in virtualTable.Relations)
            {
                var row = table.NewRow();
                row["Description"] = relation.Description;
                row["FriendlyName"] = relation.Description;
                row["DbType"] = "";
                row["Id"] = relation.Id;
                row["ColumnType"] = "virtual_relation";
                row["ShowOnList"] = false;
                row["IsWysiwyg"] = false;
                row["IsMultiLanguageContent"] = false;
                table.Rows.Add(row);
            }

            //table.ExtendedProperties["DataKeyNames"] = dataKeyNames.ToArray();

            return table;
        }

        private void GridViewVirtualTablesDataBind()
        {
            GridViewVirtualTables.DataSource = Schema.ListVirtualTables();
            GridViewVirtualTables.DataBind();
        }

        protected void ButtonAdd_Click(object sender, EventArgs e)
        {
            if (DropDownListPhysical.SelectedItem != null)
            {
                var t = new VirtualTable
                {
                    StartingPhysicalTable = DropDownListPhysical.SelectedValue,
                    OrderColumn = ""
                };
                Schema.ChangeVirtualTable(t);
                GridViewVirtualTablesDataBind();
            }
        }

        protected void GridViewVirtualTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            var CmdSelect = GridViewVirtualTables.SelectedRow.FindControl("CmdSelect") as IButtonControl;
            if (CmdSelect != null)
            {
                SelectedVirtualTableId = Int32.Parse(CmdSelect.CommandArgument.ToString());
                DataBindMultiView2Controls();
                MultiView1.ActiveViewIndex = 1;
            }
        }

        protected void GridViewVirtualTables_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = 0;
            Int32.TryParse(e.CommandArgument.ToString(), out id);

            switch (e.CommandName.ToLower())
            {
                case "delete":
                    Schema.DeleteVirtualTable(id);
                    GridViewVirtualTablesDataBind();
                    break;
            }
        }

        protected void GridViewVirtualTables_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList DropDownListOrder = e.Row.FindControl("DropDownListOrder") as DropDownList;
                VirtualTable table = e.Row.DataItem as VirtualTable;
                if (DropDownListOrder != null && table != null)
                {
                    if (DropDownListOrder.Items.Count == 0)
                    {
                        table = Schema.GetVirtualTable(table.Id);
                        foreach (VirtualColumn column in table.VirtualColumns)
                            DropDownListOrder.Items.Add(new ListItem(column.FriendlyName, column.FQName));

                        DropDownListOrder.Items.Insert(0, new ListItem("none", ""));

                        if (!string.IsNullOrEmpty(table.OrderColumn))
                            DropDownListOrder.SelectedValue = table.OrderColumn;
                    }
                }
            }
        }

        protected void GridViewVirtualTables_RowDeleted(object sender, GridViewDeletedEventArgs e)
        { }

        protected void GridViewVirtualTables_Deleting(object sender, GridViewDeleteEventArgs e)
        { }

        protected void GridViewItems_RowDeleting(object sender, GridViewDeleteEventArgs e)
        { }

        protected void GridViewItems_RowDeleted(object sender, GridViewDeletedEventArgs e)
        { }

        protected void GridViewItems_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = 0;
            string raw = e.CommandArgument.ToString();
            int index1 = raw.IndexOf(',');
            string part1 = raw.Substring(0, index1);
            string part2 = raw.Substring(index1 + 1);
            Int32.TryParse(part1, out id);

            switch (e.CommandName.ToLower())
            {
                case "delete":
                    if (part2 == "virtual_column")
                    {
                        //BOVirtualColumn col = BSchema.GetVirtualColumn(id);
                        //if (col != null)
                        //{
                        Schema.RemoveVirtualColumn(id);
                        //}
                    }
                    else if (part2 == "virtual_relation")
                    {
                        Relation rel = Schema.GetRelation(id);
                        if (rel != null)
                        {
                            Schema.RemoveRelation(rel.PrimaryKeySourceTableId, rel.ForeignKeyTableId);
                        }
                    }
                    break;
            }

            DataBindMultiView2Controls();
        }

        protected void CmdSave_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in GridViewVirtualTables.Rows)
            {
                var CmdDelete = row.FindControl("CmdDelete") as LinkButton;
                var CheckBoxShowOnMenu = row.FindControl("CheckBoxShowOnMenu") as CheckBox;
                var TextBoxFriendlyName = row.FindControl("TextBoxFriendlyName") as TextBox;
                var TextBoxWhereCondition = row.FindControl("TextBoxWhereCondition") as TextBox;
                var DropDownListOrder = row.FindControl("DropDownListOrder") as DropDownList;

                if (CmdDelete != null && CheckBoxShowOnMenu != null && TextBoxFriendlyName != null && TextBoxWhereCondition != null && DropDownListOrder != null)
                {
                    if (string.IsNullOrEmpty(TextBoxWhereCondition.Text.Trim()) || TextBoxWhereCondition.Text.Trim().StartsWith("AND", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int id = Int32.Parse(CmdDelete.CommandArgument.ToString());
                        VirtualTable table = Schema.GetVirtualTable(id);

                        table.OrderColumn = DropDownListOrder.SelectedValue;
                        table.FriendlyName = TextBoxFriendlyName.Text;
                        table.Condition = TextBoxWhereCondition.Text;
                        table.ShowOnMenu = CheckBoxShowOnMenu.Checked;
                        Schema.ChangeVirtualTable(table);
                    }
                    else
                    {
                        Notifier1.Warning = "Warning: Failed to save - where condition must start with AND";
                    }
                }
            }
        }

        protected void CmdSaveColumnChanges_Click(object sender, EventArgs e)
        {
            VirtualTable table = Schema.GetVirtualTable(SelectedVirtualTableId.Value);

            foreach (GridViewRow row in GridViewItems.Rows)
            {
                var CmdDelete = row.FindControl("CmdDelete") as IButtonControl;
                var CheckBoxShowOnList = row.FindControl("CheckBoxShowOnList") as CheckBox;
                var CheckBoxWysiwyg = row.FindControl("CheckBoxWysiwyg") as CheckBox;
                var TextBoxFriendlyName = row.FindControl("TextBoxFriendlyName") as TextBox;
                var CheckBoxIsMultiLanguageContent = row.FindControl("CheckBoxIsMultiLanguageContent") as CheckBox;

                if (CmdDelete != null && CheckBoxShowOnList != null && TextBoxFriendlyName != null)
                {
                    var idRaw = CmdDelete.CommandArgument.ToString(); // the command argument is given in {id, type} form in this case
                    var indexOfComma = idRaw.IndexOf(',');
                    idRaw = idRaw.Replace(idRaw.Substring(indexOfComma), "");
                    int id = Int32.Parse(idRaw);
                    VirtualColumn virtualColumn = Schema.GetVirtualColumn(id, table);

                    if (virtualColumn != null)
                    {
                        virtualColumn.FriendlyName = TextBoxFriendlyName.Text;
                        virtualColumn.ShowOnList = CheckBoxShowOnList.Checked;
                        virtualColumn.IsWysiwyg = CheckBoxWysiwyg.Checked;
                        virtualColumn.IsMultiLanguageContent = CheckBoxIsMultiLanguageContent.Checked;
                        Schema.ChangeVirtualColumn(virtualColumn);
                    }
                }
            }
        }

        protected void CmdCancelColumnChanges_Click(object sender, EventArgs e)
        {
            MultiView1.ActiveViewIndex = 0;
        }

        protected void GridViewItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList DropDownListDbType = e.Row.FindControl("DropDownListDbType") as DropDownList;
                DataRow row = e.Row.DataItem as DataRow;

                if (DropDownListDbType != null && row != null)
                {
                    if (row["ColumnType"].ToString() == "virtual_column")
                    {
                        //DropDownListDbType.Items.Clear();
                        //DropDownListDbType.Items.Add(new ListItem("", ""));
                        //DropDownListDbType.Items.Add(new ListItem("", ""));
                        //DropDownListDbType.Items.Add(new ListItem("", ""));
                        //DropDownListDbType.Items.Add(new ListItem("", ""));
                        DropDownListDbType.Visible = false;
                    }
                    else
                    {
                        DropDownListDbType.Visible = false;
                    }
                }
            }
        }

        protected void CmdCancel_Click(object sender, EventArgs e)
        {
            GridViewVirtualTablesDataBind();
        }

        protected void CmdAddAllColumns_Click(object sender, EventArgs e)
        {
            foreach (ListItem item in ListBoxPhysicalColumns.Items)
            {
                string name = item.Value;
                var virtualTable = Schema.GetVirtualTable(SelectedVirtualTableId.Value);
                var virtualColumn = ((from p in PhysicalSchema.ListPhysicalColumns(virtualTable.Id) where p.Name == name select p)).First();

                var newVirtualColumn = new VirtualColumn();
                newVirtualColumn.Name = name;
                newVirtualColumn.VirtualTableId = SelectedVirtualTableId.Value;
                newVirtualColumn.DbType = virtualColumn.DbType;
                newVirtualColumn.ShowOnList = true;
                Schema.ChangeVirtualColumn(newVirtualColumn);
            }
            DataBindMultiView2Controls();
        }

        protected void CmdAddColumn_Click(object sender, EventArgs e)
        {
            if (ListBoxPhysicalColumns.SelectedIndex == -1)
            {
                Notifier1.Warning = "No physical column selected";
            }
            else
            {
                string name = ListBoxPhysicalColumns.SelectedValue;
                var virtualTable = Schema.GetVirtualTable(SelectedVirtualTableId.Value);
                var virtualColumn = ((from p in PhysicalSchema.ListPhysicalColumns(virtualTable.Id) where p.Name == name select p)).First();

                var newVirtualColumn = new VirtualColumn();
                newVirtualColumn.Name = name;
                newVirtualColumn.VirtualTableId = SelectedVirtualTableId.Value;
                newVirtualColumn.DbType = virtualColumn.DbType;
                newVirtualColumn.ShowOnList = true;
                Schema.ChangeVirtualColumn(newVirtualColumn);

                DataBindMultiView2Controls();
            }
        }

        protected void CmdAddRelationship_Click(object sender, EventArgs e)
        {
            if (ListBoxOneToManyVirtualTables.SelectedIndex == -1)
            {
                Notifier1.Warning = "No table selected for relationship";
            }
            else if (ListBoxRelationshipDisplayColumn.SelectedIndex == -1)
            {
                Notifier1.Warning = "No display column selected for relationship";
            }
            else
            {
                var primaryVirtualTableId = int.Parse(ListBoxOneToManyVirtualTables.SelectedValue);
                var displayColumn = ListBoxRelationshipDisplayColumn.SelectedValue;

                var foreignKeySourceTableId = SelectedVirtualTableId.Value;
                Schema.AddRelation(primaryVirtualTableId, foreignKeySourceTableId, displayColumn);
                DataBindMultiView2Controls();
            }
        }

        protected void ButtonCreateTables_Click(object sender, EventArgs e)
        {
            List<string> errors;

            PhysicalSchema.CreateConfiguration(out errors);

            if (errors.Count == 0)
            {
                Response.Redirect("ScaffoldConfig.aspx");
            }
            else
            {
                foreach (string error in errors)
                {
                    Response.Write(error);
                }
            }
        }
    }
}