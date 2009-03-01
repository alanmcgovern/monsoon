using System;
using System.Collections.Generic;
using MonoTorrent.Client;
using MonoTorrent.Common;
using MonoTorrent.BEncoding;

namespace Monsoon
{
	public class Download
	{
		public event EventHandler Started;
		public event EventHandler<TorrentStateChangedEventArgs> StateChanged;
		public event EventHandler Stopped;
		public event EventHandler Paused;
		public event EventHandler Resumed;
		
		double hashProgress;
		TorrentManager manager;
		SpeedMonitor swarmSpeed;
		
		public TorrentManager Manager {
			get { return manager; }
		}
		
		public MonoTorrent.Common.TorrentState State {
			get { return manager.State; }
		}
		
		public int Available {
			get { return manager.Peers.Available; }
		}
		
		public bool Complete {
			get { return manager.Complete; }
		}
		
		public long DownloadSpeed {
			get { return manager.Monitor.DownloadSpeed; }
		}

		public int Leechs {
			get { return manager.Peers.Leechs; }
		}
		
		public double Progress {
			get { return manager.State == TorrentState.Hashing ? hashProgress : manager.Progress / 100.0; }
		}
		
		public string SavePath {
			get { return manager.SavePath; }
		}
		
		public int Seeds {
			get { return manager.Peers.Seeds; }
		}
		
		public long SwarmSpeed {
			get { return swarmSpeed.Rate; }
		}
		
		public Torrent Torrent {
			get { return manager.Torrent; }
		}
		
		public long TotalDownloaded {
			get { return manager.Monitor.DataBytesDownloaded + manager.Monitor.ProtocolBytesDownloaded; }
		}
		
		public long TotalUploaded {
			get { return manager.Monitor.DataBytesUploaded + manager.Monitor.ProtocolBytesUploaded; }
		}
				
		public long UploadSpeed {
			get { return manager.Monitor.UploadSpeed; }
		}
		
		public Download (TorrentManager manager)
		{
			this.manager = manager;
			this.swarmSpeed = new SpeedMonitor ();
			// FIXME: This needs to be re-instated better
//			this.manager.Engine.ConnectionManager.PeerMessageTransferred += delegate(object sender, PeerMessageEventArgs e) {
//				if (e.Direction != Direction.Incoming)
//					return;
//			
//				if (!(e.Message is MonoTorrent.Client.Messages.Standard.HaveMessage))
//					return;
//
//				swarmSpeed.AddDelta (manager.Torrent.PieceLength);
//			};
			
			GLib.Timeout.Add (1000, delegate {
				swarmSpeed.Tick ();
				return true;
			});

			manager.PieceHashed += delegate (object sender, PieceHashedEventArgs e) {
				hashProgress = (float) e.PieceIndex / manager.Torrent.Pieces.Count;
			};
			
			manager.TorrentStateChanged += delegate(object sender, TorrentStateChangedEventArgs e) {
				if (e.NewState == TorrentState.Hashing)
					hashProgress = 0;
				Gtk.Application.Invoke (delegate {
					Event.Raise<TorrentStateChangedEventArgs> (StateChanged, this, e);
				});
			};
		}
		
		public List<Piece> GetRequests ()
		{
			return manager.GetActiveRequests ();
		}
		
		public List<PeerId> GetPeers ()
		{
			return manager.GetPeers ();
		}

		public void Pause ()
		{
			manager.Pause ();
			Event.Raise (Paused, this, EventArgs.Empty);
		}
		
		public void Resume ()
		{
			manager.Start ();
			Event.Raise (Resumed, this, EventArgs.Empty);
		}
		
		public FastResume SaveFastResume ()
		{
			return manager.SaveFastResume ();
		}
		
		public void Start ()
		{
			manager.Start ();
			Event.Raise (Started, this, EventArgs.Empty);
		}
		
		public void Stop ()
		{
			manager.Stop ().WaitOne ();
			Event.Raise (Stopped, this, EventArgs.Empty);
		}
	}
}
