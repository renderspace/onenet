using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using One.Net.BLL.DAL;


namespace One.Net.BLL
{
    [Serializable]
    public class BContent : BusinessBaseClass
    {
        private const string NO_MEANING_PRE = "<span style=\"color: Purple; font-weight: strong;\">";
        private const string NO_MEANING_POST = "</span>";

        public enum ResultCode { OK, ERROR, NO_CHANGE, KEYWORD_ALREADY_EXISTS } ;
        
        private static readonly BInternalContent contentB = new BInternalContent();

        // under heavy load mulitple requests will start the reading process, then the Cache
        // inserts will invalidate cache. Better to check for existence first.
        private static readonly object cacheLockingDictionary = new Object();

        protected const string DICT_CACHE_ID = "DICT_";
        protected const string CPLX_DICT_CACHE_ID = "CXDICT_";

        public static string GetMeaning(string keyWord)
        {
            return GetMeaning(keyWord, false);
        }

        public static string GetMeaning(string keyWord, bool noHtml = false)
        {
            if (string.IsNullOrEmpty(keyWord))
                return string.Empty;

            string cacheKey = DICT_CACHE_ID + "L_" + Thread.CurrentThread.CurrentCulture.LCID + "_" + keyWord;

            string meaning = OCache.Get(cacheKey) as string;
            if (string.IsNullOrEmpty(meaning))
            {

                BODictionaryEntry stc = contentDb.GetDictionaryEntry(keyWord, Thread.CurrentThread.CurrentCulture.LCID);
                meaning = (stc != null) ? stc.Title : null; 

                if (!string.IsNullOrEmpty(meaning))
                {
                    lock (cacheLockingDictionary)
                    {
                        string tempMeaning = OCache.Get(cacheKey) as string;
                        if (string.IsNullOrEmpty(tempMeaning))
                            OCache.Max(cacheKey, meaning);
                    }
                }
            }

            if (string.IsNullOrEmpty(meaning))
            {
                bool publishFlag = false;
                bool.TryParse(ConfigurationManager.AppSettings["PublishFlag"], out publishFlag);

                if (publishFlag)
                {
                    log.Error("Missing meaning for:" + keyWord);
                    return "";
                }
                else
                {
                    log.Debug("Missing meaning for:" + keyWord);
                    if (noHtml)
                        return keyWord;
                    return NO_MEANING_PRE + keyWord + NO_MEANING_POST;
                }
                    
            }

            return meaning;
        }

        public static BODictionaryEntry GetComplexMeaning (string keyWord)
        {
            if (string.IsNullOrEmpty(keyWord))
                return new BODictionaryEntry();

            // this is static context, we can't access CACHE_LANG_PREFIX
            string cacheKey = CPLX_DICT_CACHE_ID + "L_" + Thread.CurrentThread.CurrentCulture.LCID + 
                "_" + keyWord;

            BODictionaryEntry meaning = OCache.Get(cacheKey) as BODictionaryEntry;
            if (null == meaning)
            {
                BODictionaryEntry stc = contentDb.GetDictionaryEntry(keyWord, Thread.CurrentThread.CurrentCulture.LCID);
                if (stc != null && stc.ContentId.HasValue && !stc.MissingTranslation)
                {
                    lock (cacheLockingDictionary)
                    {
                        BODictionaryEntry tempMeaning = OCache.Get(cacheKey) as BODictionaryEntry;
                        if (null == tempMeaning)
                            OCache.Max(cacheKey, stc);
                    }
                    return stc;
                }
            }

            bool publishFlag = false;
            bool.TryParse(ConfigurationManager.AppSettings["PublishFlag"], out publishFlag);
            if (null == meaning && !publishFlag)
            {
                meaning = new BODictionaryEntry();
                meaning.Title = NO_MEANING_PRE + keyWord + NO_MEANING_POST;
                meaning.Teaser = NO_MEANING_PRE + keyWord + NO_MEANING_POST;
                meaning.SubTitle = NO_MEANING_PRE + keyWord + NO_MEANING_POST;
                meaning.Html = NO_MEANING_PRE + keyWord + NO_MEANING_POST;
                log.Debug("Missing COMPLEX meaning for:" + keyWord);
            }
            else if (null == meaning)
            {
                meaning = new BODictionaryEntry();
                meaning.Title = "";
                meaning.Teaser = "";
                meaning.SubTitle = "";
                meaning.Html = "";
                log.Error("Missing  COMPLEX meaning for:" + keyWord);
            }

            return meaning;
        }

        public static string GetComplexMeaningRendered(string keyWord)
        {
            BODictionaryEntry meaning = GetComplexMeaning(keyWord);
            if (null == meaning)
                return "";
            else
            { 
                string result = "<div class=\"meaning\">";

                if (!string.IsNullOrEmpty(meaning.Title))
                    result += "<h2>" + meaning.Title + "</h2>";
                if (!string.IsNullOrEmpty(meaning.SubTitle))
                    result += "<h3>" + meaning.SubTitle + "</h3>";
                if (!string.IsNullOrEmpty(meaning.ProcessedTeaser))
                    result += "<div class=\"teaser\">" + meaning.ProcessedTeaser + "</div>";
                if (!string.IsNullOrEmpty(meaning.ProcessedHtml))
                    result += "<div class=\"content\">" + meaning.ProcessedHtml + "</div>";

                result += "</div>";
                return result;
            }
        }

		/// <summary>
		/// used mainly for slovenian or similar languages for plural / singular issues with counting items
		/// eg 1 jabuka, 2 jabuke, 3 jabuke, 8 jabuka
		/// data is stored in dictionary as jabuka;jabuke;jabuke;jabuke;jabuka... so on
		/// </summary>
		/// <param name="keyWord"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public static string GetMeaning(string keyWord, int position)
		{
			string strReturn = "";
			
			string rawData = GetMeaning(keyWord);
			Regex splitter = new Regex(";");
			string[] arrAnswer = splitter.Split(rawData);
			for (int i = 0; i < arrAnswer.Length; i++)
			{
				if ( i == (position -1))
				{
					strReturn = arrAnswer[i];
					break;
				}
				// if we are at the end of the array and position we seek is greater than the count we have reached
				// just return the last available value
				else if (( i == (arrAnswer.Length-1)) && ((position - 1) > i))
				{
					strReturn = arrAnswer[i];
					break;
				}
			}

			return strReturn;
		}

        /// <summary>
        /// </summary>
        /// <param name="entry"></param>
        public ResultCode ChangeDictionaryEntry(BODictionaryEntry entry)
        {
            BODictionaryEntry existing = contentDb.GetDictionaryEntry(entry.KeyWord);

            if (string.IsNullOrEmpty(entry.KeyWord))
                return ResultCode.NO_CHANGE;

            if (existing != null && entry.ContentId.HasValue && existing.ContentId.HasValue &&
                  entry.ContentId.Value != existing.ContentId.Value) 
                throw new InvalidDataException("ContentId is diffrent between existing and new version of keyword. Did data change?");

            if (null != existing && !entry.ContentId.HasValue)
                entry.ContentId = existing.ContentId.Value;


            contentB.Change(entry);

            if (existing == null && entry.ContentId.HasValue) 
            {
                contentDb.InsertDictionaryEntry(entry);
            }

            return ResultCode.OK;
        }

        public BODictionaryEntry GetDictionaryEntry(string keyWord, int languageId)
        {
            if (string.IsNullOrEmpty(keyWord))
                return null;
            BODictionaryEntry entry = contentDb.GetDictionaryEntry(keyWord, languageId);
            return entry;
        }

        public BODictionaryEntry GetDictionaryEntry(string keyWord)
        {
            if (string.IsNullOrEmpty(keyWord))
                return null;
            BODictionaryEntry entry = contentDb.GetDictionaryEntry(keyWord, LanguageId);
            return entry;
        }

        public bool DeleteDictionaryEntry(string keyWord)
        {
            BODictionaryEntry entry = contentDb.GetDictionaryEntry(keyWord);

            if (entry != null)
            {
                contentB.Delete(entry.ContentId.Value);
                contentDb.DeleteDictionaryEntry(keyWord);
                return true;
            }
            return false;
        }

        public List<List<BODictionaryEntry>> ListAllDictionaryEntries()
        {
            return contentDb.ListAllDictionaryEntries();
        }

        public PagedList<BODictionaryEntry> ListDictionaryEntries(ListingState state, bool showUntranslated, string searchKeywordOrMeaning)
        {
            PagedList<BODictionaryEntry> entries = contentDb.ListDictionaryEntries(state, showUntranslated, LanguageId, searchKeywordOrMeaning);

            foreach (BODictionaryEntry entry in entries)
            {
                if (showUntranslated && entry.ContentId.HasValue && entry.MissingTranslation)
                    entry.Title = BInternalContent.GetContentTitleInAnyLanguage(entry.ContentId.Value);
            }
            return entries;
        }

        public List<BODictionaryEntry> ListDictionaryEntries(int languageId, string searchKeyword)
        {
            return contentDb.ListDictionaryEntries(languageId, searchKeyword);
        }
	}
}
