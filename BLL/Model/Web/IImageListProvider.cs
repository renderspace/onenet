using System.Collections.Generic;

namespace One.Net.BLL.Web
{
    public interface IImageListProvider
    {
        /// <summary>
        /// Provides a list of all images displayed by a module. Should be availible after Load event.
        /// </summary>
        List<BOIntContImage> ListImages { get; }
    }
}
