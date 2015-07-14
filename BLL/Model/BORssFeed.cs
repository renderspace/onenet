using System;
using System.Collections.Generic;
using System.Text;

namespace One.Net.BLL.Model
{
    [Serializable]
    public class BORssFeed
    {
        private int? id = null;
        private string title = "";
        private string description = "";
        private string type = "";
        private string linkToList = "";
        private string linkToSingle = "";
        private List<int> categories = new List<int>();
        private int languageId;

        public int? Id { get { return id; } set { id = value; } }
        public string Title { get { return title; } set { title = value; } }
        public string Description { get { return description; } set { description = value; } }
        public string Type { get { return type; } set { type = value; } }
        public string LinkToList { get { return linkToList; } set { linkToList = value; } }
        public string LinkToSingle { get { return linkToSingle; } set { linkToSingle = value; } }
        public List<int> Categories { get { return categories; } set { categories = value; } }
        public int LanguageId { get { return languageId; } set { languageId = value; } }
    }

    [Serializable]
    public class BORssItem
    {
        private string title;
        private string subTitle;
        private string teaser;
        private int id;
        private string html;
        private string imageUrl;
        private DateTime pubDate;

        public string Title { get { return title; } set { title = value; } }
        public string SubTitle { get { return subTitle; } set { subTitle = value; } }
        public string Teaser { get { return teaser; } set { teaser = value; } }
        public int Id { get { return id; } set { id = value; } }
        public string Html { get { return html; } set { html = value; } }
        public string ImageUrl { get { return imageUrl; } set { imageUrl = value; } }
        public DateTime PubDate { get { return pubDate; } set { pubDate = value; } }
    }

    [Serializable]
    public class BORssCategory
    {
        private string title;
        private int id;

        public string Title { get { return title; } set { title = value; } }
        public int Id { get { return id; } set { id = value; } }
    }
}
