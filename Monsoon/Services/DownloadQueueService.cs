
using System;
using MonoTorrent.Common;
using MonoTorrent.Client;

namespace Monsoon
{
	public class DownloadQueueService : IService
	{
		public bool Initialised {
			get; private set;
		}
		
		public int MaxActiveDownloads {
			get; set;
		}
		
		public DownloadQueueService()
		{
			TorrentController c = ServiceManager.Get <TorrentController> ();
			c.Added += delegate(object sender, DownloadAddedEventArgs e) {
				e.Download.ShouldStart += HandleShouldStart;
				e.Download.StateChanged += HandleStateChanged;
			};
			c.Removed += delegate(object sender, DownloadAddedEventArgs e) {
				e.Download.ShouldStart -= HandleShouldStart;
				e.Download.StateChanged -= HandleStateChanged;
			};
		}

		void HandleStateChanged(object sender, TorrentStateChangedEventArgs e)
		{
			TorrentController c = ServiceManager.Get <TorrentController> ();
			if (e.OldState == TorrentState.Downloading && e.NewState != TorrentState.Paused) {
				foreach (Download download in c.Torrents) {
					if (download.Queued) {
						download.Start ();
						return;
					}
				}
			}
		}

		void HandleShouldStart(object sender, ShouldStartEventArgs e)
		{
			TorrentController c = ServiceManager.Get <TorrentController> ();
			int count = 0;
			c.Torrents.ForEach (delegate (Download d) {
				if (d.Manager.State == TorrentState.Downloading)
					count++;
			});
			
			if (count >= MaxActiveDownloads) {
				e.ShouldStart = false;
				e.Download.Queued = true;
			}
		}
		
		public void Initialise ()
		{
			Initialised = true;
		}
	}
}
