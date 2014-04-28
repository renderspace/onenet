using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security;

// only include security rules stuff if in 4.0
namespace One.Net.BLL.WebControls
{
	[ToolboxData("<{0}:TabularMultiView runat=\"server\"></{0}:TabularMultiView>")]
	[DisplayName("TabularMultiView")]
	[Description("A Multiview control that displays tabs for each selectable tab.")]
	[DefaultProperty("Views")]
	[ParseChildren(true, "Views")]

	public class TabularMultiView : CompositeControl
	{
		#region Properties

		// Internal controls
		private MultiView m_MultiView;
		private Menu m_Menu;

		private List<TabularView> _List;
		
		// Exposed events
		public delegate void SelectedMenuItemClickHandler(object sender, MenuEventArgs e);
		public event SelectedMenuItemClickHandler SelectedMenuItemClick;
		
		public delegate void ViewIndexChangedHandler(object sender, EventArgs e);
		public event ViewIndexChangedHandler ViewIndexChanged;
	    
		#endregion

		#region Control Life Cycle Events

		private void TabularMultiView_PreRender(object sender, System.EventArgs e)
		{
			if (InnerMultiView.ActiveViewIndex < 0)
			{
				if (InnerMultiView.Views.Count > 0)
				{
					throw new InvalidOperationException("No active tab set. Use method 'SetActiveIndex' to set the active tab on page load.");
				}
				else
				{
					InnerMultiView.Visible = true;
				}
			}
		}
		#endregion

		#region View Collection
		[Category("Data")]
		[Description("The TabularView collection")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[PersistenceMode(PersistenceMode.InnerDefaultProperty)]
		public List<TabularView> Views
		{
			get
			{
				if (_List == null)
				{
					_List = new List<TabularView>();
				}
				return _List;
			}
		}
		#endregion

		#region View - Active Index
		
		protected int ActiveViewIndex
		{
			get
			{
				object obj = ViewState["m_ActiveIndex"];
				if (obj == null)
				{
					return -1;
				}
				return (int)obj;
			}
			set
			{
				ViewState["m_ActiveIndex"] = value;
			}
		}
		#endregion

		#region Controls
		private MultiView InnerMultiView
		{
			get { return m_MultiView; }
			set { m_MultiView = value; }
		}

		private Menu InnerMenu
		{
			get { return m_Menu; }
			set { m_Menu = value; }
		}
		#endregion

		#region Composite - Events
		protected void Menu_SelectedMenuItemClick(object sender, MenuEventArgs e)
		{
			//InnerMultiView.ActiveViewIndex = int.Parse(e.Item.Value);
			int activeViewIndex = int.Parse(e.Item.Value);

			// Inform the user of item click event
			if((activeViewIndex == ActiveViewIndex || InnerMultiView.ActiveViewIndex == activeViewIndex) && SelectedMenuItemClick != null)
			{
				SelectedMenuItemClick(sender, e);
			}
			InnerMultiView.ActiveViewIndex = ActiveViewIndex = activeViewIndex;
		}

		protected void Mlt_ActiveIndexChanged(object sender, EventArgs e)
		{
			ViewIndexChanged(sender, e);
		}
		#endregion

		#region CreateChildControls
		protected override void CreateChildControls()
		{
			BuildMenu();
			BuildMultiView();
			
			// If Layout = LayoutOption.Flow Then
			Controls.Add(InnerMenu);
			Controls.Add(InnerMultiView);
		}
		#endregion

		#region Render
		protected void SetProperties()
		{
			for (int i=0; i<Views.Count; i++)
			{
				InnerMenu.Items[i].Selectable = Views[i].Selectable;
				InnerMenu.Items[i].Text = Views[i].TabName;
				if (ActiveViewIndex == i)
				{
					InnerMenu.Items[i].Selected = true;
					InnerMultiView.ActiveViewIndex = i;
				}
			}
		}

		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			EnsureChildControls();
			SetProperties();
			base.Render(writer);
		}
		#endregion

		#region RecreateChildControls
		protected override void RecreateChildControls()
		{
			EnsureChildControls();
		}
		#endregion

		#region Helper Methods

		#region Build Menu
		private void BuildMenu()
		{
			InnerMenu = new Menu();
			m_Menu.MenuItemClick += new MenuEventHandler(Menu_SelectedMenuItemClick);

			InnerMenu.ID = "tigerMenu";
			InnerMenu.Orientation = Orientation.Horizontal;
			
			for (int i=0; i<Views.Count; i++)
			{
				TabularView tabView = Views[i];
				if (string.IsNullOrEmpty(tabView.TabName))
				{
					tabView.TabName = "Tab " + i;
				}
				MenuItem mItem = new MenuItem((string.IsNullOrEmpty(tabView.TabName) ? tabView.ID : tabView.TabName), i.ToString());
				mItem.Selected = ActiveViewIndex.Equals(i);
				mItem.Selectable = tabView.Selectable;
				InnerMenu.Items.Add(mItem);
			}
		}
		#endregion

		#region Build MultiView
		private void BuildMultiView()
		{
			InnerMultiView = new MultiView();
			InnerMultiView.ID = "tigerMltView";
			foreach (TabularView tabView in Views)
			{
				InnerMultiView.Views.Add(tabView);
			}
			m_MultiView.ActiveViewChanged += new EventHandler(Mlt_ActiveIndexChanged);
		}
		#endregion

		#region Set Active Index
		public void SetActiveIndex(int activeIndex)
		{
			EnsureChildControls();
			if (InnerMultiView != null)
			{
				if (InnerMultiView.Views.Count >= 0 && InnerMultiView.Views.Count > activeIndex && activeIndex >= 0)
				{
					ActiveViewIndex = activeIndex;
					InnerMultiView.ActiveViewIndex = activeIndex;
					InnerMenu.Items[activeIndex].Selected = true;
				}
			}
		}
		#endregion
		#endregion
	}
}