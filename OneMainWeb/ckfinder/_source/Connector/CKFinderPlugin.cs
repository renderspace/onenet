using System;
using System.Collections.Generic;
using System.Text;

namespace CKFinder
{
	public interface CKFinderPlugin
	{
		string JavascriptPlugins
		{
			get;
		}
		void Init( Connector.CKFinderEvent CKFinderEvent );
	}
}
