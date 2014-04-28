using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace One.Net.BLL.Web
{
    public interface IContentIdProvider
    {
        int? ContentId { get; }
        bool EnableContentIdProvider { get; }
        string Title { get; }
        int TeaserImageId { get; }
        string Teaser { get; }
    }
}