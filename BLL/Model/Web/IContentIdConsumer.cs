using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace One.Net.BLL.Web
{
    public interface IContentIdConsumer
    {
        int? ContentId { set; }
        string Uri { set; }
        string Title { set; }
        int TeaserImageId { set; }
        string Teaser { set; }
    }
}