using System;
using System.Collections.Generic;

namespace One.Net.BLL.Model.Web
{
	public interface IMetaDataProvider
	{
		/// <summary>
		/// Add extra link tags in the head, useful for facebook
		/// at least.
		/// 
		/// The tags will look like this:
		/// &lt;link rel=dictionary_key href=dictionary_value&gt;
		/// </summary>
		Dictionary<string, string> ExtraLinkTags { get; }

		/// <summary>
		/// Add extra meta tags in the head, used mostly by facebook
		/// 
		/// The tags will look like this:
		/// &lt;meta property=dictionary_key content=dictionary_value&gt;
		/// </summary>
		Dictionary<string, string> ExtraMetaData { get; }
	}
}