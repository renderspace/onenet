using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace One.Net.BLL.Web
{
    public interface ILeadImageProvider
    {
        string ImageUri { get; }
        int ImageId { get; }
        bool EnableLeadImageProvider { get; }
        string ImageDescription { get; }
    }
}
