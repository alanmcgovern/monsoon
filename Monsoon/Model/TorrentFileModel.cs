// TorrentFileModel.cs created with MonoDevelop
// User: alan at 01:15 13/04/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;
using Gtk;
using MonoTorrent.Client;
using MonoTorrent.Common;

namespace Monsoon
{
	public class TorrentFileModel : TreeStore
	{
		private Dictionary<string, Gdk.Pixbuf> pixbufs;
		private Download download;
		private TorrentManager manager;
		
		public TorrentManager Manager
		{
			get { return manager; }
		}
		
		public TorrentFileModel()
			: base (typeof(Download), typeof(TorrentFile), typeof(Gdk.Pixbuf), typeof(string))
		{
			pixbufs = new Dictionary<string, Gdk.Pixbuf>();
		}
		
		
		public void UpdateRow (TreeIter iter)
		{
			TorrentFile file = (TorrentFile) GetValue (iter, 1);
			SetValue (iter, 2, GetPixbuf (file.Priority));
		}
		
		public void Update (Download download)
		{
			if (this.download == download)
				return;

			this.download = download;
			this.manager = download == null ? null : download.Manager;
			
			Clear ();
			if (download == null)
				return;

			foreach (TorrentFile torrentFile in manager.Torrent.Files)
				AppendValues(manager, torrentFile, GetPixbuf (torrentFile.Priority), torrentFile.Path);
		}
		
		public Gdk.Pixbuf GetPixbuf (Priority priority)
		{
			string name;
			switch (priority) {
				case Priority.Immediate:
					name = "immediate.png";
				break;
				case Priority.Highest:
					name = "highest.png";
				break;
				case Priority.High:
					name = "high.png";
				break;
				case Priority.Normal:
					name = "null";
				break;
				case Priority.Low:
					name = "low.png";
				break;
				case Priority.Lowest:
					name = "lowest.png";
				break;
				case Priority.DoNotDownload:
					name = "donotdownload.png";
				break;
				default:
					name = "null";
				break;
			}

			if (!pixbufs.ContainsKey (name))
			{
				if (name == "null")
					pixbufs.Add(name, new Gdk.Pixbuf (IntPtr.Zero));
				else
					pixbufs.Add(name, new Gdk.Pixbuf(System.IO.Path.Combine(Defines.IconPath, name)));
			}
			return pixbufs[name];
		}
	}
}
