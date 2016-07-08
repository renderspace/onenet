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
    public partial class ScaffoldInternationalize : System.Web.UI.Page
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
                if (PhysicalSchema.CheckDatabaseConfiguration())
                {
                    MultiView1.ActiveViewIndex = 0;
                }
                else
                {
                    MultiView1.ActiveViewIndex = 1;
                }
            }
        }

        protected void MultiView1_ActiveViewChanged(object sender, EventArgs e)
        {
            switch (MultiView1.ActiveViewIndex)
            {
                case 0:
                    DataBindMultiView0Controls();
                    break;
            }
        }

        private void DataBindMultiView0Controls()
        {
            var virtualTables = Schema.ListVirtualTables();

            DropDownListVirtual.DataSource = virtualTables;
            DropDownListVirtual.DataBind();
        }

        protected void ButtonSelect_Click(object sender, EventArgs e)
        {
            if (DropDownListVirtual.SelectedItem != null)
            {
                SelectedVirtualTableId = Int32.Parse(DropDownListVirtual.SelectedValue.ToString());
                GridViewItemsDataBind();
            }
        }
        
        private void GridViewItemsDataBind()
        {
            GridViewItems.DataSource = ListVirtualColumns(SelectedVirtualTableId.Value);
            GridViewItems.DataBind();
        }

        protected void GridViewItems_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = Int32.Parse(e.CommandArgument.ToString());

            if (id > 0)
            {
                var result = Schema.InternationalizeColumn(id);
                if (result)
                {
                    GridViewItemsDataBind();
                    Notifier1.Message = "Column successfully internationalized";
                }
                else 
                {
                    Notifier1.ExceptionMessage = "Unknown error occured while internationalizing.";
                }
                
            }
            else
            {
                Notifier1.ExceptionMessage= "No column selected";
            }
        }

        protected void GridViewItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var CmdInternationalize = (LinkButton)e.Row.FindControl("CmdInternationalize");
                DataRow row = ((DataRowView)e.Row.DataItem).Row;

                if (CmdInternationalize != null && row != null)
                {
                    CmdInternationalize.Visible = false;
                    if (row["DbType"].ToString().Contains("String"))
                    {
                        CmdInternationalize.Visible = true;
                    }
                }
            }
        }
        
        public static DataTable ListVirtualColumns(int virtualTableId)
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
            table.Columns.Add(new DataColumn { ColumnName = "EnableSearch", Caption = "EnableSearch", ReadOnly = false });
            table.Columns.Add(new DataColumn { ColumnName = "IsWysiwyg", Caption = "Wysiwyg", ReadOnly = false });
            table.Columns.Add(new DataColumn { ColumnName = "IsMultiLanguageContent", Caption = "Multilanguage", ReadOnly = false });

            foreach (var column in virtualTable.VirtualColumns.Where(v => v.IsPartOfUserView == true))
            {
                var row = table.NewRow();
                row["Description"] = column.Description;
                row["FriendlyName"] = column.FriendlyName;
                row["DbType"] = column.DbType;
                row["Id"] = column.Id;
                row["ColumnType"] = "virtual_column";
                row["ShowOnList"] = column.ShowOnList;
                row["EnableSearch"] = column.EnableSearch;
                row["IsWysiwyg"] = column.IsWysiwyg;
                row["IsMultiLanguageContent"] = column.IsMultiLanguageContent;
                table.Rows.Add(row);
            }
            
            return table;
        }
    }
}