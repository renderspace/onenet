namespace One.Net.BLL
{
    interface IContent
    {
        int? ContentId { get; set; }

        int LanguageId { get; set; }

        string Title { get; set; }

        string SubTitle { get; set; }

        string Teaser { get; set; }

        string Html { get; set; }
    }
}
