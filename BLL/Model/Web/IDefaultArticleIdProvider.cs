using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace One.Net.BLL.Web
{
    public interface IDefaultArticleIdProvider
    {
        int? DefaultArticleId { get; }
        bool EnableDefaultArticleIdProvider { get; }
    }
}
