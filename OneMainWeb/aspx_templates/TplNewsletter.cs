using System;
using System.Data;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using One.Net.BLL;

namespace OneMainWeb.aspx_templates
{
    public partial class TplNewsletter : System.Web.UI.Page
    {
        protected BNewsLtr nsB = new BNewsLtr();

        int subscriptionID = -1;
        int templateID = -1;
        int newsletterID = -1;
        bool validData = false;
        string testEmail, fromEmail, toEmail, hash = "";

        public int NewsletterID
        {
            get { return newsletterID; }
            set { newsletterID = value; }
        }

        public string TestEmail
        {
            get { return testEmail; }
            set { testEmail = value; }
        }

        public int TemplateID
        {
            get { return templateID; }
            set { templateID = value; }
        }

        public int SubscriptionID
        {
            get { return subscriptionID; }
            set { subscriptionID = value; }
        }

        public string Hash
        {
            get { return hash; }
            set { hash = value; }
        }

        public bool IsTest
        {
            get { return testEmail != null; }
        }

        public string FromEmail
        {
            get { return fromEmail; }
            set { fromEmail = value; }
        }

        public string ToEmail
        {
            get { return toEmail; }
            set { toEmail = value; }
        }

        public bool ValidData
        {
            get { return validData; }
            set { validData = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request["testEmail"] != null)
            {
                TestEmail = Request["testEmail"];
            }

            if (Request["subject"] != null)
            {
                Title = Request["subject"].ToString();
            }

            if (testEmail != null)
            {
                ValidData = true;
            }
            else
            {
                if (Request["subscriptionID"] != null)
                {
                    SubscriptionID = int.Parse(Request["subscriptionID"]);
                }

                if (Request["newsletterID"] != null)
                {
                    NewsletterID = int.Parse(Request["newsletterID"]);
                }

                if (Request["templateID"] != null)
                {
                    TemplateID = int.Parse(Request["templateID"]);
                }

                if (Request["hash"] != null)
                {
                    Hash = (string)Request["hash"];
                }

                if (!string.IsNullOrEmpty(Request["email"]))
                {
                    ToEmail = "<" + Request["email"].ToString() + "> " + Request["email"].ToString();
                }

                if (SubscriptionID > 0 && NewsletterID > 0 && TemplateID > 0 && Hash.Length == 10 && ToEmail.Length > 0)
                {
                    ValidData = true;
                }
            }

            if (ValidData && !string.IsNullOrEmpty(TestEmail))
            {
                ToEmail = TestEmail;
                CreateMetaTags();
            }
            else if (ValidData)
            {
                CreateMetaTags();
            }
            else
            {
                HtmlHead head = (HtmlHead)Page.Header;
                HtmlMeta hmError = new HtmlMeta();
                hmError.Name = "error";
                hmError.Content = "ERROR: ";
                head.Controls.Add(hmError);
            }
        }

        private void CreateMetaTags()
        {
            HtmlHead head = (HtmlHead)Page.Header;

            HtmlMeta hmTo = new HtmlMeta();
            hmTo.Name = "To";
            hmTo.Content = ToEmail;
            HtmlMeta hmInlineImages = new HtmlMeta();
            hmInlineImages.Name = "InlineImages";
            hmInlineImages.Content = "false";
            HtmlMeta hmTextVersion = new HtmlMeta();
            hmTextVersion.Name = "TextVersion";

            string textVersionPath = OContext.Current.MapPath(Path.GetDirectoryName(Request.CurrentExecutionFilePath)) + "/newsletter.txt";

            if (!File.Exists(textVersionPath))
                throw new ApplicationException("missing text version");

            using (FileStream txtFile = File.Open(textVersionPath, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    byte[] textVersion;
                    StreamReader sr = new StreamReader(txtFile, true);
                    string text = sr.ReadToEnd();
                    System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                    textVersion = encoding.GetBytes(text);
                    hmTextVersion.Content = Convert.ToBase64String(textVersion);
                }
                finally
                {
                    txtFile.Close();
                }
            }
            head.Title = Title;
            head.Controls.Add(hmTo);
            head.Controls.Add(hmInlineImages);
            head.Controls.Add(hmTextVersion);
        }
    }
}
