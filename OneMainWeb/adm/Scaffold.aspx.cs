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
using System.Collections.Specialized;
using OneMainWeb.AdminControls;

namespace OneMainWeb.adm
{
    public partial class Scaffold : OneBasePage
    {
        public const string REQUEST_VIRTUAL_TABLE_ID = "rvtid";

        public enum Action
        {
            None,
            Delete,
            Insert,
            Update
        } ;

        protected int VirtualTableId
        {
            get
            {
                return (ViewState["VirtualTableId"] == null ? 0 : (int)ViewState["VirtualTableId"]);
            }
            set { ViewState["VirtualTableId"] = value; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (SelectedWebsite == null)
            {
                Notifier1.Warning = "You don't have permissions for any site or there are no websites defined in database.";
                return;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request[REQUEST_VIRTUAL_TABLE_ID] != null)
                    VirtualTableId = FormatTool.GetInteger(Request[REQUEST_VIRTUAL_TABLE_ID]);

                PostbackPager1.SelectedPage = 1;
                PostbackPager1.RecordsPerPage = GridViewPageSize;
                GridViewItemsDataBind();
                MultiView1.ActiveViewIndex = 0;
            }
        }

        private void GridViewItemsDataBind(string searchTerm = "")
        {
            ButtonInsert.Visible = ButtonInsert.Enabled = ButtonExportToExcel.Visible =
                ButtonDeleteSelected.Visible = PostbackPager1.Visible = false;

            if (VirtualTableId > 0)
            {
                var virtualTable = Schema.GetVirtualTable(VirtualTableId);

                if (virtualTable != null)
                {

                    var searchableFields = virtualTable.VirtualColumns.Where(vc => vc.EnableSearch);
                    var placholder = "Search by: ID, ";
                    foreach (var field in searchableFields)
                    {
                        placholder += field.Name + ", ";
                    }
                    TextBoxSearch.Attributes["placeholder"] = placholder.Substring(0, placholder.Length - 2);

                    var state = new ListingState
                    {
                        RecordsPerPage = virtualTable.HasPager ? PostbackPager1.RecordsPerPage : 10000,
                        FirstRecordIndex = virtualTable.HasPager ? PostbackPager1.FirstRecordIndex : 0,
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

                    var items = Data.ListItems(VirtualTableId, state, null, false, searchTerm);

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
                        Notifier1.Warning = "NoPrimaryKeyDefined";
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

                    var commandField = new CommandField { ShowHeader = true, ShowSelectButton = true, HeaderText = "", SelectText = "<span class=\"glyphicon glyphicon-pencil\"></span> Edit" };
                    commandField.ItemStyle.CssClass = "scaffold-edit-button";
                    GridViewItems.Columns.Add(commandField);
                    GridViewItems.DataBind();

                    var allRecords = (int)items.ExtendedProperties["AllRecords"];
                    ButtonExportToExcel.Visible = allRecords > 0;
                    ButtonDeleteSelected.Visible = allRecords > 0;
                    if (virtualTable.HasPager)
                    {
                        PostbackPager1.TotalRecords = allRecords;
                        PostbackPager1.DetermineData();
                        PostbackPager1.Visible = true;
                    }
                    PostbackPager1.Visible = virtualTable.HasPager && (allRecords > PostbackPager1.RecordsPerPage);
                    //if (items.ExtendedProperties.Contains("sql"))
                    //    Label1.Text = items.ExtendedProperties["sql"].ToString().Replace("\n", "<br />");

                }

                EmptyGridFix(GridViewItems);
            }
        }

        protected void GridViewItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (GridViewItems.DataKeys[GridViewItems.SelectedIndex] == null || GridViewItems.DataKeys[GridViewItems.SelectedIndex].Values.Count != 1)
            {
                // nothing selected
                return;
            }

            var id = 0;
            var primaryKey = "";

            var primaryKeys = (OrderedDictionary)GridViewItems.DataKeys[GridViewItems.SelectedIndex].Values;
            foreach (var partOfPrimaryKey in primaryKeys.Keys)
            {
                primaryKey = partOfPrimaryKey.ToString();
                id = int.Parse(primaryKeys[partOfPrimaryKey].ToString());
            }
            SelectItem(primaryKey, id);
        }

        protected void ButtonDisplayById_Click(object sender, EventArgs e)
        {
            var id = 0;
            int.TryParse(TextBoxSearch.Text.Trim(), out id);
            if (id < 1 || GridViewItems.DataKeys.Count < 1)
                return;

            var primaryKey = "";
            var primaryKeys = (OrderedDictionary)GridViewItems.DataKeys[0].Values;
            foreach (var partOfPrimaryKey in primaryKeys.Keys)
            {
                primaryKey = partOfPrimaryKey.ToString();
            }
            SelectItem(primaryKey, id);
        }

        protected void ButtonSearch_Click(object sender, EventArgs e)
        {
            var searchTerm = TextBoxSearch.Text.Trim();
            if (searchTerm.Length > 0)
            {
                GridViewItemsDataBind(searchTerm);
            }
        }

        protected void SelectItem(string primaryKeyName, int primaryKeyNameValue)
        {
            if (GridViewItems.DataKeyNames.Count() > 0)
            {
                var primaryKeys = new OrderedDictionary();
                primaryKeys.Add(primaryKeyName, primaryKeyNameValue);

                Notifier1.Warning = "";

                var item = Data.GetItem(VirtualTableId, primaryKeys);
                if (item == null || item.Columns == null || !item.Columns.ContainsKey(primaryKeyName) || (string.IsNullOrEmpty(item.Columns[primaryKeyName].Value) || item.Columns[primaryKeyName].Value == "0"))
                {
                    Notifier1.Warning = "NoItemWithIdExists";
                    return;
                }
                //else if (item.PrimaryKeys.Count == 1)
                //{
                //    if (string.IsNullOrEmpty(item.Columns[item.PrimaryKeys[0]].Value))
                //    {
                //        Notifier1.Warning = "NoItemWithIdExists";
                //        return;
                //    }
                //}

                DynamicEditor1.Clear();
                DynamicEditor1.VirtualTableId = VirtualTableId;
                DynamicEditor1.PrimaryKeys = primaryKeys;
                MultiView1.ActiveViewIndex = 1;
            }
            else
            {
                throw new Exception("no PK defined");
            }
        }

        protected void GridViewItems_Sorting(object sender, GridViewSortEventArgs e)
        {
            GridViewSorting(e);
            GridViewItemsDataBind();
        }

        public void PostbackPager1_Command(object sender, CommandEventArgs e)
        {
            PostbackPager1.SelectedPage = Convert.ToInt32(e.CommandArgument);
            GridViewItemsDataBind(TextBoxSearch.Text.Trim());
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

        protected void DynamicEditor1_Error(object sender, DynamicEditorEventArgs e)
        {
            Notifier1.Warning = "Can't save";
            foreach (var err in e.Errors)
            {
                Notifier1.Message += "<br>" + err;
            }
        }

        protected void ButtonExportToExcel_Click(object sender, EventArgs e)
        {
            Response.Redirect("/Utils/BambooExcelExport.ashx?virtualTableId=" + VirtualTableId);
        }

        protected void ButtonExportToCsv_Click(object sender, EventArgs e)
        {
            Response.Redirect("/Utils/BambooCsvExport.ashx?virtualTableId=" + VirtualTableId);
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
                Notifier1.Message = "NothingToDelete";
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
                    if (grdView.Rows[0].Cells[i].Text != "")
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

        
        /*
        protected void ButtonCreateTables_Click(object sender, EventArgs e)
        {
         * MultiView1.ActiveViewIndex = PhysicalSchema.CheckDatabaseConfiguration() ? 0 : 1;
            List<string> errors;

            PhysicalSchema.CreateConfiguration(out errors);

            if (true)
            {
                Response.Redirect("/adm/ScaffoldConfig.aspx");
            }
        }*/


    }
}