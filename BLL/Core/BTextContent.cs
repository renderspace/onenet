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

        public BOInternalContent GetTextContent(int moduleInstanceId, bool privatePublishFlag)
        {
            BOInternalContent answer = null;

            BOModuleInstance instance = webSiteB.GetModuleInstance(moduleInstanceId, privatePublishFlag);

            if (instance != null && instance.Settings != null && instance.Settings.ContainsKey("ContentId"))
            {
                int contentId = int.Parse(instance.Settings["ContentId"].Value);
                if (contentId > 0)
                    answer = contentB.GetUnCached(contentId);
                else
                    answer = new BOInternalContent();
            }
            return answer;
        }

        private static string INTERNAL_CONTENT_CACHE_ID(int moduleInstanceID)
        {
            return "_IntCont_" + moduleInstanceID;
        }

	    public BOInternalContent GetTextContent(int moduleInstanceID)
        {
	        BOInternalContent moduleInstance = null;
            var useCache = PublishFlag;
            string cacheKey = CACHE_LANG_PREFIX + INTERNAL_CONTENT_CACHE_ID(moduleInstanceID);
            if (useCache)
                moduleInstance = cache.Get<BOInternalContent>(cacheKey);

            if (moduleInstance == null)
            {
                moduleInstance = GetTextContent(moduleInstanceID, PublishFlag);

                if (moduleInstance != null && !moduleInstance.MissingTranslation && useCache)
                {
                    lock (cacheLockingTextContentModuleInstance)
                    {
                        var temp = cache.Get<BOInternalContent>(cacheKey);
                        if (null == temp)
                            cache.Put(cacheKey, moduleInstance);
                    }
                }
            }
            return moduleInstance;
        }

        public int ChangeTextContent(int moduleInstanceID, string title, string subtitle, string teaser, string htmlContent)
        {
            if (PublishFlag)
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
                instance.Settings["ContentId"] = new BOSetting("ContentId", SettingTypeEnum.Int, content.ContentId.Value.ToString(), VisibilityEnum.SPECIAL);
            }

            webSiteB.ChangeModuleInstance(instance);
            int result = 0;
            if (instance != null && instance.Settings != null)
            {
                int.TryParse(instance.Settings["ContentId"].Value, out result);
            }
            return result;
        }
    }
}