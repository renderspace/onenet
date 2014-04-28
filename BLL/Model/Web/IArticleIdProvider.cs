using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace One.Net.BLL.Web
{
    public interface IArticleIdProvider
    {
        int? ArticleId { get; }
        bool EnableArticleIdProvider { get; }
    }
}
