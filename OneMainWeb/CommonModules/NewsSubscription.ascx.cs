using System;
using System.Configuration;
using System.Net;

using One.Net.BLL;
using One.Net.BLL.Web;

using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;
using One.Net.BLL.Utility;

namespace OneMainWeb.CommonModules
{
    public partial class NewsSubscription : MModule
    {
        #region Variables

        private readonly Regex regex = new Regex(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3} \.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)");
        //private readonly Regex regex = new Regex(@"^(([\w\-\.]+@([A-Za-z0-9]([\w\-])+\.){1,2}([a-zA-Z]([\w\-]){1,3}));*)+");
        private BNewsLtr.NewsletterAction action = BNewsLtr.NewsletterAction.Default;
        private static readonly BNewsLtr newsletterB = new BNewsLtr();
        private string hash = "";
        private int subscriptionID = -1;

        #endregion Variables

        #region Properties

        protected BNewsLtr.NewsletterAction Action { get { return action; } set { action = value; } }
        protected string Hash { get { return hash; } set { hash = value; } }
        protected int SubscriptionId { get { return subscriptionID; } set { subscriptionID = value; } }

        #endregion Properties

        #region Settings

        protected string ConfirmationPage { get { return GetStringSetting("ConfirmationPage"); } }
        protected string CancelationPage { get { return GetStringSetting("CancelationPage"); } }
        protected string SubscriptionEmailSubject { get { return GetStringSetting("SubscriptionEmailSubject"); } }
        protected List<int> NewsletterIds { get { return StringTool.SplitStringToIntegers(GetStringSetting("NewsletterId")); } }

        protected string SubscriptionEmailBody { get { return GetStringSetting("SubscriptionEmailBody"); } }

        

        #endregion Settings

        private void HideAllControls()
        {
            PanelEmail.Visible = false;

            PlaceHolderSubscribe.Visible = false;
            PlaceHolderStatus.Visible = false;
            PNoNewsletterChecked.Visible = false;
            PEmailExists.Visible = false;
            PSendingFailed.Visible = false;
            PEmailRequired.Visible = false;
            PConfirmationEmailSent.Visible = false;
            PConfirmText.Visible = false;
            PUnsubscribeText.Visible = false;
            PEmailConfirmed.Visible = false;
            PEmailConfirmationFailed.Visible = false;
            PUserUnsubscribed.Visible = false;
            PErrorSubscriptionAlreadyCancelled.Visible = false;

            PEmailInvalid.Visible = false;
            RepeaterResults.Visible = false;
            PSubscribeText.Visible = false;
        }

        protected void RepeaterResults_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                SubResult item = (SubResult) e.Item.DataItem;
                //HtmlGenericControl liConfRequired = e.Item.FindControl("liConfRequired") as HtmlGenericControl;
                HtmlGenericControl liExists = e.Item.FindControl("liExists") as HtmlGenericControl;
                HtmlGenericControl liFailed = e.Item.FindControl("liFailed") as HtmlGenericControl;
                HtmlGenericControl liSent = e.Item.FindControl("liSent") as HtmlGenericControl;

                //if (liExists != null && liFailed != null && liSent != null)
                //{
                    liExists.Visible = liFailed.Visible = liSent.Visible = false;
                    switch (item.Result)
                    {
                        case NewsLtrSubRes.Failed:
                            liFailed.Visible = true;
                            break;
                        case NewsLtrSubRes.Success:
                            liSent.Visible = true;
                            break;
                        case NewsLtrSubRes.AlreadyExists:
                            liExists.Visible = true;
                            break;
                    }
                //}
            }
        }

        private void ProcessQueryAction()
        {
            switch (Action)
            {
                case BNewsLtr.NewsletterAction.ConfirmSubscription:
                    {
                        NewsLtrSubRes result = newsletterB.ConfirmSubscription(Hash, IPAddress.Parse(Request.UserHostAddress), SubscriptionId);

                        HideAllControls();
                        PlaceHolderStatus.Visible = true;

                        switch (result)
                        {
                            case NewsLtrSubRes.Success: { PEmailConfirmed.Visible = true; break; }
                            default: { PEmailConfirmationFailed.Visible = true; break; }
                        }
                    }
                    break;
                case BNewsLtr.NewsletterAction.CancelSubscriptionConfirmed:
                    {
                        NewsLtrSubRes result = newsletterB.Unsubscribe(SubscriptionId, Hash);

                        HideAllControls();
                        PlaceHolderStatus.Visible = true;
                        
                        switch (result)
                        {
                            case NewsLtrSubRes.Success: { PUserUnsubscribed.Visible = true; break; }
                            default: { PErrorSubscriptionAlreadyCancelled.Visible = true; break; }
                        }
                        break;
                    }
                default:
                    {
                        PlaceHolderSubscribe.Visible = true;
                        PanelEmail.Visible = true;
                        PSubscribeText.Visible = true;
                        break;
                    }
            }
        }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (NewsletterIds.Count == 0)
                throw new Exception("missing_newsletterids_setting");

            LabelEmail.Text = Translate("enter_email");

            if (!IsPostBack)
            {
                CmdSubscribe.Text = Translate(CmdSubscribe.Text);
                InputEmail.Attributes.Add("placeholder",  Translate("enter_email"));
                HideAllControls();
                DisplaySubscribe();

                Action = BNewsLtr.NewsletterAction.Default;
                if (Request[BNewsLtr.REQUEST_ACTION] != null)
                {
                    int _action;

                    if (int.TryParse(Request[BNewsLtr.REQUEST_ACTION], out _action) && _action >= 1 && _action <= 4)
                        Action = (BNewsLtr.NewsletterAction) 
                            Enum.Parse(typeof(BNewsLtr.NewsletterAction), Request[BNewsLtr.REQUEST_ACTION]);

                    if (Request[BNewsLtr.REQUEST_HASH] != null)
                        Hash = Request[BNewsLtr.REQUEST_HASH];

                    if (Request[BNewsLtr.REQUEST_SUBSCRIPTION_ID] != null)
                        SubscriptionId = FormatTool.GetInteger(Request[BNewsLtr.REQUEST_SUBSCRIPTION_ID]);
                }

                ProcessQueryAction();
            }
        }

        private void DisplaySubscribe()
        {
            if (NewsletterIds.Count > 1)
            {
                CheckBoxListNewsletters.Visible = true;
                CheckBoxListNewsletters.DataSource = newsletterB.ListNewsletters(NewsletterIds);
                CheckBoxListNewsletters.DataTextField = "Name";
                CheckBoxListNewsletters.DataValueField = "Id";
                CheckBoxListNewsletters.DataBind();
            }
            else if (NewsletterIds.Count == 1)
            {
                CheckBoxListNewsletters.Visible = false;
            }

            PlaceHolderSubscribe.Visible = true;
            PanelEmail.Visible = true;
        }

        public void CmdSubscribe_Click(object sender, EventArgs e)
        {
            HideAllControls();
            bool showSubscribeEmailPanel = true;
            if (string.IsNullOrEmpty(InputEmail.Text))
            {
                PEmailRequired.Visible = true;
            }
            else if (!regex.IsMatch(InputEmail.Text))
            {   
                PEmailInvalid.Visible = true;
            }
            else
            {
                List<SubResult> subResults = new List<SubResult>();

                if (NewsletterIds.Count == 1)
                {
                    NewsLtrSubRes result = newsletterB.Subscribe(InputEmail.Text, NewsletterIds[0],
                                              new Uri(BuildUriString(ConfirmationPage, this.Page)),
                                              SubscriptionEmailSubject, SubscriptionEmailBody);

                    switch (result)
                    {
                        case NewsLtrSubRes.AlreadyExists:
                            {
                                PEmailExists.Visible = true;
                                break;
                            }
                        case NewsLtrSubRes.Failed:
                            {
                                PSendingFailed.Visible = true;
                                break;
                            }
                        case NewsLtrSubRes.Success:
                            {
                                PConfirmationEmailSent.Visible = true;
                                showSubscribeEmailPanel = false;
                                break;
                            }
                    }
                }
                else
                {
                    Dictionary<int, string> selectedNewsletterIds = new Dictionary<int, string>();
                    foreach (ListItem item in CheckBoxListNewsletters.Items)
                    {
                        if (item.Selected)
                            selectedNewsletterIds.Add(FormatTool.GetInteger(item.Value), item.Text);
                    }

                    foreach (int id in selectedNewsletterIds.Keys)
                    {
                        NewsLtrSubRes result =
                            newsletterB.Subscribe(InputEmail.Text, id,
                                                  new Uri(BuildUriString(ConfirmationPage, this.Page)),
                                                  SubscriptionEmailSubject, SubscriptionEmailBody);
                        
                        SubResult subRes = new SubResult();
                        subRes.Result = result;
                        subRes.NewsletterId = id;
                        subRes.NewsletterName = selectedNewsletterIds[id];
                        subResults.Add(subRes);
                    }

                    if (subResults.Count == 0)
                    {
                        PNoNewsletterChecked.Visible = true;
                    }
                    else
                    {
                        RepeaterResults.Visible = true;
                        RepeaterResults.DataSource = subResults;
                        RepeaterResults.DataBind();
                        showSubscribeEmailPanel = false;
                    }
                }
            }

            DisplaySubscribe();
            PanelEmail.Visible = showSubscribeEmailPanel;
        }

        private static string BuildUriString(string settingValue, System.Web.UI.Page page)
        {
            UrlBuilder builder = new UrlBuilder(page);
            if (!string.IsNullOrEmpty(settingValue))
            {
                #warning TODO We need some other form of URL handling
                if (settingValue.StartsWith("http"))
                    builder = new UrlBuilder(settingValue);
                else
                    builder.Path = settingValue;
            }
            return builder.ToString();
        }
    }

    public class SubResult
    {
        private NewsLtrSubRes result;
        private int newsletterId;
        private string newsletterName;

        public NewsLtrSubRes Result { get { return result; } set { result = value; } }
        public int NewsletterId { get { return newsletterId; } set { newsletterId = value;} } 
        public string NewsletterName { get { return newsletterName; } set { newsletterName = value; } }
    }
}