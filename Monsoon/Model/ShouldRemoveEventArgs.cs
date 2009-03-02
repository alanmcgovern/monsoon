
using System;
using System.Collections.Generic;

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
		public List<Download> Downloads {
			get; private set;
		}
		public bool ShouldRemove {
			get; set;
		}
		
		public ShouldRemoveEventArgs (List <Download> downloads, bool deleteData, bool deleteTorrent)
		{
			DeleteData = deleteData;
			DeleteTorrent = deleteTorrent;
			Downloads = downloads;
			ShouldRemove = true;
		}
	}
}
