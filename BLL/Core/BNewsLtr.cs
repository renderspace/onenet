using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Transactions;

using One.Net.BLL.DAL;
using System.Text;


namespace One.Net.BLL
{
    public class BNewsLtr : BusinessBaseClass
    {
        public const string REQUEST_ACTION = "ac";
        public const string REQUEST_HASH = "h";
        public const string REQUEST_SUBSCRIPTION_ID = "sid";

        private static readonly DbNewsLtr newsletterDB = new DbNewsLtr();

        public enum NewsletterAction { Default = 1, ConfirmSubscription, CancelSubscription, CancelSubscriptionConfirmed};

        public BNewsLtr()
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["emailTemplatesFolder"]))
            {
                throw new ConfigurationErrorsException("Missing emailTemplatesFolder setting in web.config");
            }
        }

        public BONewsLtr GetNewsletter(int newsletterId)
        {
            return newsletterDB.GetNewsletter(newsletterId);
        }

        public BONewsLtrSub GetSubscription(int subscriptionId)
        {
            return newsletterDB.GetSubscription(subscriptionId, -1, null);
        }

        public NewsLtrSubRes SubscribeAndConfirm(string emailTo, int newsletterId, System.Net.IPAddress ip)
        {
            NewsLtrSubRes result = NewsLtrSubRes.Failed;

            BONewsLtrSub subscription = newsletterDB.GetSubscription(-1, newsletterId, emailTo);

            if (subscription.SubscriptionId > -1)
            {
                if (!subscription.DateUnsubscribed.HasValue && subscription.Confirmed)
                {
                    // do nothing, everything is fine
                    result = NewsLtrSubRes.AlreadyExists;
                }
                else if (!subscription.Confirmed && subscription.Subscribed)
                {
                    // simply confirm subscription
                    subscription.DateConfirmed = DateTime.Now;
                    subscription.IpConfirmed = ip;
                    newsletterDB.Change(subscription);
                    result = NewsLtrSubRes.Success;
                }
                else if (subscription.DateUnsubscribed.HasValue)
                {
                    // remove unsubscribed date and confirm subscription
                    subscription.DateUnsubscribed = null;
                    subscription.DateConfirmed = DateTime.Now;
                    subscription.IpConfirmed = ip;
                    newsletterDB.Change(subscription);
                    result = NewsLtrSubRes.Success;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(subscription.Hash))
                    subscription.Hash = StringTool.RandomString(10, false);

                // subscription is not subscribed
                // call newsLtrSubDb.Change method twice... 

                // once to subscribe
                newsletterDB.Change(subscription);
                subscription = newsletterDB.GetSubscription(subscription.SubscriptionId, subscription.NewsLetterId, subscription.Email);

                // and once to confirm
                subscription.DateConfirmed = DateTime.Now;
                subscription.IpConfirmed = ip;
                newsletterDB.Change(subscription);

                result = NewsLtrSubRes.Success;
            }
            return result;
        }

        public NewsLtrSubRes Subscribe(string emailTo, int newsletterId, Uri confirmationPage, string messageSubject)
        {
            int subscriptionId;
            return Subscribe(emailTo, newsletterId, confirmationPage, messageSubject, out subscriptionId);
        }

        public NewsLtrSubRes Subscribe(string emailTo, int newsletterId, Uri confirmationPage, string messageSubject,
            out int subscriptionId)
        {
            NewsLtrSubRes result = NewsLtrSubRes.Failed;

            BONewsLtrSub subscription = newsletterDB.GetSubscription(-1, newsletterId, emailTo);

            if (subscription.SubscriptionId > -1 && !subscription.DateUnsubscribed.HasValue && subscription.Confirmed)
            {
                result = NewsLtrSubRes.AlreadyExists;
            }
            else
            {
                if (!subscription.Confirmed && subscription.Subscribed)
                {}
                else if (subscription.DateUnsubscribed.HasValue)
                {
                    subscription.DateUnsubscribed = null;
                    subscription.DateConfirmed = null;
                    subscription.IpConfirmed = null;
                }
                else
                {
                    subscription.Hash = StringTool.RandomString(10, false);
                }

                if (string.IsNullOrEmpty(subscription.Hash)) 
                {
                    subscription.Hash = StringTool.RandomString(10, false);
                }


#if DISTRIBUTED_TRANSACTIONS // Doesn't work with high securtiy template
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
                {
#endif
                newsletterDB.Change(subscription);

                    if ( !subscription.Confirmed)
                    {
                        BONewsLtr newsletter = GetNewsletter(newsletterId);

                        newsletter.ConfirmationTemplate.TemplateSourceDirectory = ConfigurationManager.AppSettings["emailTemplatesFolder"];
                        newsletter.ConfirmationTemplate.Extension = ".tpl";
                        newsletter.ConfirmationTemplate.PhysicalApplicationPath = AppDomain.CurrentDomain.BaseDirectory;
                        
                        StringBuilder builder = new StringBuilder(confirmationPage.AbsoluteUri);
                        builder.Append("?" + REQUEST_HASH + "=" + subscription.Hash);
                        builder.Append("&" + REQUEST_ACTION + "=" + (int)NewsletterAction.ConfirmSubscription);
                        builder.Append("&" + REQUEST_SUBSCRIPTION_ID + "=" + subscription.SubscriptionId);

                        if (File.Exists(newsletter.ConfirmationTemplate.FilePath))
                        {
                            StreamReader objStreamReader = File.OpenText(newsletter.ConfirmationTemplate.FilePath);
                            newsletter.ConfirmationTemplate.TemplateContent = objStreamReader.ReadToEnd();
                            newsletter.ConfirmationTemplate.TemplateContent = newsletter.ConfirmationTemplate.TemplateContent.Replace("{$confirmRegistration}", builder.ToString());
                        }
                        else
                        {
                            throw new ApplicationException("Missing template: " + newsletter.ConfirmationTemplate.FilePath);
                        }

                        try
                        {
                            MailMessage message = new MailMessage();

                            message.To.Add(new MailAddress(subscription.Email));
                            message.Subject = messageSubject;
                            message.Body = newsletter.ConfirmationTemplate.TemplateContent;
                            message.IsBodyHtml = false;

                            SmtpClient client = new SmtpClient();
                            
                            client.Send(message);

                            result = NewsLtrSubRes.Success;
                        }
                        catch (Exception ex)
                        {
                            log.Debug(ex);
                            result = NewsLtrSubRes.Failed;
                        }
                    }
                    else
                    {
                        result = NewsLtrSubRes.Failed;
                    }
#if DISTRIBUTED_TRANSACTIONS 
                    ts.Complete();
                }
#endif
            }

            subscriptionId = subscription.SubscriptionId;

            return result;
        }

        public NewsLtrSubRes Subscribe(string emailTo, int newsletterId, out BONewsLtrSub subscription)
        {
            subscription = newsletterDB.GetSubscription(-1, newsletterId, emailTo);

            if (subscription.SubscriptionId > -1)
            {
                if (!subscription.Subscribed)
                {
                    return NewsLtrSubRes.Unsubscribed;
                }
                else if (!subscription.Confirmed)
                {
                    return NewsLtrSubRes.AwaitingConfirmation;
                }
                else
                {
                    return NewsLtrSubRes.AlreadyExists;
                }
            }
            else
            {
                subscription.Hash = StringTool.RandomString(10, false);

                if (newsletterDB.Change(subscription) > 0)
                {
                    return NewsLtrSubRes.Success;
                }
                else
                {
                    return NewsLtrSubRes.Failed;
                }
            }
        }

        public NewsLtrSubRes Unsubscribe(int subscriptionId, string hash)
        {
            BONewsLtrSub subscription = newsletterDB.GetSubscription(subscriptionId, -1, "");

            if (subscription.Hash != hash)
            {
                return NewsLtrSubRes.IllegalAttempt;
            }
            else if (subscription.SubscriptionId == -1)
            {
                return NewsLtrSubRes.NonExistent;
            }
            else
            {
                subscription.DateUnsubscribed = DateTime.Now;
                int rowsAffected = newsletterDB.Change(subscription);
                return (rowsAffected > 0) ? NewsLtrSubRes.Success : NewsLtrSubRes.Failed;
            }
        }

        public NewsLtrSubRes ConfirmSubscription(string hash, IPAddress ip, int subscriptionId)
        {
            BONewsLtrSub subscription = newsletterDB.GetSubscription(subscriptionId, -1, "");

            if (subscription.SubscriptionId == -1)
            {
                return NewsLtrSubRes.NonExistent;
            }
            else
            {
                subscription.Hash = hash;
                subscription.DateConfirmed = DateTime.Now;
                subscription.IpConfirmed = ip;
                int rowsAffected = newsletterDB.Change(subscription);
                return (rowsAffected > 0) ? NewsLtrSubRes.Success : NewsLtrSubRes.Failed;
            }
        }

        public NewsLtrSubRes DeleteSubscription(int subscriptionId)
        {
            return (newsletterDB.DeleteSubscription(subscriptionId) > 0 ? NewsLtrSubRes.Success : NewsLtrSubRes.Failed);
        }

        public PagedList<BONewsLtrSub> ListSubscriptions(int newsletterId, ListingState state, int subscriptionType)
        {
            return newsletterDB.ListSubscriptions(newsletterId, state, subscriptionType);
        }

        public List<BONewsLtrSub> ListSubscriptions(string email)
        {
            return newsletterDB.ListSubscriptions(email);
        }

        public List<BONewsLtr> ListNewsletters(List<int> newsletterIds)
        {
            return newsletterDB.ListNewsletters(newsletterIds);
        }
    }
}
