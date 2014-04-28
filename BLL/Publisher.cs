using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using log4net;
using One.Cron.API;
using One.Net.BLL;

namespace One.Net.BLL
{
    public class Publisher : IJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Publisher));

        private static readonly BPublisher publisherB = new BPublisher();

        public string Description { get; set; }

        public bool IsRunning  { get; set; }
        
        public void Execute()
        {
            const int batchSize = 5;
            int publishedPerBatch = 0;
            int runningLoop = 0;
            var recentlyPublishedItems = new List<BOPublisherData>();

            var items = publisherB.ListPendingItems(
                                new ListingState(batchSize, 0, SortDir.Ascending, "scheduled_at"));

            foreach (BOPublisherData item in items)
            {
                if (publisherB.PublishItem(item))
                {
                    log.Info("Published item " + item);
                    //recentlyPublishedItems.Add(item);
                    publishedPerBatch++;
                }
                else
                    log.Info("FAILED to publish item " + item);

            }
            if (items.AllRecords < batchSize)
            {
                if (publishedPerBatch > 0)
                    log.Info("Published " + publishedPerBatch + " items.");
                if (items.AllRecords > 0)
                    SendReport(recentlyPublishedItems, publishedPerBatch);
                Thread.Sleep(1000);
            }
            else
            {
                runningLoop++;
                if (runningLoop > 4)
                {
                    log.Info("skipped the looping (after 5 loops) (5x batchSize = items)");
                }
            }
        }

        private static void SendReport(IEnumerable<BOPublisherData> list, int publishedPerBatch)
        {
            MailMessage message = new MailMessage();
            message.IsBodyHtml = false;

            foreach (string s in StringTool.SplitString(ConfigurationManager.AppSettings.Get("FatalReportToEmailList")))
                message.To.Add(new MailAddress(s));

            if (list != null)
            {
                message.Subject = "AutoPublish Report - " + publishedPerBatch + " published items";
                StringBuilder builder = new StringBuilder();
                foreach (BOPublisherData data in list)
                {
                    builder.Append("- ");
                    builder.Append(data + "\r\n\r\n");
                }
                builder.Append("\r\n\r\n");
                builder.Append(DateTime.Now.ToLongDateString());
                message.Body = builder.ToString();
                if (message.To.Count > 0)
                {
                    try
                    {
                        SmtpClient client = new SmtpClient();
                        client.Send(message);
                        log.Info("---------------- report sent to: " + message.To[0].Address);
                    }
                    catch (Exception eex)
                    {
                        log.Fatal("** Sending report failed ** ", eex);
                    }
                }
            }
        }

        private static void SendReport(Exception ex)
        {
            MailMessage message = new MailMessage();
            message.IsBodyHtml = false;

            foreach (string s in StringTool.SplitString(ConfigurationManager.AppSettings.Get("FatalReportToEmailList")))
                message.To.Add(new MailAddress(s));

            if (ex != null)
            {
                message.Subject = "Failure - AutoPublish SERVICE - shutting down";
                StringBuilder builder = new StringBuilder();
                builder.Append("Message \r\n\r\n");
                builder.Append(ex.Message + "\r\n\r\n");
                builder.Append("Source \r\n\r\n");
                builder.Append(ex.Source + "\r\n\r\n");
                builder.Append("StackTrace" + "\r\n\r\n");
                builder.Append(ex.StackTrace + "\n");
                message.Body = builder.ToString();
            }
            else
            {
                message.Subject = "Email test - CRAWLER SERVICE";
                message.Body = "test";
            }
            message.Priority = MailPriority.High;
            if (message.To.Count > 0)
            {
                try
                {
                    SmtpClient client = new SmtpClient();
                    client.Send(message);
                    log.Info("---------------- report sent to: " + message.To[0].Address);
                }
                catch (Exception eex)
                {
                    log.Fatal("** Sending report failed ** ", eex);
                }
            }
        }
    }
}
