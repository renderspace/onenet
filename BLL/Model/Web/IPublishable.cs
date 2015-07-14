namespace One.Net.BLL
{
    public interface IPublishable
    {
        bool PublishFlag { get; set; }
        bool IsChanged { get; set ; }
    }
}
