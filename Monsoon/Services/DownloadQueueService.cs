
using System;
using System.Collections.Generic;
using MonoTorrent.Common;
using MonoTorrent.Client;

namespace Monsoon
{
	public class DownloadQueueService : IService
	{
		DateTime LastChanged {
			get; set;
		}
		
		public bool Initialised {
			get; private set;
		}

//		public int MaxActive {
//			get; set;
//		}
		
		public int MaxDownloads {
			get; set;
		}
		
//		public int MaxSeeds {
//			get; set;
//		}
		
		public DownloadQueueService()
		{
			TorrentController c = ServiceManager.Get <TorrentController> ();
			c.Added += delegate(object sender, DownloadAddedEventArgs e) {
				e.Download.ShouldStart += HandleShouldStart;
				e.Download.StateChanged += HandleStateChanged;
				e.Download.PriorityChanged += HandlePriorityChanged;;
			};
			c.Removed += delegate(object sender, DownloadAddedEventArgs e) {
				e.Download.ShouldStart -= HandleShouldStart;
				e.Download.StateChanged -= HandleStateChanged;
				e.Download.PriorityChanged -= HandlePriorityChanged;
			};
		}

		void HandlePriorityChanged(object sender, DownloadAddedEventArgs e)
		{
			HandleChange ();
		}

		void HandleStateChanged(object sender, StateChangedEventArgs e)
		{
			HandleChange ();
		}

		void HandleChange ()
		{
			if (MaxDownloads <= 0)
				return;
			
			Console.WriteLine ("State/Priority changed");
			LastChanged = DateTime.Now;
			GLib.Timeout.Add ((uint)TimeSpan.FromSeconds (2).TotalMilliseconds, delegate {
				if (DateTime.Now - LastChanged > TimeSpan.FromSeconds (1)) {
					Console.WriteLine ("Initiating review");
					LastChanged = DateTime.Now;
					ReviewDownloads ();
				}
				return false;
			});
		}

		void HandleShouldStart(object sender, ShouldStartEventArgs e)
		{
			if (MaxDownloads <= 0)
				return;
			
			TorrentController c = ServiceManager.Get <TorrentController> ();
			int count = 0;
			c.Torrents.ForEach (delegate (Download d) {
				if (d.Manager.State == TorrentState.Downloading)
					count++;
			});
			
			if (count >= MaxDownloads) {
				e.ShouldStart = false;
				e.Download.Queue ();
			}
		}
		
		public void Initialise ()
		{
			Initialised = true;
		}

		void ReviewDownloads ()
		{
			TorrentController controller = ServiceManager.Get <TorrentController> ();
			List<Download> downloads = new List<Download>(controller.Torrents);
			downloads.Sort ((left, right) => left.Priority.CompareTo (right.Priority));

			// Try to start the torrents without stopping any existing ones
			while (Toolbox.Count <Download>(downloads, d => d.Active) < MaxDownloads) {
				bool started = false;
				for (int i = 0; i < downloads.Count && !started; i++) {
					if (downloads[i].Queued) {
						downloads[i].Start ();
						started = true;
					}
				}

				if (!started)
					break;
			}

			//if (Toolbox.Count <Download>(downloads, d => d.Active) <= MaxActiveDownloads)
			//	return;

			// Ensure that the highest priority downloads are active
			for (int i = 0; i < downloads.Count; i++) {
				if (downloads [i].Queued) {
					for (int j = downloads.Count - 1; j > i; j--) {
						Console.WriteLine ("\t Checking: {0}", downloads [j]);
						if (downloads[j].Active) {
							Console.WriteLine ("\t\tStopping: {0}", downloads[j]);
							downloads[j].Queue ();
							downloads[i].Start ();
							break;
						}
					}
				}
			}
		}
	}
}
