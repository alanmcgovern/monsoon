using System;
using MonoTorrent.Common;
using System.Collections.Generic;

namespace Monsoon
{
	public class LabelController : IService
	{
		private static NLog.Logger logger = MainClass.DebugEnabled ? NLog.LogManager.GetCurrentClassLogger () : new EmptyLogger ();
		
		public event EventHandler<LabelEventArgs> Added;
		public event EventHandler<LabelEventArgs> Removed;
		public event EventHandler SelectionChanged;

		private TorrentLabel selection;
		
		public TorrentLabel All {
			get; private set;
		}
		
		public TorrentLabel Delete {
			get; private set;
		}
		
		public TorrentLabel Downloading {
			get; private set;
		}

		public bool Initialised {
			get; private set;
		}
		
		public List<TorrentLabel> Labels {
			get; private set;
		}
		
		public TorrentLabel Seeding {
			get; private set;
		}

		public TorrentLabel Selection {
			get {  return selection; }
			set { selection = value; Event.Raise (SelectionChanged, this, EventArgs.Empty); }
		}

		public LabelController()
		{
			All = new TorrentLabel (_("All"), "gtk-home", true);
			Delete = new TorrentLabel (_("Remove"), "gtk-remove", true);
			Downloading = new TorrentLabel (_("Downloading"), "gtk-go-down", true);
			Seeding = new TorrentLabel (_("Seeding"), "gtk-go-up", true);
			Selection = All;
			
			Labels = new List<TorrentLabel> { All, Downloading, Seeding };
			HookEvents ();
		}
		
		void HookEvents ()
		{
			var torrentController = ServiceManager.Get <TorrentController> ();
			
			torrentController.Added += delegate(object sender, DownloadAddedEventArgs e) {
				All.AddTorrent(e.Download);
				e.Download.StateChanged += HandleStateChanged;
			};
			
			torrentController.Removed += delegate(object sender, DownloadAddedEventArgs e) {
				All.RemoveTorrent(e.Download);
				e.Download.StateChanged -= HandleStateChanged;
				
				foreach(TorrentLabel label in Labels)
					label.RemoveTorrent(e.Download);
			};
		}
		
		void HandleStateChanged (object sender, StateChangedEventArgs args)
		{
			Download manager = (Download) sender;
			if (args.OldState == State.Downloading) {
				logger.Debug("Removing " + manager.Torrent.Name + " from download label");
				Downloading.RemoveTorrent(manager);
			} else if (args.OldState == State.Seeding) {
				logger.Debug("Removing " + manager.Torrent.Name + " from upload label");
				Seeding.RemoveTorrent(manager);
			}
			
			if (args.NewState == State.Downloading) {
				logger.Debug("Adding " + manager.Torrent.Name + " to download label");
				Downloading.AddTorrent(manager);
			} else if (args.NewState == State.Seeding) {
				logger.Debug("Adding " + manager.Torrent.Name + " to upload label");
				Seeding.AddTorrent(manager);
			}
		}
		public void Add (TorrentLabel label)
		{
			Labels.Add (label);
			Event.Raise <LabelEventArgs> (Added, this, new LabelEventArgs (label)); 
		}

		public void Initialise ()
		{
			Initialised = true;
		}
		
		public void Remove (TorrentLabel label)
		{
			if (Labels.Remove (label))
				Event.Raise <LabelEventArgs> (Removed, this, new LabelEventArgs (label)); 
		}
		
		public void Restore ()
		{
			List <TorrentLabel> labels = new List<TorrentLabel> ();
			SettingsManager.Restore <List <TorrentLabel>> (labels);
			labels.ForEach (Add);
		}
		
		public void Store ()
		{
			List <TorrentLabel> labels = new List<TorrentLabel> ();
			
			foreach (TorrentLabel label in Labels) {
				if (label.Immutable)
					continue;
				labels.Add(label);
			}
			
			SettingsManager.Store <List <TorrentLabel>> (labels);
		}
		
		static string _(string s)
		{
			return s;
		}
	}
}
