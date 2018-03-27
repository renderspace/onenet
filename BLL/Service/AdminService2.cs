using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Activation;
using System.Runtime.Serialization;
using One.Net.BLL.DAL;
using Newtonsoft.Json;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;
using NLog;
using System.IO;
using Newtonsoft.Json.Converters;
using System.ServiceModel.Web;
using One.Net.BLL.Utility;
using System.Data.SqlTypes;

namespace One.Net.BLL.Service
{
    public partial class AdminService : IAdminService
    {
        public static string RenderStatusIcons(object objMarkedForDeletion, object objIsChanged)
        {
            string title = "";
            string strReturn = "";
            if (objIsChanged != null && objMarkedForDeletion != null)
            {
                if (bool.Parse(objMarkedForDeletion.ToString()))
                {
                    strReturn = "/Res/brisanje.png";
                    title = "Marked for deletion";
                }
                else if (bool.Parse(objIsChanged.ToString()))
                {
                    strReturn = "/Res/objava.png";
                    title = "Changes waiting for publish";
                }
                else
                {
                    strReturn = "/Res/objavljeno.png";
                    title = "Published";
                }
            }
            return "<img data-toggle='tooltip' data-placement='left' src='" + strReturn + "' alt='' title='" + title + "' />";
        }

        public List<DTOArticleSearch> ListArticles(int languageId, int page)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);
            var result = new List<DTOArticleSearch>();
            var contentB = new BContent();
            var state = new ListingState();
            state.RecordsPerPage = 15;
            state.SortDirection = SortDir.Descending;
            int firstRecordIndex = (page * state.RecordsPerPage.Value) - state.RecordsPerPage.Value;
            state.FirstRecordIndex = firstRecordIndex < 0 ? 0 : firstRecordIndex;
            state.SortField = "id";
            state.SortDirection = SortDir.Descending;
            PagedList<BOArticle> articles = articleB.ListArticles(new List<int>(), null, null, state, "", new List<int>());

            foreach (var a in articles)
            {
                var item = new DTOArticleSearch()
                {
                    Id = a.Id.Value.ToString(),
                    Status = RenderStatusIcons(a.MarkedForDeletion, a.IsChanged),
                    Title = a.Title,
                    HumanReadableUrl = a.HumanReadableUrl,
                    DisplayDate = a.DisplayDate,
                    Categories = a.RegularsList
                };
                result.Add(item);
            }

            WebOperationContext.Current.OutgoingResponse.Headers.Add("X-OneNet-AllRecords", articles.AllRecords.ToString());
            WebOperationContext.Current.OutgoingResponse.Headers.Add("X-OneNet-CurrentPage", articles.CurrentPage.ToString());
            WebOperationContext.Current.OutgoingResponse.Headers.Add("X-OneNet-RecordsPerPage", state.RecordsPerPage.ToString());
            return result;
        }

        public List<DTORegular> ListRegulars(int languageId)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);
            var state = new ListingState();
            state.SortDirection = SortDir.Ascending;
            state.SortField = "id";
            var regulars = articleB.ListRegulars(state);
            var result = new List<DTORegular>();
            foreach (var r in regulars)
            {
                result.Add(new DTORegular { Id = r.Id.Value, Title = r.Title });
            }
            return result;
        }

        public bool CheckArticleHumanReadableUrl(string humanReadableUrl, int articleId)
        {
            var a = articleB.GetArticle(humanReadableUrl);
            return (a != null && a.Id.HasValue && a.Id.Value != articleId);
        }

        public DTOArticle GetArticle(string rawId, int languageId)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);
            int id = 0;
            int.TryParse(rawId, out id);
            var a = articleB.GetArticle(id);
            if (a == null)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.NotFound;
                return null;
            }
            var ci = new CultureInfo(a.LanguageId);
            var result = new DTOArticle()
            {
                Id = a.Id.Value.ToString(),
                Status = RenderStatusIcons(a.MarkedForDeletion, a.IsChanged),
                Title = a.Title,
                SubTitle = a.SubTitle,
                Teaser = a.Teaser,
                Html = a.Html,
                HumanReadableUrl = a.HumanReadableUrl,
                DisplayDate = a.DisplayDate,
                Categories = a.RegularsList,
                DisplayLastChanged = a.DisplayLastChanged,
                LanguageId = a.LanguageId,
                ContentId = a.ContentId.Value,
                HasTranslationInCurrentLanguage = a.HasTranslationInCurrentLanguage,
                Language = CultureInfo.GetCultureInfo(a.LanguageId).IetfLanguageTag

            };
            result.Regulars = new List<DTORegular>();
            foreach (var r in a.Regulars)
            {
                result.Regulars.Add(new DTORegular { Id = r.Id.Value, Title = r.Title });
            }
            return result;
        }

        public int SaveArticle(DTOArticle article)
        {
            if(article == null)
            {
                return -1;
            }
            if (!Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                log.Error("ChangeContent NOT authenticated.");
                return -2;
            } 
            if (string.IsNullOrWhiteSpace(article.Title) || article.Teaser == null || article.Html == null)
            {
                log.Error("Null somewhere..");
                return -4;
            }
            if (article.Title.Contains(BOInternalContent.NO_TRANSLATION_TAG))
            {
                log.Error("ChangeContent NO_TRANSLATION_TAG");
                return -5;
            }
            if (article.DisplayDate < SqlDateTime.MinValue.Value)
            {
                article.DisplayDate = SqlDateTime.MinValue.Value;
            }
            if (article.Regulars == null || article.Regulars.Count() < 1)
            {
                return -6;
            }
            if (string.IsNullOrEmpty(article.HumanReadableUrl))
            {
                return -7;
            }
            int id = 0;
            int.TryParse(article.Id, out id);
            
            var a = new BOArticle
            {
                Id = id,
                HumanReadableUrl = article.HumanReadableUrl,
                Title = article.Title,
                Teaser = article.Teaser,
                Html = article.Html,
                DisplayDate = article.DisplayDate,
                IsChanged = true,
                MarkedForDeletion = false,
                PublishFlag = false,
                LanguageId = article.LanguageId,
                IsNew = id < 1
            };
            a.Regulars = new List<BORegular>();
            foreach(var r in article.Regulars)
            {
                a.Regulars.Add(new BORegular { Id = r.Id, Title = r.Title });
            }
            articleB.ChangeArticle(a);
            if (!a.Id.HasValue)
            {
                return -8;
            }
            else
            {
                return a.Id.Value;
            }
        }
    }
}
