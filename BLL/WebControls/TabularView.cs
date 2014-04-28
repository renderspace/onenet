using System.Web.UI;
using System;
using System.Web;
using System.Security.Permissions;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Security;

namespace One.Net.BLL.WebControls
{
	[ToolboxData("<{0}:TabularView runat=\"server\"></{0}:TabularView>")]
	public class TabularView : View
	{
		#region Properties = Tab
		private string m_TabName = string.Empty;
		private bool m_Selectable = true;
		#endregion 

		#region Getters/Setters
			[Category("Behavior"), DefaultValue("Tab"), Description("Name of the tab."), NotifyParentProperty(true)]
			public string TabName
			{
				get
				{
					string str = ViewState["m_TabName"] as string;
					if (str == null)
					{
						str = string.Empty;
					}
					return str;
				}
				set
				{
					ViewState["m_TabName"] = value;
				}
			}

			[Category("Behavior"), DefaultValue(true), Description("Whether or not the tab is selectable."), NotifyParentProperty(true)]
			public bool Selectable
			{
				get
				{
					object obj = ViewState["m_Selectable"];
					if (obj == null)
					{
						return true;
					}
					return (bool)obj;
				}
				set
				{
					ViewState["m_Selectable"] = value;
					m_Selectable = value;
				}
			}
		#endregion 

		#region Render
			protected override void Render(System.Web.UI.HtmlTextWriter writer)
			{
				this.EnsureChildControls();
				base.Render(writer);
			}
		#endregion 
	}
}