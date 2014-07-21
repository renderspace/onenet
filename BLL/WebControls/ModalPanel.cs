using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace One.Net.BLL.WebControls
{
    /// <summary>
    /// Creates a floating modal panel
    /// </summary>
    [
    DefaultProperty("Text"),
    ToolboxData("<{0}:ModalPanel runat=server></{0}:ModalPanel>"),
    DefaultEvent("WindowClosed")
    ]
    public class ModalPanel : Panel
    {
        HtmlGenericControl mainDiv = new HtmlGenericControl("div");
        HtmlGenericControl innerDiv = new HtmlGenericControl("div");
        HtmlGenericControl titleBar = new HtmlGenericControl("div");
        Literal titleDiv = new Literal();
        Image img = new Image();
        ImageButton closeBtn = new ImageButton();

        #region Scripts

        private const string HANDLER_SCRIPT = @"
 	<script language='javascript' type='text/javascript'>
	<!--

			var ie=document.all;
			var ns6=document.getElementById && !document.all;
			var dragapproved = false;
			var z,x,y;
			
			if(ie)
			{
				document.attachEvent('onmousedown', drags);
				document.attachEvent('onmouseup', stop);
			}
			else
			{
				document.onmousedown = drags;
				document.onmouseup = stop;
			}
			
			function stop(e)
			{
				dragapproved = false;
				if(z)
				{
					if(findPosY(z)<0 || findPosX(z)<0)return;

					setCookie(z.id + '_TOP', z.style.top);
					setCookie(z.id + '_LEFT', z.style.left);
				}
			}
			
			function findPosX(obj)
			{
				var curleft = 0;
				if (obj.offsetParent)
				{
					while (obj.offsetParent)
					{
						curleft += obj.offsetLeft
						obj = obj.offsetParent;
					}
				}
				else if (obj.x)
					curleft += obj.x;
				return curleft;
			}

			function findPosY(obj)
			{
				var curtop = 0;
				if (obj.offsetParent)
				{
					while (obj.offsetParent)
					{
						curtop += obj.offsetTop
						obj = obj.offsetParent;
					}
				}
				else if (obj.y)
					curtop += obj.y;
				
				return curtop;
			}
			
			function move(e)
			{
				if (dragapproved)
				{
					var eventX = (ns6 ? e.clientX : event.clientX);
					var eventY = (ns6 ? e.clientY : event.clientY);

					z.style.left = parseInt(z.style.left) + (eventX - x);
					z.style.top = parseInt(z.style.top) + (eventY - y);

					x = eventX;					
					y = eventY;

					return false
				}
			}

			function drags(e)
			{
	
				if (!ie && !ns6) return;
				
				var firedobj = ns6? e.target : event.srcElement;
				var topelement = ns6? 'HTML' : 'BODY';
				
				var firedobjName = ns6 ? firedobj.attributes['name'].value : firedobj.name;
				if(firedobjName != 'dragger') return;

				while (firedobj.tagName!=topelement && firedobj.className!='modalPanel'){
					firedobj=ns6? firedobj.parentNode : firedobj.parentElement;
				}

				if (firedobj.className=='modalPanel'){
					dragapproved=true

					z = firedobj
					x = (ns6 ? e.clientX: event.clientX);
					y = (ns6 ? e.clientY: event.clientY);

					document.onmousemove = move;
					return false;
				}
			}

			function positionDiv(elId)
			{
				var el = document.getElementById(elId);
				
				if(getCookie(el.id + '_TOP'))
				{
					el.style.top = getCookie(el.id + '_TOP');
					el.style.left = getCookie(el.id + '_LEFT');
				}				
				else
				{
					var thinkY = el.offsetTop;
					var realY = findPosY(el);
					var thinkX = el.offsetLeft;
					var realX = findPosX(el);

					var centerLeft  = ns6 ? (window.innerWidth - el.offsetWidth) /2 : (document.body.clientWidth - el.offsetWidth) / 2;
					var centerTop = ns6 ? (window.innerHeight - el.offsetHeight) /2 : (document.body.clientHeight- el.offsetHeight) / 2;
					
					el.style.top = thinkY + (centerTop - realY);
				    el.style.left = thinkX + (centerLeft - realX);
				}				
			}

			function getCookie (name) 
			{
				var dcookie = document.cookie; 
				var cname = name + '=';
				var clen = dcookie.length;
				var cbegin = 0;
				while (cbegin < clen) 
				{
					var vbegin = cbegin + cname.length;
					if (dcookie.substring(cbegin, vbegin) == cname) 
					{ 
						var vend = dcookie.indexOf (';', vbegin);
						if (vend == -1) vend = clen;
						return unescape(dcookie.substring(vbegin, vend));
					}
	        
					cbegin = dcookie.indexOf(' ', cbegin) + 1;
					if (cbegin == 0) break;
				}
				return null;
			}

			function setCookie (name, value) 
			{
				document.cookie = name + '=' + escape (value) + '; expires=Thu, 01-Jan-10 00:00:01 GMT; path=/';
			}

			//-->
		</script>";

        private const string INIT_SCRIPT = @"
		<script language='javascript' type='text/javascript'>
		    <!--
				positionDiv('{0}');
			//-->
		</script>";

        #endregion

        public event EventHandler WindowClosed;

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value></value>
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                titleDiv.Text = title;
            }
        }

        /// <summary>
        /// Gets or sets the title CSS class.
        /// </summary>
        /// <value></value>
        public string TitleCssClass
        {
            get { return titleCssClass; }
            set { titleCssClass = value; }
        }

        /// <summary>
        /// Gets or sets the window class.
        /// </summary>
        /// <value></value>
        public string WindowClass
        {
            get { return windowClass; }
            set { windowClass = value; }
        }

        /// <summary>
        /// Gets or sets the close button image URL.
        /// </summary>
        /// <value></value>
        public string CloseButtonImageUrl
        {
            get { return closeButtonImageUrl; }
            set { closeButtonImageUrl = value; }
        }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value></value>
        public string IconCssClass
        {
            get { return iconCssClass; }
            set { iconCssClass = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show close button].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [show close button]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowCloseButton
        {
            get { return showCloseBtn; }
            set { showCloseBtn = value; }
        }

        /// <summary>
        /// Gets or sets the icon URL.
        /// </summary>
        /// <value></value>
        public string IconUrl
        {
            get { return iconUrl; }
            set { iconUrl = value; }
        }

        private string title = "";
        private string titleCssClass = "modalPanelTitle";
        private string windowClass = "modalPanel";
        private string closeButtonImageUrl = "toolbar_close.gif";
        private string iconUrl = "";
        private string iconCssClass = "titleIcon";

        /// <summary>
        /// Ons the init.
        /// </summary>
        /// <param name="e">E.</param>
        protected override void OnInit(EventArgs e)
        {
            this.CssClass = windowClass;

            mainDiv.ID = "mainDiv";
            innerDiv.ID = "innerDiv";
            innerDiv.Attributes.Add("class", "innerDiv");

            titleBar.ID = "titleBar";
            titleBar.Attributes.Add("class", titleCssClass);
            titleBar.Attributes.Add("name", "dragger");

            titleDiv.Text = title;
            titleDiv.ID = "titleDiv";
            titleBar.Controls.Add(titleDiv);

            if (iconUrl.Length > 0)
            {
                img.ID = "icon";
                img.CssClass = iconCssClass;
                img.ImageUrl = iconUrl;
                titleBar.Controls.Add(img);
            }

            if (showCloseBtn)
            {
                // Add the close button
                closeBtn.ID = "closeBtn";
                closeBtn.ImageUrl = closeButtonImageUrl;
                closeBtn.Click += new ImageClickEventHandler(btn_Click);
                closeBtn.ValidationGroup = "no-validation-group";
                titleBar.Controls.Add(closeBtn);
            }

            mainDiv.Controls.Add(innerDiv);
            innerDiv.Controls.Add(titleBar);

            this.Controls.AddAt(0, mainDiv);

            // Add the table
            //Table table = new Table();
            //table.CellSpacing = 0;
            //table.CellPadding = 0;
            //table.Width = new Unit("100%");

            //// Add the title Row
            //TableRow row = new TableRow();
            //row.CssClass = titleCssClass;

            //// Add th title cell
            //TableCell cell = new TableCell();
            //cell.Width = new Unit("100%");
            //cell.Attributes.Add("name", "dragger");

            //if (iconUrl.Length > 0)
            //{
            //    Image img = new Image();
            //    img.CssClass = iconCssClass;
            //    img.ImageAlign = ImageAlign.AbsMiddle;
            //    img.ImageUrl = iconUrl;
            //    cell.Controls.Add(img);
            //}

            //cell.Controls.Add(new LiteralControl(this.title));
            //row.Controls.Add(cell);

            //if (showCloseBtn)
            //{
            //    // Add the close button
            //    TableCell cellClose = new TableCell();
            //    ImageButton btn = new ImageButton();
            //    btn.ImageUrl = closeButtonImageUrl;
            //    btn.Click += new ImageClickEventHandler(btn_Click);
            //    cellClose.Controls.Add(btn);
            //    row.Controls.Add(cellClose);
            //}

            //table.Controls.Add(row);
            //this.Controls.AddAt(0, table);

            if (this.ID == null) this.ID = this.UniqueID;
            this.Controls.AddAt(0, new LiteralControl(String.Format(INIT_SCRIPT, this.ClientID)));

            base.OnInit(e);

        }

        /// <summary>
        /// Ons the pre render.
        /// </summary>
        /// <param name="e">E.</param>
        protected override void OnPreRender(EventArgs e)
        {
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "DRAGSCRIPT", HANDLER_SCRIPT);
            base.OnPreRender(e);
        }

        private bool showCloseBtn = true;

        private void btn_Click(object sender, ImageClickEventArgs e)
        {
            if (this.WindowClosed != null)
                this.WindowClosed(this, new EventArgs());
        }
    }
}
