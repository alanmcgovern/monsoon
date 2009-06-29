using System;
using System.Collections.Generic;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Common;
using MonoTorrent.BEncoding;

namespace Monsoon
{
	public class Download
	{
		public event EventHandler<ShouldStartEventArgs> ShouldStart;
		public event EventHandler Started;
		public event EventHandler<StateChangedEventArgs> StateChanged;
		public event EventHandler Stopped;
		public event EventHandler Paused;
		public event EventHandler<DownloadAddedEventArgs> PriorityChanged;
		public event EventHandler Resumed;
		
		double hashProgress;
		TorrentManager manager;
		int priority;
		State state = State.Stopped;
		SpeedMonitor swarmSpeed;
		
		public TorrentManager Manager {
			get { return manager; }
		}
		
		public State State {
			get { return state; }
			set {
				State oldState = state;
				state = value;
				if (oldState != state)
					Event.Raise <StateChangedEventArgs> (StateChanged, this, new StateChangedEventArgs (this, state, oldState));
			}
		}

		public bool Active {
			get; private set;
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

		public InfoHash InfoHash {
			get { return manager.InfoHash; }
		}
		
		public int Priority {
			get { return priority; }
			set {
				priority = value;
				Event.Raise <DownloadAddedEventArgs> (PriorityChanged, this, new DownloadAddedEventArgs (this));
			}
		}
		
		public double Progress {
			get { return state == State.Hashing ? hashProgress : manager.Progress / 100.0; }
		}

		public bool Queued {
			get { return state == State.Queued; }
		}
		
		public string SavePath {
			get; private set;
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
		
		public Download (string savePath, TorrentManager manager)
		{
			this.manager = manager;
			this.swarmSpeed = new SpeedMonitor (30);
			SavePath = savePath;
			
			GLib.Timeout.Add (1000, delegate {
				swarmSpeed.Tick ();
				return true;
			});
			
			manager.PieceHashed += delegate (object sender, PieceHashedEventArgs e) {
				hashProgress = (double) e.PieceIndex / manager.Torrent.Pieces.Count;
			};
			
			manager.TorrentStateChanged += delegate(object sender, TorrentStateChangedEventArgs e) {
				hashProgress = 0;

				if (Active)
				Gtk.Application.Invoke (delegate {
					switch (e.NewState) {
					case TorrentState.Downloading:
						State = State.Downloading;
						break;
					case TorrentState.Hashing:
						State = State.Hashing;
						break;
					case TorrentState.Paused:
						State = State.Paused;
						break;
					case TorrentState.Seeding:
						State = State.Seeding;
						break;
					case TorrentState.Stopped:
						State = State.Stopped;
						Active = false;
						break;
					}
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

		void HandlePeerMessageTransferred(object sender, PeerMessageEventArgs e)
		{
			if (e.Direction != Direction.Incoming)
				return;
			if (!(e.Message is MonoTorrent.Client.Messages.Standard.HaveMessage))
				return;
			
			swarmSpeed.AddDelta (manager.Torrent.PieceLength);
		}
		
		public void HashCheck ()
		{
			Active = true;
			manager.HashCheck (false);
		}
		
		public void Pause ()
		{
			manager.Pause ();
			Event.Raise (Paused, this, EventArgs.Empty);
			State = State.Paused;
		}

		public void Queue ()
		{
			if (Active)
				Stop ();
			State = State.Queued;
		}
		
		public void Resume ()
		{
			manager.Start ();
			Event.Raise (Resumed, this, EventArgs.Empty);
			State = Manager.Complete ? State.Seeding : State.Downloading;
		}
		
		public FastResume SaveFastResume ()
		{
			return manager.SaveFastResume ();
		}
		
		public void Start ()
		{
			EventHandler <ShouldStartEventArgs> h = ShouldStart;
			if (h != null) {
				ShouldStartEventArgs e = new ShouldStartEventArgs (this);
				h (this, e);
				if (!e.ShouldStart)
					return;
			}
			Active = true;
			manager.Start ();
			manager.Engine.ConnectionManager.PeerMessageTransferred += HandlePeerMessageTransferred;
			
			Event.Raise (Started, this, EventArgs.Empty);
			State = manager.Complete ? State.Seeding : State.Downloading;
		}
		
		public void Stop ()
		{
			Active = false;
			if (State == State.Queued) {
				State = State.Stopped;
			} else {
				manager.Stop ();
				manager.Engine.ConnectionManager.PeerMessageTransferred -= HandlePeerMessageTransferred;
				
				Event.Raise (Stopped, this, EventArgs.Empty);
				State = State.Stopped;
			}
		}

		public override string ToString ()
		{
			return Torrent.Name;
		}

	}
}
