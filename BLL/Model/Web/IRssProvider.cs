using System;
using System.Collections.Generic;
using System.Text;
using One.Net.BLL.Model;
using One.Net.BLL.WebConfig;

namespace One.Net.BLL
{
    public interface IRssProvider
    {
        RssConfigProvider ConfigProvider { get; set; }
        List<BORssItem> ListItems(List<int> categories, int languageId);
        List<BORssCategory> ListCategories(int languageId);
    }
}
