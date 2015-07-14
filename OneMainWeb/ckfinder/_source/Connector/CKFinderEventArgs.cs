using System;
using System.Collections.Generic;
using System.Text;

namespace CKFinder.Connector
{
	public class CKFinderEventArgs : EventArgs
	{
		public CKFinderEventArgs( params object[] paramlist )
		{
			this.data = paramlist;
			this.cancelled = false;
		}

		public object[] data;
		public bool cancelled;
	}
}
