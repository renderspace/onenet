using System;
using System.Collections.Generic;
using System.Text;

namespace One.Net.BLL.Web
{
    public interface IBasicSEOProvider
    {
        bool HasTitle { get; }
        string Title { get; }

        bool HasDescription { get; }
        string Description { get; }
    }
}
