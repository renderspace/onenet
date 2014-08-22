using System;
using System.Collections.Generic;
using System.Text;

namespace One.Net.BLL.Web
{
    public interface IBasicSEOProvider
    {
        string Title { get; }
        string Description { get; }
        string OgImageUrl { get; }
    }
}
