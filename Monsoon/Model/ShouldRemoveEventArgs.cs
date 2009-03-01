
using System;

namespace Monsoon
{
	public class ShouldRemoveEventArgs : EventArgs
	{
		public bool DeleteData {
			get; private set;	
		}
		public bool DeleteTorrent {
			get; private set;
		}
		public Download Download {
			get; private set;
		}
		public bool ShouldRemove {
			get; set;
		}
		
		public ShouldRemoveEventArgs (Download download, bool deleteData, bool deleteTorrent)
		{
			DeleteData = deleteData;
			DeleteTorrent = DeleteTorrent;
			Download = download;
			ShouldRemove = true;
		}
	}
}
