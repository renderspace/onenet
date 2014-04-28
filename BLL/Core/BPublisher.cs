using System;
using System.Collections.Generic;
using System.Text;
using One.Net.BLL.DAL;

namespace One.Net.BLL
{
    public class BPublisher
    {
        private static readonly DbPublisher publisherDb = new DbPublisher();

        public PagedList<BOPublisherData> ListPublisherItems(ListingState listingState, bool published)
        {
            return publisherDb.ListPublisherData(listingState, published, false);
        }

        public PagedList<BOPublisherData> ListPendingItems(ListingState listingState)
        {
            return publisherDb.ListPublisherData(listingState, false, true);
        }

        public void Delete(int publisherItemId)
        {
            publisherDb.Delete(publisherItemId);
        }

        public void Change(BOPublisherData data)
        {
            publisherDb.Change(data);
        }

        public bool PublishItem(BOPublisherData item)
        {
            bool success = true;
            switch(item.SubSystem)
            {
                case BOPublisherData.ARTICLE_SUBSYSTEM:
                    BArticle articleB = new BArticle();
                    articleB.Publish(item.FkId);
                    break;
                case BOPublisherData.PAGE_SUBSYSTEM:
                    BWebsite webSiteB = new BWebsite();
                    success = webSiteB.PublishPage(item.FkId);
                    break;
            }
            if (success)
            {
                item.PublishedAt = DateTime.Now;
                Change(item);
            }
            return success;
        }
    }
}
