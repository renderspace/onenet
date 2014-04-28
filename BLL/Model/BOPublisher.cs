using System;
using System.Collections.Generic;
using System.Text;

namespace One.Net.BLL
{
    public class BOPublisherData
    {
        public const string ARTICLE_SUBSYSTEM = "Article";
        public const string COMMENT_SUBSYSTEM = "Comment";
        public const string FAQ_SUBSYSTEM = "Faq";
        public const string EVENT_SUBSYSTEM = "Event";
        public const string PAGE_SUBSYSTEM = "Page";

        private int? id;
        public int? Id { get { return id; } set { id = value; } }

        private string subSystem = string.Empty;
        public string SubSystem { get { return subSystem; } set { subSystem = value; } }

        private int fkId;
        public int FkId { get { return fkId; } set { fkId = value; } }

        private DateTime scheduledAt;
        public DateTime ScheduledAt { get { return scheduledAt; } set { scheduledAt = value; } }

        private DateTime? publishedAt;
        public DateTime? PublishedAt { get { return publishedAt; } set { publishedAt = value; } }

        private string table;
        public string Table { get { return table; } set { table = value; } }

        public override string ToString()
        {
            return (SubSystem ?? "[uninitialized BOPublisherData]") + " " + FkId;
        }
    }
}
