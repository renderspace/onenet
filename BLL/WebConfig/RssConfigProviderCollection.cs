using System;
using System.Configuration;

namespace One.Net.BLL.WebConfig
{
    public class RssConfigProviderCollection : ConfigurationElementCollection
    {
        public RssConfigProvider this[int index]
        {
            get
            {
                return base.BaseGet(index) as RssConfigProvider;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }


        protected override ConfigurationElement CreateNewElement()
        {
            return new RssConfigProvider();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RssConfigProvider)element).Name;
        } 
    }
}
