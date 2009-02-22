
using System;

namespace Monsoon
{
	public class DownloadAddedEventArgs : EventArgs
	{
		public Download Download {
			get; private set;
		}
		
		public DownloadAddedEventArgs (Download download)
		{
			Download = download;
		}
	}
}
