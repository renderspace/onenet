using System;
using System.Collections.Generic;
using System.Text;

namespace One.Net.BLL.Model
{
    [Serializable]
    public abstract class PublishableInternalContent : BOInternalContent
    {
        public bool PublishFlag { get; set;}
        public bool IsNew { get; set;}
        public bool IsChanged { get; set;}
        public bool MarkedForDeletion { get; set;}

        public bool IsPublished { get { return !MarkedForDeletion && !IsNew; } }
    }
}
