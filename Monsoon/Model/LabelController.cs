using System;
using System.Collections.Generic;

namespace Monsoon
{
	public class LabelController
	{
		public event EventHandler<LabelEventArgs> Added;
		public event EventHandler<LabelEventArgs> Removed;
		
		public TorrentLabel All {
			get; private set;
		}
		
		public TorrentLabel Delete {
			get; private set;
		}
		
		public TorrentLabel Downloading {
			get; private set;
		}
		
		public List<TorrentLabel> Labels {
			get; private set;
		}
		
		public TorrentLabel Seeding {
			get; private set;
		}

		public LabelController()
		{
			All = new TorrentLabel (_("All"), "gtk-home", true);
			Delete = new TorrentLabel (_("Remove"), "gtk-remove", true);
			Downloading = new TorrentLabel (_("Downloading"), "gtk-go-down", true);
			Seeding = new TorrentLabel (_("Seeding"), "gtk-go-up", true);
			
			Labels = new List<TorrentLabel> { All, Delete, Downloading, Seeding };
		}
		
		public void Add (TorrentLabel label)
		{
			Labels.Add (label);
			Event.Raise <LabelEventArgs> (Added, this, new LabelEventArgs (label)); 
		}
		
		public void Remove (TorrentLabel label)
		{
			if (Labels.Remove (label))
				Event.Raise <LabelEventArgs> (Removed, this, new LabelEventArgs (label)); 
		}
		
		static string _(string s)
		{
			return s;
		}
	}
}
