
using System;

namespace Monsoon
{
	public class ShouldStartEventArgs : EventArgs
	{
		public Download Download {
			get; private set;
		}
		
		public bool ShouldStart {
			get; set;
		}
		
		public ShouldStartEventArgs (Download download)
		{
			Download = download;
			ShouldStart = true;
		}
	}
}
