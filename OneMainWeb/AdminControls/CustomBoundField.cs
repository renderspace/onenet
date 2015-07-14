using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.UI;
using System.Web.UI.WebControls;

namespace OneMainWeb.AdminControls
{
    public class CustomBoundField : DataControlField
    {
        /// <summary>
        /// This property describe weather the column should be an editable column or non editable column.
        /// </summary>
        public bool Editable
        {
            get
            {
                var value = ViewState["Editable"];
                return value == null || Convert.ToBoolean(value);
            }
            set
            {
                ViewState["Editable"] = value;
                OnFieldChanged();
            }
        }

        /// <summary>
        /// This property is to describe weather to display a check box or not. 
        /// This property works in association with Editable.
        /// </summary>
        public bool ShowCheckBox
        {
            get
            {
                var value = base.ViewState["ShowCheckBox"];
                return value != null && Convert.ToBoolean(value);
            }
            set
            {
                ViewState["ShowCheckBox"] = value;
                OnFieldChanged();
            }
        }

        /// <summary>
        /// This property describe column name, which acts as the primary data source for the column. 
        /// The data that is displayed in the column will be retreived from the given column name.
        /// </summary>
        public string DataField
        {
            get
            {
                var value = base.ViewState["DataField"];
                return value != null ? value.ToString() : "";
            }
            set
            {
                ViewState["DataField"] = value;
                OnFieldChanged();
            }
        }

        /// <summary>
        /// Overriding the CreateField method is mandatory if you derive from the DataControlField.
        /// </summary>
        /// <returns></returns>
        protected override DataControlField CreateField()
        {
            return new BoundField();
        }

        /// <summary>
        /// Adds text controls to a cell's controls collection. Base method of DataControlField is
        /// called to import much of the logic that deals with header and footer rendering.
        /// </summary>
        /// <param name="cell">A reference to the cell</param>
        /// <param name="cellType">The type of the cell</param>
        /// <param name="rowState">State of the row being rendered</param>
        /// <param name="rowIndex">Index of the row being rendered</param>
        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
        {
            //Call the base method.
            base.InitializeCell(cell, cellType, rowState, rowIndex);

            switch (cellType)
            {
                case DataControlCellType.DataCell:
                    this.InitializeDataCell(cell, rowState);
                    break;
                case DataControlCellType.Footer:
                    this.InitializeFooterCell(cell, rowState);
                    break;
                case DataControlCellType.Header:
                    this.InitializeHeaderCell(cell, rowState);
                    break;
            }
        }

        /// <summary> 
        /// Determines which control to bind to data. In this a hyperlink control is bound regardless
        /// of the row state. The hyperlink control is then attached to a DataBinding event handler
        /// to actually retrieve and display data.
        /// 
        /// Note: This control was built with the assumption that it will not be used in a gridview
        /// control that uses inline editing. If you are building a custom data control field and 
        /// using this code for reference purposes key in mind that if your control needs to support
        /// inline editing you must determine which control to bind to data based on the row state.
        /// </summary>
        /// <param name="cell">A reference to the cell</param>
        /// <param name="rowState">State of the row being rendered</param>
        protected void InitializeDataCell(DataControlFieldCell cell, DataControlRowState rowState)
        {
            //Check to see if the column is a editable and does not show the checkboxes.
            if (Editable & !ShowCheckBox)
            {
                string ID = Guid.NewGuid().ToString();
                TextBox txtBox = new TextBox();
                txtBox.Columns = 5;
                txtBox.ID = ID;
                txtBox.DataBinding += new EventHandler(txtBox_DataBinding);

                cell.Controls.Add(txtBox);
            }
            else
            {
                if (ShowCheckBox)
                {
                    var chkBox = new CheckBox();
                    cell.Controls.Add(chkBox);
                }
                else
                {
                    var lblText = new Label();
                    lblText.DataBinding += new EventHandler(lblText_DataBinding);
                    cell.Controls.Add(lblText);
                }
            }
        }

        void lblText_DataBinding(object sender, EventArgs e)
        {
            // get a reference to the control that raised the event
            var target = (Label)sender;
            var container = target.NamingContainer;

            // get a reference to the row object
            var dataItem = DataBinder.GetDataItem(container);

            // get the row's value for the named data field only use Eval when it is neccessary
            // to access child object values, otherwise use GetPropertyValue. GetPropertyValue
            // is faster because it does not use reflection
            object dataFieldValue = null;

            dataFieldValue = this.DataField.Contains(".") ? DataBinder.Eval(dataItem, this.DataField) :
                DataBinder.GetPropertyValue(dataItem, this.DataField);

            // set the table cell's text. check for null values to prevent ToString errors
            if (dataFieldValue != null)
            {
                target.Text = dataFieldValue.ToString();
            }
        }

        protected void InitializeFooterCell(DataControlFieldCell cell, DataControlRowState rowState)
        {
            var chkBox = new CheckBox();
            cell.Controls.Add(chkBox);
        }

        protected void InitializeHeaderCell(DataControlFieldCell cell, DataControlRowState rowState)
        {
            var lbl = new Label();
            lbl.Text = this.DataField;
            cell.Controls.Add(lbl);
        }

        void txtBox_DataBinding(object sender, EventArgs e)
        {
            // get a reference to the control that raised the event
            var target = (TextBox)sender;
            var container = target.NamingContainer;

            // get a reference to the row object
            object dataItem = DataBinder.GetDataItem(container);

            // get the row's value for the named data field only use Eval when it is neccessary
            // to access child object values, otherwise use GetPropertyValue. GetPropertyValue
            // is faster because it does not use reflection
            object dataFieldValue = null;

            if (this.DataField.Contains("."))
            {
                dataFieldValue = DataBinder.Eval(dataItem, this.DataField);
            }
            else
            {
                dataFieldValue = DataBinder.GetPropertyValue(dataItem, this.DataField);
            }

            // set the table cell's text. check for null values to prevent ToString errors
            if (dataFieldValue != null)
            {
                target.Text = dataFieldValue.ToString();
            }
        }
    }
}