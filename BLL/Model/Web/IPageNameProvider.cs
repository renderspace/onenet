using System;
using System.Collections.Generic;
using System.Text;

namespace One.Net.BLL.Web
{
    public interface IPageNameProvider
    {
        bool HasPageName { get; }
        string PageName { get; }
    }
}
