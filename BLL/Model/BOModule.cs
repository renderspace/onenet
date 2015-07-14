using System;
namespace One.Net.BLL
{
    [Serializable]
    public class BOModule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public int NoUnpublishedInstances { get; set; }
        public int NoPublishedInstances { get; set; }
        public int NoSettingsInDatabase { get; set; }   
        
    }
}
