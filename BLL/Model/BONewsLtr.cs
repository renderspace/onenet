using System;
using System.Net;

namespace One.Net.BLL
{
    public enum NewsLtrSubRes { Success = 1, AlreadyExists, Failed, NonExistent, AwaitingConfirmation, Unsubscribed, IllegalAttempt}

    [Serializable]
    public class BONewsLtrSub
    {
        public const int STARTED_SUBSCRIPTION = 1;
        public const int CONFIRMED = 2;
        public const int UNSUBSCRIBED = 3;

        int emailId = -1;
        int newsLetterId = -1;
        int subscriptionId = -1;
        string email = string.Empty;
        string hash = string.Empty;
        DateTime dateSubscribed = DateTime.MinValue;
        DateTime? dateConfirmed;
        DateTime? dateUnsubscribed;
        IPAddress ipConfirmed = null;
        
        public BONewsLtrSub()
        {}

        public int NewsLetterId
        {
            get { return this.newsLetterId; }
            set { this.newsLetterId = value; }
        }

        public int SubscriptionId
        {
            get { return this.subscriptionId; }
            set { this.subscriptionId = value; }
        }

        public string Email
        {
            get { return email; }
            set { email = value; }
        }

        public bool Confirmed { get { return DateConfirmed.HasValue; } }
        public bool Subscribed { get { return !DateUnsubscribed.HasValue && DateSubscribed != DateTime.MinValue; } }

        public string Hash
        {
            get { return hash; }
            set { hash = value; }
        }

        public DateTime? DateConfirmed
        {
            get { return dateConfirmed; }
            set { dateConfirmed = value; }
        }

        public DateTime DateSubscribed
        {
            get { return dateSubscribed; }
            set { dateSubscribed = value; }
        }

        public DateTime? DateUnsubscribed
        {
            get { return dateUnsubscribed; }
            set { dateUnsubscribed = value; }
        }

        public IPAddress IpConfirmed
        {
            get { return ipConfirmed; }
            set { ipConfirmed = value; }
        }

        public int EmailId
        {
            get { return emailId; }
            set { emailId = value; }
        }
    }

    [Serializable]
    public class BONewsLtr
    {
        BOTemplate confirmationTemplate = new BOTemplate();
        int id = -1;
        int confirmationTemplateId = -1;
        string name = string.Empty;

        public BONewsLtr()
        { }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public int ConfirmationTemplateId
        {
            get { return confirmationTemplateId; }
            set { confirmationTemplateId = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public BOTemplate ConfirmationTemplate
        {
            get { return confirmationTemplate; }
            set { confirmationTemplate = value; }
        }
    }
}
