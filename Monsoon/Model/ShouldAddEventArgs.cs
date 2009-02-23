
using System;
using MonoTorrent.Common;

namespace Monsoon
{
	public class ShouldAddEventArgs : EventArgs
	{
		public string SavePath {
			get; private set;
		}
		public bool ShouldAdd {
			get; set;
		}
		public Torrent Torrent {
			get; private set;
		}
		
		public ShouldAddEventArgs (Torrent torrent, string savePath)
		{
			SavePath = savePath;
			ShouldAdd = true;
			Torrent = torrent;
		}
	}
}
