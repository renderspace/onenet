using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using One.Net.BLL;
using One.Net.BLL.Web;
using One.Net.BLL.Scaffold;

namespace OneMainWeb.AdminControls
{
    public partial class ScaffoldMainPagedList : System.Web.UI.UserControl
    {
        public const string REQUEST_VIRTUAL_TABLE_ID = "rvtid";

        public enum Action
        {
            None,
            Delete,
            Insert,
            Update
        } ;

        private SortDir GridViewSortDirection
        {
            get
            {
                if (ViewState["sortDirection"] == null)
                    ViewState["sortDirection"] = SortDir.Ascending;
                return (SortDir)ViewState["sortDirection"];
            }
            set { ViewState["sortDirection"] = value; }
        }

        protected string GridViewSortExpression
        {
            get
            {
                if (ViewState["sortField"] == null)
                    ViewState["sortField"] = "";
                return (string)ViewState["sortField"];
            }
            set { ViewState["sortField"] = value; }
        }

        protected int VirtualTableId
        {
            get
            {
                return (ViewState["VirtualTableId"] == null ? 0 : (int)ViewState["VirtualTableId"]);
            }
            set { ViewState["VirtualTableId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (PhysicalSchema.CheckDatabaseConfiguration())
                {
                    if (Request[REQUEST_VIRTUAL_TABLE_ID] != null)
                        VirtualTableId = FormatTool.GetInteger(Request[REQUEST_VIRTUAL_TABLE_ID]);

                    PostbackPager1.SelectedPage = 1;
                    PostbackPager1.RecordsPerPage = 20;
                    GridViewItemsDataBind();
                }
            }
        }
        
        private void GridViewItemsDataBind()
        {
            ButtonInsert.Visible = ButtonInsert.Enabled = ButtonExportToExcel.Visible =
                ButtonDeleteSelected.Visible = PostbackPager1.Visible = false;

            if (VirtualTableId > 0)
            {
                var virtualTable = Schema.GetVirtualTable(VirtualTableId);

                if (virtualTable != null)
                {
                    var state = new ListingState
                    {
                        RecordsPerPage = PostbackPager1.RecordsPerPage,
                        FirstRecordIndex = PostbackPager1.FirstRecordIndex,
                        SortDirection = GridViewSortDirection,
                        SortField = GridViewSortExpression
                    };

                    if (string.IsNullOrEmpty(GridViewSortExpression))
                    {
                        GridViewSortExpression = virtualTable.OrderColumn;
                        state.SortField = GridViewSortExpression;
                    }

                    if (virtualTable != null)
                    {
                        ButtonInsert.Visible = virtualTable.VirtualColumns.Count > 0;
                        ButtonInsert.Enabled = virtualTable.CanInsert;
                    }
                    else
                    {
                        ButtonInsert.Visible = false;
                    }

                    var items = Data.ListItems(VirtualTableId, state);

                    GridViewItems.Columns.Clear();
                    GridViewItems.DataSource = items;
                    GridViewItems.DataKeyNames = (string[])items.ExtendedProperties["DataKeyNames"];

                    var multiRowSelector = new CustomBoundField { HeaderText = "_select", ShowCheckBox = true };

                    if (GridViewItems.DataKeyNames.Count() > 0)
                    {
                        GridViewItems.Columns.Add(multiRowSelector);
                    }
                    else if (VirtualTableId > 0)
                    {
                        Notifier1.Warning = ResourceManager.GetString("NoPrimaryKeyDefined");
                    }

                    foreach (DataColumn col in items.Columns)
                    {
                        var field = new BoundField { DataField = col.ColumnName, HeaderText = col.Caption, SortExpression = col.ColumnName };

                        //if (col.ExtendedProperties["Editable"] != null)
                        //{
                        //    bfield.Editable = Convert.ToBoolean(col.ExtendedProperties["Editable"]);
                        //}
                        //if (col.ExtendedProperties["CheckBox"] != null)
                        //{
                        //    bfield.ShowCheckBox = Convert.ToBoolean(col.ExtendedProperties["CheckBox"]);
                        //}
                        if (bool.Parse(col.ExtendedProperties["ShowOnList"].ToString()))
                            GridViewItems.Columns.Add(field);
                    }

                    GridViewItems.Columns.Insert(1, new CommandField { ShowHeader = true, ShowSelectButton = true, HeaderText = "", SelectText = "Edit" });
                    GridViewItems.DataBind();
                    var allRecords = (int)items.ExtendedProperties["AllRecords"];
                    PostbackPager1.TotalRecords = allRecords;
                    PostbackPager1.DetermineData();
                    ButtonExportToExcel.Visible = allRecords > 0;
                    ButtonDeleteSelected.Visible = allRecords > 0;
                    PostbackPager1.Visible = allRecords > PostbackPager1.RecordsPerPage;
                    //if (items.ExtendedProperties.Contains("sql"))
                    //    Label1.Text = items.ExtendedProperties["sql"].ToString().Replace("\n", "<br />");

                }

                EmptyGridFix(GridViewItems);
            }
        }

        protected void GridViewItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (GridViewItems.DataKeys[GridViewItems.SelectedIndex] == null)
            {
                // nothing selected
                return;
            }

            if (GridViewItems.DataKeyNames.Count() > 0)
            {
                DynamicEditor1.Clear();
                DynamicEditor1.VirtualTableId = VirtualTableId;
                DynamicEditor1.PrimaryKeys = GridViewItems.DataKeys[GridViewItems.SelectedIndex].Values;
                MultiView1.ActiveViewIndex = 1;
            }
            else
            {
                throw new Exception("no PK defined");
            }
        }

        protected void GridViewItems_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (GridViewSortExpression == e.SortExpression)
            {
                GridViewSortDirection = SortDir.Ascending == GridViewSortDirection
                                            ? SortDir.Descending
                                            : SortDir.Ascending;
            }
            else
            {
                GridViewSortExpression = e.SortExpression;
                GridViewSortDirection = SortDir.Ascending;
            }
            GridViewItemsDataBind();
        }

        public void PostbackPager1_Command(object sender, CommandEventArgs e)
        {
            PostbackPager1.SelectedPage = Convert.ToInt32(e.CommandArgument);
            GridViewItemsDataBind();
        }

        protected void DynamicEditor1_Canceled(object sender, EventArgs e)
        {
            MultiView1.ActiveViewIndex = 0;
        }

        protected void DynamicEditor1_Saved(object sender, DynamicEditorEventArgs e)
        {
            if (e.IsInsert)
            {
                Notifier1.Message = "Item insert with identity " + e.InsertedId + ".";
            }
            else
            {
                Notifier1.Message = "Item updated";
            }
            GridViewItemsDataBind();
        }

        protected void ButtonExportToExcel_Click(object sender, EventArgs e)
        {
            Response.Redirect("/_ashx/BambooExcelExport.ashx?virtualTableId=" + VirtualTableId);
        }

        private void ProcessCustomBoundFieldsInGridViewItems(Action action)
        {
            var deletedCount = 0;
            var deleteFailedCount = 0;

            foreach (GridViewRow orow in GridViewItems.Rows)
            {
                if (GridViewItems != null && GridViewItems.DataKeys != null && GridViewItems.DataKeys[orow.DataItemIndex] != null)
                {
                    var primaryKeys = GridViewItems.DataKeys[orow.DataItemIndex].Values;

                    foreach (TableCell cell in orow.Cells)
                    {
                        if (cell.Controls.Count > 0)
                        {
                            var checkBox = cell.Controls[0] as CheckBox;

                            if (action == Action.Delete && checkBox != null && checkBox.Checked)
                            {
                                var result = Data.DeleteItem(VirtualTableId, primaryKeys);
                                deletedCount += result ? 1 : 0;
                                deleteFailedCount += result ? 0 : 1;
                            }
                        }
                    }
                }
            }

            if (deletedCount > 0)
            {
                GridViewItemsDataBind();
            }
            if (deleteFailedCount > 0)
            {
            }
            if (deletedCount == 0 && deleteFailedCount == 0)
            {
                Notifier1.Message = ResourceManager.GetString("NothingToDelete");
            }
            else
            {
                Notifier1.Message = "<ul><li>deletedCount:" + deletedCount + "</li><li>deleteFailedCount:" +
                                    deleteFailedCount + "</li></ul>";
            }
        }

        protected void ButtonInsert_Click(object sender, EventArgs e)
        {
            DynamicEditor1.Clear();
            DynamicEditor1.VirtualTableId = VirtualTableId;
            DynamicEditor1.PrimaryKeys = null;
            MultiView1.ActiveViewIndex = 1;
        }

        protected void ButtonDeleteSelected_Click(object sender, EventArgs e)
        {
            ProcessCustomBoundFieldsInGridViewItems(Action.Delete);
        }

        protected void EmptyGridFix(GridView grdView)
        {
            // normally executes after a grid load method
            if (grdView.Rows.Count == 0 && grdView.DataSource != null)
            {
                DataTable dt = null;
                // need to clone sources otherwise it will be indirectly adding to 
                // the original source
                if (grdView.DataSource is DataSet)
                    dt = ((DataSet)grdView.DataSource).Tables[0].Clone();
                else if (grdView.DataSource is DataTable)
                    dt = ((DataTable)grdView.DataSource).Clone();

                if (dt == null)
                    return;

                dt.Rows.Add(dt.NewRow()); // add empty row

                grdView.DataSource = dt;
                grdView.DataBind();

                // hide row
                grdView.Rows[0].Visible = false;
                grdView.Rows[0].Controls.Clear();
            }

            // normally executes at all postbacks

            if (grdView.Rows.Count == 1 && grdView.DataSource == null)
            {
                bool bIsGridEmpty = true;

                // check first row that all cells empty
                for (int i = 0; i < grdView.Rows[0].Cells.Count; i++)
                {
                    if (grdView.Rows[0].Cells[i].Text != string.Empty)
                    {
                        bIsGridEmpty = false;
                    }
                }
                // hide row
                if (bIsGridEmpty)
                {
                    grdView.Rows[0].Visible = false;
                    grdView.Rows[0].Controls.Clear();
                }
            }
        }


    }
}