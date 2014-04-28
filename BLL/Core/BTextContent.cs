using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;

namespace One.Net.BLL
{
	/// <summary>
	/// Summary description for BTextContent.
	/// </summary>
	[Serializable]
    public class BTextContent : BusinessBaseClass
	{
        readonly BWebsite webSiteB = new BWebsite();
        readonly BInternalContent contentB = new BInternalContent();

        private static readonly object cacheLockingTextContentModuleInstance = new Object();

        public BTextContent()
        {
            bool.TryParse(ConfigurationManager.AppSettings["PublishFlag"], out publishFlag);
        }

        public BOInternalContent GetTextContent(int moduleInstanceId, bool privatePublishFlag)
        {
            BOInternalContent answer = null;

            BOModuleInstance instance = webSiteB.GetModuleInstance(moduleInstanceId, privatePublishFlag);

            if (instance != null && instance.Settings != null && instance.Settings.ContainsKey("ContentId"))
            {
                int contentId = int.Parse(instance.Settings["ContentId"].Value);
                if (contentId > 0)
                    answer = contentB.Get(contentId);
                else
                    answer = new BOInternalContent();
            }
            return answer;
        }

        public void Vote(int votedScore, int moduleInstanceID)
        {
            contentB.Vote(votedScore, GetTextContent(moduleInstanceID, publishFlag).ContentId.Value);
        }

        private static string INTERNAL_CONTENT_CACHE_ID(int moduleInstanceID)
        {
            return "_IntCont_" + moduleInstanceID;
        }

	    public BOInternalContent GetTextContent(int moduleInstanceID)
        {
	        BOInternalContent moduleInstance = null;
	        var useCache = publishFlag;
            string cacheKey = CACHE_LANG_PREFIX + INTERNAL_CONTENT_CACHE_ID(moduleInstanceID);
            if (useCache)
                moduleInstance = OCache.Get(cacheKey) as BOInternalContent;

            if (moduleInstance == null)
            {
                moduleInstance = GetTextContent(moduleInstanceID, publishFlag);

                if (moduleInstance != null && !moduleInstance.MissingTranslation && useCache)
                {
                    lock (cacheLockingTextContentModuleInstance)
                    {
                        var temp = OCache.Get(cacheKey) as BOInternalContent;
                        if (null == temp)
                            OCache.Max(cacheKey, moduleInstance);
                    }
                }
            }
            return moduleInstance;
        }

        public void ChangeTextContent(int moduleInstanceID, string title, string subtitle, string teaser, string htmlContent)
        {
            if (publishFlag)
            {
                throw new ApplicationException("invalid configuration: admin application should be using publish=false");
            }

            BOModuleInstance instance = webSiteB.GetModuleInstance(moduleInstanceID);

            int contentId = Int32.Parse(instance.Settings["ContentId"].Value);

            BOInternalContent content = contentB.Get(contentId);

            if (content != null && contentId > 0)
            {
                content.Title = title;
                content.SubTitle = subtitle;
                content.Teaser = teaser;
                content.Html = htmlContent;
                content.LanguageId = LanguageId;
                content.ContentId = contentId;
                contentB.Change(content);
            }
            else
            {
                content = new BOInternalContent();
                content.Title = title;
                content.SubTitle = subtitle;
                content.Teaser = teaser;
                content.Html = htmlContent;
                content.LanguageId = LanguageId;
                contentB.Change(content);
                instance.Settings["ContentId"] = new BOSetting("ContentId", "Int", content.ContentId.Value.ToString(), BOSetting.USER_VISIBILITY_SPECIAL);
            }

            webSiteB.ChangeModuleInstance(instance);
        }
    }
}