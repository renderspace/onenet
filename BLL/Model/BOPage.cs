using System;
using System.Collections.Generic;
using System.Threading;
using One.Net.BLL.Model;

namespace One.Net.BLL
{
	/// <summary>
	/// Summary description for BOPage.
	/// </summary>
    public class BOPage : PublishableInternalContent
	{
		BOTemplate template;
		Dictionary<string, BOSetting> pageSettings;
        Dictionary<int, BOPlaceHolder> placeHolders = new Dictionary<int, BOPlaceHolder>();
        string parLink;

	    public string parentPagesSimpleList = "";

	    private string frontEndRequireGroupList = "";
	    private string editRequireGroupList = "";

        public bool IsRoot
        {
            get { return !ParentId.HasValue; }
        }

		public int Id { get; set; }

        public int? ParentId { get; set; }

        public bool IsRedirected
        {
            get 
            {
                return (RedirectToUrl.StartsWith("http") || RedirectToUrl.StartsWith("/"));
            }
        }

        public bool RobotsIndex
        {
            get
            {
                return bool.Parse(Settings["RobotsIndex"].Value);
            }
        }

        public string ParentURI
        {
            get
            {
                var lastSlashPosition = URI.LastIndexOf('/');
                if (lastSlashPosition < 0)
                    return URI;
                return URI.Substring(0, lastSlashPosition + 1);

            }
        }

        public string OgImage
        {
            get
            {
                return SubTitle;
            }
        }

        public string URI { get; set; }

        public string RedirectToUrl { get; set; }

        public int WebSiteId { get; set; }

		public Dictionary<string, BOSetting> Settings
		{
			get
			{
				if (this.pageSettings == null)
					this.pageSettings = new Dictionary<string, BOSetting> ();
				return this.pageSettings;
			}
			set { pageSettings = value; }
		}

        public Dictionary<int, BOPlaceHolder> PlaceHolders
        {
            get { return placeHolders; }
            set { placeHolders = value; }
        }

		public BOTemplate Template
		{
			get {return this.template;}
			set { template = value; }
		}

        public int Order { get; set; }

        public int MenuGroup { get; set; }

        public bool BreakPersistance { get; set; }

        public int Level { get; set; }

        public string ParLink
        {
            get { return parLink; }
            set 
            {
                if (!value.Contains("/"))
                {
                    parLink = value;
                }
                else
                {
                    throw new ArgumentException("ParLink can't contain slashes");
                }
            }
        }

	    public bool IsEditabledByCurrentPrincipal
	    {
	        get
	        {
                if (EditRequireGroupList.Length == 0 || Thread.CurrentPrincipal.IsInRole("admin"))
                    return true;

	            string[] roles = EditRequireGroupList.Split(new char[] {',', ';'});
	            bool isEditable = false;
	            foreach (string s in roles)
	            {
	                isEditable = Thread.CurrentPrincipal.IsInRole(s);
                    if (isEditable)
                        break;
	            }
	            return isEditable;
	        }
	    }

        public bool IsViewableByCurrentPrincipal
        {
            get
            {
                if (FrontEndRequireGroupList.Length == 0)
                    return true;

                string[] roles = FrontEndRequireGroupList.Split(new char[] { ',', ';' });
                bool isViewable = false;

                foreach (string s in roles)
                {
                    isViewable = Thread.CurrentPrincipal.IsInRole(s);
                    if (isViewable)
                        break;
                }
                return isViewable;
            }
        }

	    public string FrontEndRequireGroupList
	    {
	        get { return frontEndRequireGroupList; }
	        set { frontEndRequireGroupList = value; }
	    }

	    public string EditRequireGroupList
	    {
	        get { return editRequireGroupList; }
	        set { editRequireGroupList = value; }
	    }

	    public bool RequireSSL { get; set; }

	    public override string ToString()
        {
            if (Id > 0)
            {
                return "BOPage (" + Id + ")";
            }
            return base.ToString();
        }

	}
}
