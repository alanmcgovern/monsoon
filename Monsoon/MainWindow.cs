//
// MainWindow.cs
//
// Author:
//   Jared Hendry (buchan@gmail.com)
//   Mirco Bauer  (meebey@meebey.net)
//
// Copyright (C) 2007 Jared Hendry
// Copyright (C) 2008 Mirco Bauer
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Monsoon;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Text;

using Gtk;
using MonoTorrent.Common;
using MonoTorrent.Client;
using MonoTorrent.BEncoding;
using MonoTorrent.TorrentWatcher;


namespace Monsoon
{
	public partial class MainWindow: Gtk.Window
	{
		
		private static NLog.Logger logger = MainClass.DebugEnabled ? NLog.LogManager.GetCurrentClassLogger () : new EmptyLogger ();
		
		private LabelTreeView labelTreeView;
		public LabelController LabelController {
			get; private set;
		}

		private Button statusDownButton;
		private Button statusUpButton;
		
		private	TorrentTreeView torrentTreeView;
		private TreeSelection torrentsSelected;
		private ListStore torrentListStore;
		private Dictionary<Download, TreeIter> torrents;
		
		private TorrentController torrentController;

		private EngineSettings engineSettings;
		private TorrentSettings defaultTorrentSettings;
		private PreferencesSettings prefSettings;
		private InterfaceSettings interfaceSettings;
		
		private PeerTreeView peerTreeView;
		private ListStore peerListStore;
		private TreeModelFilter peerFilter;
		
		private FileTreeView fileTreeView;
		private TorrentFileModel fileTreeStore;
		
		private PiecesTreeView piecesTreeView;
		private ListStore piecesListStore;
		
		private ListenPortController portController;
		
		private TorrentFolderWatcher folderWatcher;
		
		private Menu trayMenu;
		//private ImageMenuItem quitItem;
		private Egg.TrayIcon trayIcon;
		
		private RssManagerController rssManagerController;
		
		internal ListStore PeerListStore
		{
			get { return peerListStore; }
		}

		public GconfSettingsStorage SettingsStorage {
			get {
				return GconfSettingsStorage.Instance;
			}
		}
		
		public InterfaceSettings InterfaceSettings {
			get { return interfaceSettings; }
		}

		public EngineSettings EngineSettings {
			get {
				return engineSettings;
			}
		}

		public PreferencesSettings Preferences {
			get {
				return prefSettings;
			}
		}
		
		public TorrentController TorrentController {
			get {
				return torrentController;
			}
		}

		public TorrentSettings DefaultTorrentSettings {
			get {
				return defaultTorrentSettings;
			}
		}

		public ListStore TorrentListStore {
			get {
				return torrentListStore;
			}
		}

		public Dictionary<Download, TreeIter> Torrents {
			get {
				return torrents;
			}
		}

		public MainWindow (EngineSettings engineSettings, ListenPortController portController): base (Gtk.WindowType.Toplevel)
		{
			this.engineSettings = engineSettings;
			this.portController = portController;
            
			interfaceSettings = new InterfaceSettings ();
			defaultTorrentSettings = new  TorrentSettings ();
			prefSettings = new PreferencesSettings ();
			
			Ticker.Tick ();
			LoadAllSettings ();
			Ticker.Tock ("Loaded all settings: {0}");

			torrents = new Dictionary<Download,Gtk.TreeIter> ();
			
			Ticker.Tick ();
			Ticker.Tick ();
			
			Build ();
			
			InitNatStatus ();
			
			Ticker.Tock ("Build");
			Ticker.Tick();
			BuildStatusBar();
			Ticker.Tock ("Status bar");			
			Ticker.Tick ();
			BuildTray();
			Ticker.Tock ("Tray");
			Ticker.Tick ();
			BuildPiecesTreeView();
			Ticker.Tock ("PiecesTreeview");
			Ticker.Tick ();
			BuildTorrentTreeView();
			Ticker.Tock ("TorrentTreeview");
			Ticker.Tick ();
			BuildPeerTreeView();
			Ticker.Tock ("PeerTreeview");
			Ticker.Tick ();
			BuildFileTreeView();
			Ticker.Tock ("FileTreeview");
			Ticker.Tick ();
			BuildLabelTreeView();
			Ticker.Tock ("Label treeview");
			Ticker.Tick ();
			BuildOptionsPage();
			Ticker.Tock ("Options page");
			Ticker.Tick ();
			BuildSpeedsPopup();
			Ticker.Tock ("Speeds popup");
			Ticker.Tock ("Built all stuff");		
			
			GLib.Timeout.Add (1000, new GLib.TimeoutHandler (updateView));
            
			Ticker.Tick ();
			RestoreInterfaceSettings ();
			Ticker.Tock ("Restored Interface");
			
			if (Preferences.UpnpEnabled)
				portController.Start();
			
			Ticker.Tick ();
			try{
				torrentController.LoadStoredTorrents ();
			}catch (Exception ex) {
				Console.WriteLine (ex);
				Environment.Exit(414);
			}
			Ticker.Tock ("Loaded torrents");
			
			Ticker.Tick ();
			logger.Info ("Restoring labels");
			LabelController.Restore ();
			// Restore previously labeled torrents
			foreach (Download download in torrentController.Torrents){
				foreach (TorrentLabel l in LabelController.Labels) {
					if (l.TruePaths == null)
						continue;
					
					if (Array.IndexOf <string> (l.TruePaths, download.Manager.Torrent.TorrentPath) < 0)
						continue;
					
					l.AddTorrent (download);
				}
			}
			Ticker.Tock ("Restored labels");
			
			folderWatcher = new TorrentFolderWatcher (new DirectoryInfo (Preferences.ImportLocation));
			folderWatcher.TorrentFound += delegate(object o, TorrentWatcherEventArgs e) {
				GLib.Idle.Add(Event.Wrap ((GLib.IdleHandler) delegate {
					TorrentFound (o, e);
					return false;
				}));
			};
			
			if (Preferences.ImportEnabled) {
				logger.Info ("Starting import folder watcher");
				folderWatcher.Start ();
			}
			
			logger.Info ("Starting RSS manager");
			rssManagerController = new RssManagerController(EngineSettings);
			rssManagerController.TorrentFound += delegate(object sender, TorrentRssWatcherEventArgs e) {
				string savePath = e.Filter == null ? EngineSettings.SavePath : e.Filter.SavePath;
				try {
					LoadTorrent(e.Item.Link, true, false, false, null, savePath, true);
				} catch {
					logger.Error("RSS Manager: Unable to add - " + e.Item.Title);
				}
			};
			logger.Info ("Started RSS manager");
			rssManagerController.StartWatchers();
			ShowAll();
		}
		
		void TorrentFound (object sender, TorrentWatcherEventArgs args)
		{
			if(!prefSettings.ImportEnabled)
				return;

			logger.Info("New torrent detected, adding " + args.TorrentPath);
			GLib.Timeout.Add (1000, delegate {
				LoadTorrent (args.TorrentPath, false);
				return false;
			});
		}
		
		void InitNatStatus ()
		{
			// Monodevelop keeps breaking and repeatedly sets the 'natStatus' variable to null
			// whenever i rebuild the project files. Sometimes it works, sometimes it fails.
			// Work around the bug by setting it in code
			if (natStatus == null) {
				hbox4.Remove (natStatus);
				natStatus = new NatWidget ();
				natStatus.WidthRequest = 28;
				natStatus.Name = "natStatus";
				hbox4.PackEnd (natStatus);
			}
			
			portController.PortMapped += Event.Wrap ((EventHandler) delegate {
				GLib.Idle.Add(delegate {
					natStatus.PortForwarded = true;
					return false;
				});
			});

			portController.RouterFound += Event.Wrap ((EventHandler) delegate {
				GLib.Idle.Add(delegate {
					natStatus.RouterFound = true;
					return false;
				});
			});
		}
		
		public Egg.TrayIcon TrayIcon {
			get { return trayIcon; }
		}
		
		private void BuildTray()
		{
			EventBox eventBox = new EventBox ();
			trayMenu = new Menu ();
			
			ImageMenuItem quitItem = new ImageMenuItem (_("Quit"));
			quitItem.Image = new Image (Stock.Quit, IconSize.Menu);
			quitItem.Activated += Event.Wrap (delegate(object sender, EventArgs args) {
				DeleteEventHandler h = OnDeleteEvent;
				h (sender ,new DeleteEventArgs ());
			});
			
			ImageMenuItem stop = new ImageMenuItem (_("Stop All"));
			stop.Image = new Image (Stock.MediaStop, IconSize.Menu);
			stop.Activated += Event.Wrap ((EventHandler) delegate {
				foreach (Download m in torrentController.Torrents)
					m.Stop ();
			});
			
			ImageMenuItem start = new ImageMenuItem (_("Start All"));
			start.Image = new Image (Stock.MediaPlay, IconSize.Menu);
			start.Activated += Event.Wrap ((EventHandler) delegate {
				foreach (Download m in torrentController.Torrents)
					m.Start ();
			});

			CheckMenuItem notifications = new CheckMenuItem (_("Show Notifications"));
			notifications.Active = Preferences.EnableNotifications;
			notifications.Activated += Event.Wrap ((EventHandler) delegate {
				Preferences.EnableNotifications = !Preferences.EnableNotifications; 
			});
			
			trayMenu.Append (start);
			trayMenu.Append (stop);
			trayMenu.Append (new SeparatorMenuItem());
			trayMenu.Append (notifications);
			trayMenu.Append (new SeparatorMenuItem());
			trayMenu.Append (quitItem);
			
			eventBox.Add (new Image (Stock.GoDown, IconSize.Menu));
			eventBox.ButtonPressEvent += OnTrayClicked;
			trayIcon = new Egg.TrayIcon (Defines.ApplicationName);
			trayIcon.Icon = new Image (Stock.Network, IconSize.Menu).Pixbuf;
			trayIcon.Add (eventBox);
			
			Tooltips trayTip = new Tooltips();
			
			trayIcon.EnterNotifyEvent += Event.Wrap ((EnterNotifyEventHandler) delegate { 
				trayTip.SetTip(trayIcon, "Monsoon - D: " + ByteConverter.ConvertSpeed(torrentController.Engine.TotalDownloadSpeed) +
				               " U: " + ByteConverter.ConvertSpeed(torrentController.Engine.TotalUploadSpeed), null);
			});
			
			if (this.prefSettings.EnableTray)
				trayIcon.ShowAll ();
		}
		
		private void OnTrayClicked(object sender, ButtonPressEventArgs args)
		{
			Gdk.EventButton eventButton = args.Event;
			
			if(eventButton.Button == 1){
				if (Visible) {
					int x, y;
					GetPosition (out x, out y);
					interfaceSettings.WindowXPos = x;
					interfaceSettings.WindowYPos = y;
					Hide();
				} else {
					Show();
					Move (interfaceSettings.WindowXPos, interfaceSettings.WindowYPos);
				}	
			}
			
			if(eventButton.Button == 3){
				// show context menu
				trayMenu.ShowAll ();
				trayMenu.Popup();
			}
		}
		
		private void LoadAllSettings ()
		{
			try
			{
				SettingsManager.Restore <InterfaceSettings> (InterfaceSettings);
			}
			catch (Exception ex)
			{
				logger.Error ("Couldn't load interface settings: {0}", ex.Message);
			}
			
			try	
			{
				SettingsManager.Restore <PreferencesSettings> (Preferences);
			}
			catch (Exception ex)
			{
				logger.Error("Could not load preferences: {0}", ex);
			}
			
			try	
			{
				SettingsManager.Restore <TorrentSettings> (DefaultTorrentSettings);
			}
			catch (Exception ex)
			{
				logger.Error("Could not load default torrent settings: {0}", ex);
			}
		}
		
		private void RestoreInterfaceSettings ()
		{
			InterfaceSettings settings = interfaceSettings;
			
			logger.Info ("Restoring interface settings");
			SetDefaultSize (settings.WindowWidth, settings.WindowHeight);
			
			if (settings.WindowYPos == 0 && settings.WindowXPos == 0)
				SetPosition (WindowPosition.Center);
			else
				Move (settings.WindowXPos, settings.WindowYPos);
			
			vPaned.Position = settings.VPaned;
			hPaned.Position = settings.HPaned;
			
			ShowDetailedInfo.Active = settings.ShowDetails;
			ShowLabels.Active = settings.ShowLabels;
			labelViewScrolledWindow.Visible = settings.ShowLabels;
			detailNotebook.Visible = settings.ShowDetails;
			
			// Restore columns
			torrentTreeView.nameColumn.FixedWidth = settings.NameColumnWidth;
			torrentTreeView.doneColumn.FixedWidth = settings.DoneColumnWidth;
			torrentTreeView.statusColumn.FixedWidth = settings.StatusColumnWidth;
			torrentTreeView.seedsColumn.FixedWidth = settings.SeedsColumnWidth;
			torrentTreeView.peersColumn.FixedWidth = settings.PeersColumnWidth;
			torrentTreeView.downSpeedColumn.FixedWidth = settings.DlSpeedColumnWidth;
			torrentTreeView.upSpeedColumn.FixedWidth = settings.UpSpeedColumnWidth;
			torrentTreeView.ratioColumn.FixedWidth = settings.RatioColumnWidth;
			torrentTreeView.sizeColumn.FixedWidth = settings.SizeColumnWidth;
			torrentTreeView.etaColumn.FixedWidth = settings.EtaColumnWidth; 
			
			torrentTreeView.nameColumn.Visible = settings.NameColumnVisible;
			torrentTreeView.doneColumn.Visible = settings.DoneColumnVisible;
			torrentTreeView.statusColumn.Visible = settings.StatusColumnVisible;
			torrentTreeView.seedsColumn.Visible = settings.SeedsColumnVisible;
			torrentTreeView.peersColumn.Visible = settings.PeersColumnVisible;
			torrentTreeView.downSpeedColumn.Visible = settings.DlSpeedColumnVisible;
			torrentTreeView.upSpeedColumn.Visible = settings.UpSpeedColumnVisible;
			torrentTreeView.ratioColumn.Visible = settings.RatioColumnVisible;
			torrentTreeView.sizeColumn.Visible = settings.SizeColumnVisible;
			torrentTreeView.etaColumn.Visible = settings.EtaColumnVisible;
		}

		private void StoreInterfaceSettings ()
		{
			InterfaceSettings interfaceSettings = this.interfaceSettings;
			
			int w, h;
			int x, y;
		
			logger.Info ("Storing interface settings");
			
			GetSize (out w, out h);
			GetPosition (out x, out y);
			
			// Window
			interfaceSettings.WindowHeight = h;
			interfaceSettings.WindowWidth = w;
			interfaceSettings.VPaned = vPaned.Position;
			interfaceSettings.HPaned = hPaned.Position;
			interfaceSettings.WindowXPos = x;
			interfaceSettings.WindowYPos = y;
			
			interfaceSettings.ShowDetails = ShowDetailedInfo.Active;
			interfaceSettings.ShowLabels = ShowLabels.Active;
			
			
			// TorrentTreeView column's
			interfaceSettings.NameColumnWidth = torrentTreeView.nameColumn.Width;
			interfaceSettings.NameColumnVisible = torrentTreeView.nameColumn.Visible;
			interfaceSettings.StatusColumnWidth = torrentTreeView.statusColumn.Width;
			interfaceSettings.StatusColumnVisible = torrentTreeView.statusColumn.Visible;
			interfaceSettings.DoneColumnWidth = torrentTreeView.doneColumn.Width;
			interfaceSettings.DoneColumnVisible = torrentTreeView.doneColumn.Visible;
			interfaceSettings.SeedsColumnWidth = torrentTreeView.seedsColumn.Width;
			interfaceSettings.SeedsColumnVisible = torrentTreeView.seedsColumn.Visible;
			interfaceSettings.PeersColumnWidth = torrentTreeView.peersColumn.Width;
			interfaceSettings.PeersColumnVisible = torrentTreeView.peersColumn.Visible;
			interfaceSettings.DlSpeedColumnWidth = torrentTreeView.downSpeedColumn.Width;
			interfaceSettings.DlSpeedColumnVisible = torrentTreeView.downSpeedColumn.Visible;
			interfaceSettings.UpSpeedColumnWidth = torrentTreeView.upSpeedColumn.Width;
			interfaceSettings.UpSpeedColumnVisible = torrentTreeView.upSpeedColumn.Visible;
			interfaceSettings.RatioColumnWidth = torrentTreeView.ratioColumn.Width;
			interfaceSettings.RatioColumnVisible = torrentTreeView.ratioColumn.Visible;
			interfaceSettings.SizeColumnWidth = torrentTreeView.sizeColumn.Width;
			interfaceSettings.SizeColumnVisible = torrentTreeView.sizeColumn.Visible;
			interfaceSettings.EtaColumnWidth = torrentTreeView.etaColumn.Width;
			interfaceSettings.EtaColumnVisible = torrentTreeView.etaColumn.Visible;
			
			SettingsManager.Store <InterfaceSettings> (InterfaceSettings);
		}
		
		private void BuildStatusBar()
		{
			statusToolbar.ShowArrow = false;	
			statusToolbar.ToolbarStyle = ToolbarStyle.BothHoriz;
			
			// Empty expanded ToolItem to fake right aligning items
			Label fillerLabel = new Label();
            ToolItem fillerItem = new ToolItem();
			fillerItem.Add(fillerLabel);
			fillerItem.Expand = true;
			
            statusDownButton = new Button ();
			statusDownButton.Relief = ReliefStyle.None;
			statusDownButton.Image = Gtk.Image.NewFromIconName(Gtk.Stock.GoDown, IconSize.Menu);
			
            statusUpButton = new Button ();
			statusUpButton.Relief = ReliefStyle.None;
			statusUpButton.Image = Gtk.Image.NewFromIconName(Gtk.Stock.GoUp, IconSize.Menu);
			
			ToolItem uploadItem = new ToolItem();
			uploadItem.Add(statusUpButton);
			ToolItem downItem = new ToolItem();
			downItem.Add(statusDownButton);
			
			statusToolbar.Insert(fillerItem, 0);
			statusToolbar.Insert(downItem, 1);			
			statusToolbar.Insert(uploadItem, 2);			

            statusToolbar.ShowAll ();
		}
		
		private void BuildFileTreeView ()
		{
			fileTreeStore = new TorrentFileModel();
			fileTreeView = new FileTreeView (fileTreeStore);
			filesScrolledWindow.Add (fileTreeView);
			//fileTreeView.Show();
		}
		
		private void BuildTorrentTreeView ()
		{
			torrentListStore = new ListStore (typeof(Download));
			torrentController = new TorrentController (DefaultTorrentSettings, EngineSettings, Preferences);
			torrentController.Added += delegate(object sender, DownloadAddedEventArgs e) {
				Torrents.Add (e.Download, TorrentListStore.AppendValues(e.Download));
				LabelController.All.AddTorrent(e.Download);
				StoreTorrentSettings ();
				
				e.Download.StateChanged += HandleStateChanged;
			};
			TorrentController.Removed += delegate(object sender, DownloadAddedEventArgs e) {
				Download torrent = e.Download;
				TreeIter iter = Torrents [torrent];
				TorrentListStore.Remove(ref iter);
				Torrents.Remove(torrent);

				LabelController.All.RemoveTorrent(torrent);
				foreach(TorrentLabel label in LabelController.Labels){
					label.RemoveTorrent(torrent);
				}
				
				StoreTorrentSettings();
			};
			torrentController.ShouldAdd += HandleShouldAdd;
			torrentTreeView = new TorrentTreeView (torrentController);
			torrentTreeView.DragDataReceived += TreeviewDragDataReceived;
			torrentTreeView.DeleteTorrent += Event.Wrap ((EventHandler) delegate { DeleteAndRemoveSelection (); });
			torrentTreeView.RemoveTorrent += Event.Wrap ((EventHandler) delegate { RemoveTorrent (); });
			//torrentTreeView.Model = torrentListStore;
			torrentTreeView.Selection.Changed += OnTorrentSelectionChanged;
			
			torrentViewScrolledWindow.Add (torrentTreeView);
			//torrentTreeView.Show ();
		}

		void HandleShouldAdd(object sender, ShouldAddEventArgs e)
		{
			LoadTorrentDialog dialog = new LoadTorrentDialog(e.Torrent, e.SavePath);
			dialog.AlwaysAsk = interfaceSettings.ShowLoadDialog;
			
			try
			{
				e.ShouldAdd = dialog.Run () == (int)ResponseType.Ok;
				interfaceSettings.ShowLoadDialog = dialog.AlwaysAsk;
			}
			finally
			{
				dialog.Destroy ();
			}
		}
		
		private void TreeviewDragDataReceived (object o, DragDataReceivedArgs args) 
		{
			string [] uriList = (Encoding.UTF8.GetString(args.SelectionData.Data).TrimEnd()).Split('\n');
			
			foreach(string s in uriList){
				try
				{
					Uri uri = new Uri(s.TrimEnd());
					if (uri.IsFile && uri.LocalPath.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase))
						LoadTorrent(uri.LocalPath);
				}
				catch
				{
				}
			}
		}

		void HandleStateChanged(object sender, TorrentStateChangedEventArgs args)
		{
			Download manager = (Download) sender;
			if (args.OldState == TorrentState.Downloading) {
				logger.Debug("Removing " + manager.Torrent.Name + " from download label");
				LabelController.Downloading.RemoveTorrent(manager);
			} else if (args.OldState == TorrentState.Seeding) {
				logger.Debug("Removing " + manager.Torrent.Name + " from upload label");
				LabelController.Seeding.RemoveTorrent(manager);
			}
			
			if (args.NewState == TorrentState.Downloading) {
				logger.Debug("Adding " + manager.Torrent.Name + " to download label");
				LabelController.Downloading.AddTorrent(manager);
			} else if (args.NewState == TorrentState.Seeding) {
				logger.Debug("Adding " + manager.Torrent.Name + " to upload label");
				LabelController.Seeding.AddTorrent(manager);
			} else if (args.NewState == TorrentState.Stopped) {
				PeerListStore.Clear ();
			}
		
			if (!Preferences.EnableNotifications)
				return;
			if (args.NewState != TorrentState.Seeding)
				return;
			if (args.OldState != TorrentState.Downloading)
				return;

			Notifications.Notification notify = new Notifications.Notification (_("Download Complete"), manager.Torrent.Name, Stock.GoDown);
			if (Preferences.EnableTray)
				notify.AttachToWidget (TrayIcon);
			notify.Urgency = Notifications.Urgency.Low;
			notify.Timeout = 5000;
			notify.Show ();
			notify.AddAction("reveal-item", "Show", delegate {
				System.Diagnostics.Process.Start("\"file://" + manager.SavePath + "\"");
			});
		}

		private void BuildLabelTreeView()
		{
			/* Move some stuff to LabelTreeView */
			LabelController = new LabelController ();
			labelTreeView = new LabelTreeView (LabelController, true);
			
			labelTreeView.Selection.Changed += OnLabelSelectionChanged;
			labelViewScrolledWindow.Add (labelTreeView);

			torrentTreeView.Model = torrentListStore;

			TargetEntry [] targetEntries = new TargetEntry[]{
				new TargetEntry("application/x-monotorrent-Download-objects", 0, 0)
			};

			torrentTreeView.DragBegin += Event.Wrap ((DragBeginHandler) delegate {
				TreeIter it;
				if (!labelTreeView.Selection.GetSelected (out it))
					return;
				
				TorrentLabel label = (TorrentLabel) labelTreeView.Model.GetValue (it, 0);
				if (!label.Immutable)
					labelTreeView.Model.AppendValues(LabelController.Delete); 
			});
			
			torrentTreeView.DragEnd += Event.Wrap ((DragEndHandler) delegate {
				TreeIter iter;
				if (!labelTreeView.Model.GetIterFirst (out iter))
					return;
				
				TreeIter prev = iter;
				while (labelTreeView.Model.IterNext(ref iter))
					prev = iter;
				
				TorrentLabel label = (TorrentLabel) labelTreeView.Model.GetValue (prev, 0);
				if (label == LabelController.Delete)
					labelTreeView.Model.Remove (ref prev);
			});
			
			labelTreeView.EnableModelDragDest(targetEntries, Gdk.DragAction.Copy);
			labelTreeView.DragDataReceived += OnTorrentDragDataReceived;
		}
		
		private void OnTorrentDragDataReceived(object sender, DragDataReceivedArgs args)
		{
			TreePath path;
			TreeViewDropPosition pos;
			TreeIter iter;
			TorrentLabel label;
			
			if(!labelTreeView.GetDestRowAtPos(args.X, args.Y, out path, out pos))
				return;
			if(!labelTreeView.Model.GetIter(out iter, path))
				return;
				
			label = (TorrentLabel) labelTreeView.Model.GetValue(iter, 0);
			if(label == LabelController.All || label == LabelController.Downloading || label == LabelController.Seeding)
				return;
			
			foreach (Download download in torrents.Keys)
			{
				TorrentManager manager = download.Manager;
				if(!Toolbox.ByteMatch (manager.Torrent.InfoHash, args.SelectionData.Data))
					continue;
				
				if (label != LabelController.Delete)
				{
					label.AddTorrent(download);
				}
				else
				{
					
					if (!labelTreeView.Selection.GetSelected (out iter))
						return;
					
					label = (TorrentLabel)labelTreeView.Model.GetValue (iter, 0);
					label.RemoveTorrent (download);
				}
			}
			
		}
		
		private void BuildPiecesTreeView()
		{
			piecesListStore = new ListStore (typeof(Piece));
			piecesTreeView = new PiecesTreeView ();
			piecesTreeView.Model = piecesListStore;
			piecesScrolledWindow.Add(piecesTreeView);
			//piecesScrolledWindow.ShowAll();
		}
		
		/*
		private bool FilterTorrentTree (TreeModel model, TreeIter iter)
		{
			TreeIter labelIter;
			
			if(!labelTreeView.Selection.GetSelected (out labelIter))
				return true;
	 		
	 		Download manager = (Download) model.GetValue (iter, 0);
	 		if(manager == null)
	 			return true;

	 		TorrentLabel label = (TorrentLabel) labelListStore.GetValue (labelIter, 0);
	 		
	 		if(label.torrents == null)
	 			return true;
	 		
	 		if(label.torrents.Contains (manager)){
	 			return true;
	 		}
	 		
	 		return false;
		}
		*/
		
		private void BuildPeerTreeView ()
		{
			peerTreeView = new PeerTreeView ();
			peerListStore = new ListStore (typeof(PeerId));
			
			peerFilter = new TreeModelFilter (peerListStore, null);
			peerFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterPeerTree);
			
			peerTreeView.Model = peerFilter;
			peersScrolledWindow.Add (peerTreeView);
			//peerTreeView.Show ();
		}

		private bool FilterPeerTree (Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			PeerId peer = (PeerId) model.GetValue(iter, 0);
			Download manager = GetSelectedTorrent();
			
			if(manager == null)
				return false;
				
			if(peer == null)
				return false;
			
			if(!peer.IsConnected)
				return false;
				
			if (peer.TorrentManager == manager.Manager)
				return true;
			
			return false;
		}
		
		private void OnTorrentSelectionChanged (object sender, System.EventArgs args)
		{
			torrentsSelected = (TreeSelection) sender;

			torrentController.SelectedDownload = GetSelectedTorrent ();
			updateView ();
			
			// Update Files Page
			updateFilesPage ();
			
			//update Options Page
			updateOptionsPage ();
			
			peerFilter.Refilter();
			
		}
		
		private void updateOptionsPage ()
		{
			Download download = null;
			TreePath [] treePaths;
			TreeModel model;
			treePaths = torrentsSelected.GetSelectedRows (out model);

			torrentUploadRateSpinButton.ValueChanged -= OnTorrentSettingsChanged;
			torrentDownloadRateSpinButton.ValueChanged -= OnTorrentSettingsChanged;
			torrentMaxConnectionsSpinButton.ValueChanged -= OnTorrentSettingsChanged;
			torrentUploadSlotSpinButton.ValueChanged -= OnTorrentSettingsChanged;	
			
			if(treePaths.Length != 1){
				torrentUploadRateSpinButton.Sensitive = false;
				torrentDownloadRateSpinButton.Sensitive = false;
				torrentMaxConnectionsSpinButton.Sensitive = false;
				torrentUploadSlotSpinButton.Sensitive = false;
				
				torrentUploadRateSpinButton.Value = 0;
				torrentDownloadRateSpinButton.Value = 0;
				torrentMaxConnectionsSpinButton.Value = 0;
				torrentUploadSlotSpinButton.Value = 0;
			
				return;
			}
			
			foreach(TreePath treePath in treePaths){
				TreeIter torrentIter;
				model.GetIter (out torrentIter, treePath);
				
				download = (Download) model.GetValue (torrentIter,0);	
			}
			
			TorrentManager torrent = download.Manager;
			
			torrentUploadRateSpinButton.Sensitive = true;
			torrentDownloadRateSpinButton.Sensitive = true;
			torrentMaxConnectionsSpinButton.Sensitive = true;
			torrentUploadSlotSpinButton.Sensitive = true;
			
			// Load selected torrents's settings
			torrentUploadRateSpinButton.Value = torrent.Settings.MaxUploadSpeed / 1024;;
			torrentDownloadRateSpinButton.Value = torrent.Settings.MaxDownloadSpeed / 1024;
			torrentMaxConnectionsSpinButton.Value = torrent.Settings.MaxConnections;
			torrentUploadSlotSpinButton.Value = torrent.Settings.UploadSlots;
			
			torrentUploadRateSpinButton.ValueChanged += OnTorrentSettingsChanged;
			torrentDownloadRateSpinButton.ValueChanged += OnTorrentSettingsChanged;
			torrentMaxConnectionsSpinButton.ValueChanged += OnTorrentSettingsChanged;
			torrentUploadSlotSpinButton.ValueChanged += OnTorrentSettingsChanged;	
			
		}
		
		private void OnLabelSelectionChanged (object sender, System.EventArgs e)
		{
			
			TreeIter iter;
			TorrentLabel label;
			
			TreeSelection selection = (TreeSelection) sender;
			if(!selection.GetSelected(out iter)){
				torrentTreeView.Model = torrentListStore;	
				return;
			}
			label = (TorrentLabel) labelTreeView.Model.GetValue(iter, 0);
			torrentTreeView.Model = label.Model;
			logger.Debug("Label " + label.Name + " selected." );
			
			//torrentTreeView.Selection.UnselectAll();
			//torrentFilter.Refilter();
		}

        private void updatePeersPage()
        {
			peerListStore.Clear ();
			foreach (Download manager in torrentController.Torrents) {
				foreach (PeerId peer in manager.GetPeers ()) {
					PeerListStore.AppendValues(peer);
				}
			}
        }

		private void updateStatusBar()
		{
			string limited;
			if (engineSettings.GlobalMaxDownloadSpeed == 0)
				limited = "";
			else
				limited = "[" + ByteConverter.ConvertSpeed (engineSettings.GlobalMaxDownloadSpeed) + "]";
			
			statusDownButton.Label = string.Format("{0}{1}", limited,
			                                           ByteConverter.ConvertSpeed(torrentController.Engine.TotalDownloadSpeed));
			
			if (engineSettings.GlobalMaxUploadSpeed == 0)
				limited = "";
			else
				limited = string.Format("[{0}]", ByteConverter.ConvertSpeed (engineSettings.GlobalMaxUploadSpeed));
			
			statusUpButton.Label = string.Format("{0}{1}", limited, 
			                                         ByteConverter.ConvertSpeed (torrentController.Engine.TotalUploadSpeed));
		}
		
		
		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Hide ();
			
			if(Preferences.QuitOnClose && sender == this){
				a.RetVal = true;
				return;
			}
			
			// This should be stored before the torrent is stopped
			StoreTorrentSettings ();
			
			foreach (WaitHandle h in this.torrentController.Engine.StopAll())
				h.WaitOne (TimeSpan.FromSeconds(2), false);	

//			List<WaitHandle> handles = new List<WaitHandle> ();
//			foreach (Download manager in torrents.Keys){
//				if(manager.State == TorrentState.Stopped)
//					continue;
//				try{
//					handles.Add (manager.Stop ());
//				}
//				catch{
//					logger.Error ("Cannot stop torrent " + manager.Torrent.Name);
//				}	
//			}
			
			StoreInterfaceSettings ();
			logger.Info ("Storing labels");
			LabelController.Store ();
			rssManagerController.Store();
			torrentController.StoreFastResume ();
			
//			foreach (WaitHandle h in handles)
//				h.WaitOne(TimeSpan.FromSeconds(1.5), false);
			
			Application.Quit ();
			this.torrentController.Engine.Dispose();						
			a.RetVal = true;
		}
		
		public void StoreTorrentSettings ()
		{
			List<TorrentStorage> torrents = new List<TorrentStorage> ();

			logger.Info ("Storing torrent settings");

			foreach (Download download in this.torrents.Keys){
				TorrentManager manager = download.Manager;
				TorrentStorage torrentToStore = new TorrentStorage();
				torrentToStore.TorrentPath = manager.Torrent.TorrentPath;
				torrentToStore.SavePath = manager.SavePath;
				torrentToStore.Settings = manager.Settings;
				torrentToStore.State = manager.State;
				torrentToStore.UploadedData = download.TotalUploaded;
				torrentToStore.DownloadedData = download.TotalDownloaded;
				torrentToStore.InfoHash = Convert.ToString(manager.GetHashCode());
				foreach(TorrentFile file in manager.FileManager.Files) {
					TorrentFileSettingsModel fileSettings = new TorrentFileSettingsModel();
					fileSettings.Path = file.Path;
					fileSettings.Priority = file.Priority;
					torrentToStore.Files.Add(fileSettings);
				}
				torrents.Add(torrentToStore);	
			}
			SettingsManager.Store <List <TorrentStorage>> (torrents);
		}

		protected virtual void OnAboutActivated (object sender, System.EventArgs e)
		{
			Monsoon.AboutDialog aboutDialog = new Monsoon.AboutDialog ();
			aboutDialog.Run ();
			aboutDialog.Destroy ();
		}

		protected virtual void OnQuitActivated (object sender, System.EventArgs e)
		{
			OnDeleteEvent (null,new DeleteEventArgs ());
		}

		public void Stop()
		{			
			OnDeleteEvent (null,new DeleteEventArgs ());
		}
		
		protected virtual void OnPreferencesActivated (object sender, System.EventArgs e)
		{
			try {
				logger.Debug("OnPreferencesActivated()");
				
				OpenPreferences();
			} catch (Exception ex) {
				UnhandledExceptionDialog d = new UnhandledExceptionDialog(ex);
				d.Run();
			}
		}

		public void OpenPreferences()
		{
			OpenPreferences(0);
		}

		public void OpenPreferences(int pageIndex)
		{
			PreferencesDialog preferencesDialog = new PreferencesDialog (this);
			logger.Debug("1");
			preferencesDialog.SetPageIndex(pageIndex);
			preferencesDialog.Run ();
			preferencesDialog.Destroy ();
			
			SettingsManager.Store <TorrentSettings> (DefaultTorrentSettings);
			SettingsManager.Store <PreferencesSettings> (Preferences);
			
			if (Preferences.ImportEnabled) {
				logger.Info ("Starting import folder watcher");
				folderWatcher.Start ();
			} else {
				// If it hasn't been started before, running stop will crash it -- fix in library?
				folderWatcher.Start ();
				logger.Info ("Stoping import folder watcher");
				folderWatcher.Stop ();
			}
			
			if (Preferences.UpnpEnabled) {
				if (!portController.IsRunning) {
					portController.Start ();
				}
				else if (portController.MappedPort != engineSettings.ListenPort) {
					portController.ChangePort ();
				} else {
					portController.MapPort ();
				}
			} else if (!Preferences.UpnpEnabled && portController.IsRunning) {
				portController.RemoveMap();
			}
		}

		protected virtual void OnOpenActivated (object sender, System.EventArgs e)
		{
			FileFilter torrentFilter = new FileFilter ();
			FileFilter allFilter = new FileFilter ();
			FileChooserDialog fileChooser = new FileChooserDialog (_("Open torrent(s)"), this, FileChooserAction.Open, Gtk.Stock.Cancel, ResponseType.Cancel, Gtk.Stock.Open, ResponseType.Accept);
			
			torrentFilter.Name = _("Torrent files");
			torrentFilter.AddPattern ("*.torrent");
			
			allFilter.Name = _("All files");
			allFilter.AddPattern ("*.*");
			
			fileChooser.AddFilter (torrentFilter);
			fileChooser.AddFilter (allFilter);
			
			fileChooser.SelectMultiple=true;
			ResponseType result = (ResponseType) fileChooser.Run ();
			fileChooser.Hide ();
			
			if (result == ResponseType.Accept) {
				logger.Debug ("Open torrent dialog response recieved");
				foreach (String fileName in fileChooser.Filenames)
					LoadTorrent (fileName);
			}
			
			fileChooser.Destroy ();
		}
		
		private bool updateView ()
		{
			TreeIter iter;
			
			// Update toolbar
			updateToolBar ();
				
			// Update TreeView
			if (torrentTreeView.Model != null && torrentTreeView.Model.GetIterFirst (out iter)) {
				do {
					torrentTreeView.Model.EmitRowChanged (torrentTreeView.Model.GetPath (iter), iter);
				} while (torrentTreeView.Model.IterNext (ref iter));
			}
			
			// Update General Page
			updateGeneralPage ();
			
			// Update Labels
			updateLabels ();
			
			// Update peers
			updatePeersPage ();
					
			// Update status bar
			updateStatusBar ();
			
			// update pieces
			updatePiecesPage ();
					
			return true;
		}
        
		private void updateFilesPage ()
		{
			this.fileTreeStore.Update (GetSelectedTorrent ());
		}
		
		private void updatePiecesPage()
		{
			piecesListStore.Clear ();
		
			Download manager = GetSelectedTorrent ();
			if(manager == null)
				return;

			List<Piece> pieces = manager.Manager.GetActiveRequests ();
			pieces.Sort ();
			
			foreach (Piece piece in pieces)
				piecesListStore.AppendValues(piece);
		}
		
		public Download GetSelectedTorrent ()
		{
			TreePath [] treePaths;
			TreeModel filteredModel;
			
			if(torrentsSelected == null || torrentsSelected.CountSelectedRows() != 1) 
				return null;
				
			treePaths = torrentsSelected.GetSelectedRows( out filteredModel);
			Download manager = null;
			
			// Should only be one item but have to use GetSelectedRows
			// because of TreeView is set to allow multiple selection
			TreeIter iter;
			filteredModel.GetIter (out iter, treePaths [0]);
			manager = (Download)filteredModel.GetValue(iter,0);	
			
			return manager;
		}
		
		private void updateLabels ()
		{
			TreeIter iter;
			TreeModel model = labelTreeView.Model;
			
			if (model.GetIterFirst (out iter)) {
				do {
					if (((TorrentLabel)model.GetValue (iter, 0)).Immutable)
						model.EmitRowChanged(model.GetPath(iter), iter);
				} while (model.IterNext(ref iter));
			}
		}
		
		private void updateGeneralPage ()
		{
			TreePath [] treePaths;
			
			TreeModel filteredModel;
			treePaths = torrentsSelected.GetSelectedRows (out filteredModel);
			
			Download download = null;
			
			if (treePaths.Length != 1) {
				statusProgressBar.Fraction = 0;
				
				downloadedValueLabel.Text = string.Empty;
				uploadedValueLabel.Text = string.Empty;
				elapsedTimeValueLabel.Text = string.Empty;
				trackerUrlValueLabel.Text = string.Empty;
				trackerStatusValueLabel.Text = string.Empty;
				
				swarmSpeedLabel.Text = string.Empty;
				hashFailsLabel.Text = string.Empty;
				savePathValueLabel.Text = string.Empty;
				sizeValueLabel.Text = string.Empty;
				createdOnValueLabel.Text = string.Empty;
				commentValueLabel.Text = string.Empty;
				lastUpdatedLabel.Text = string.Empty;
				updateInLabel.Text = string.Empty;
				messageLabel.Text = string.Empty;
				return;
			}
			
			foreach (TreePath treePath in treePaths) {
				TreeIter iter;
				filteredModel.GetIter (out iter, treePath);
				
				download = (Download) filteredModel.GetValue (iter,0);	
			}
			
			TorrentManager manager = download.Manager;
			
			statusProgressBar.Fraction = download.Progress;
			statusProgressBar.Text = string.Format("{0} {1:0.00}%", manager.State, download.Progress * 100);
			
			if (manager.State != TorrentState.Stopped)
				elapsedTimeValueLabel.Text = DateTime.MinValue.Add(DateTime.Now.Subtract(manager.StartTime)).ToString("HH:mm:ss");
			else
				elapsedTimeValueLabel.Text = null;
			
			downloadedValueLabel.Text = ByteConverter.ConvertSize (download.TotalDownloaded);
			uploadedValueLabel.Text = ByteConverter.ConvertSize (download.TotalUploaded);
			MonoTorrent.Client.Tracker.Tracker tracker = manager.TrackerManager.CurrentTracker;
			if (tracker == null)
			{
				trackerUrlValueLabel.Text = "";
				trackerStatusValueLabel.Text = "";
				lastUpdatedLabel.Text = "";
				messageLabel.Text = "";
			}
			
			else
			{
				trackerUrlValueLabel.Text = tracker.Uri.ToString ();
				trackerStatusValueLabel.Text = tracker.Status.ToString ();
				lastUpdatedLabel.Text = manager.TrackerManager.LastUpdated.ToString ("HH:mm:ss") ;
				messageLabel.Text = tracker.WarningMessage + ". " + tracker.FailureMessage;
			}
			hashFailsLabel.Text = manager.HashFails.ToString ();
			
			if (manager.State != TorrentState.Stopped){
				if (tracker != null)
				{
					DateTime nextUpdate = manager.TrackerManager.LastUpdated.Add (tracker.UpdateInterval);
				
					if(nextUpdate > DateTime.Now)
						updateInLabel.Text =  DateTime.MinValue.Add (nextUpdate - DateTime.Now).ToString("HH:mm:ss");
				}
			}
			
			swarmSpeedLabel.Text = ByteConverter.ConvertSpeed (torrentController.SelectedDownload.SwarmSpeed);
			savePathValueLabel.Text = manager.SavePath;
			sizeValueLabel.Text = ByteConverter.ConvertSize (manager.Torrent.Size);
			createdOnValueLabel.Text = manager.Torrent.CreationDate.ToLongDateString ();
			commentValueLabel.Text = manager.Torrent.Comment;
		}
		
		private void updateToolBar ()
		{
			TreePath [] treePaths;	
			TreeModel model;
			
			Download previousTorrent = null;
			bool isDifferent = false;
			TorrentState state = TorrentState.Downloading;
			
			//treePaths = torrentTreeView.Selection.GetSelectedRows();
			treePaths = torrentsSelected.GetSelectedRows(out model);
			
			if (treePaths.Length == 0) {
				startTorrentButton.Sensitive = false;
				stopTorrentButton.Sensitive = false;
				removeTorrentButton.Sensitive = false;
				deleteTorrentButton.Sensitive = false;
				return;
			}
			
			removeTorrentButton.Sensitive = true;
			deleteTorrentButton.Sensitive = true;
			
			foreach (TreePath treePath in treePaths) {
				TreeIter iter;
				Download torrent;
				model.GetIter (out iter, treePath);
				
				torrent = (Download) model.GetValue (iter,0);
				state = torrent.State;			
				if (previousTorrent != null) {
					if (previousTorrent.State != torrent.State) {
						isDifferent = true;
						break;
					}
				}
				previousTorrent = torrent;	
			}
			
			if (isDifferent) {
				startTorrentButton.Sensitive = false;
				stopTorrentButton.Sensitive = false;
			} else {
				startTorrentButton.Sensitive = true;
				if (state == TorrentState.Downloading || state == TorrentState.Seeding) {
					startTorrentButton.StockId = "gtk-media-pause";
					startTorrentButton.Label = _("Pause");
					stopTorrentButton.Sensitive = true;
				} else if(state == TorrentState.Paused) {
					stopTorrentButton.Sensitive = true;
					startTorrentButton.StockId = "gtk-media-play";
					startTorrentButton.Label = _("Start");
				} else if(state == TorrentState.Hashing) {
					startTorrentButton.StockId = "gtk-media-play";
					stopTorrentButton.Sensitive = true;
				} else {
					startTorrentButton.StockId = "gtk-media-play";
					startTorrentButton.Label = _("Start");
					stopTorrentButton.Sensitive = false;
				}
			}	
		}

		protected virtual void OnStartTorrentActivated (object sender, System.EventArgs e)
		{
			TreePath [] treePaths;	
			TreeModel model;
			
			treePaths = torrentsSelected.GetSelectedRows (out model);
			
			foreach (TreePath treePath in treePaths) {
				TreeIter iter;
				Download torrent;
				model.GetIter(out iter, treePath);
				
				torrent = (Download) model.GetValue (iter, 0);
				try {
					if (startTorrentButton.StockId == "gtk-media-pause") {
						torrent.Pause ();
						logger.Info ("Torrent paused " + torrent.Manager.Torrent.Name);
					} else {
						torrent.Start ();
						logger.Info ("Torrent started " + torrent.Manager.Torrent.Name);
					}
				} catch {
					logger.Error ("Torrent already started " + torrent.Manager.Torrent.Name);
				}
			}
		}
		
		protected virtual void OnStopTorrentActivated (object sender, System.EventArgs e)
		{
			TreePath [] treePaths;
			TreeModel model;
			treePaths = torrentsSelected.GetSelectedRows (out model);
			
			foreach (TreePath treePath in treePaths) {
				TreeIter iter;
				Download torrent;
				model.GetIter (out iter, treePath);
				torrent = (Download) model.GetValue (iter,0);
				try {
					torrent.Stop ();
				} catch {
					logger.Error ("Torrent already stopped " + torrent.Manager.Torrent.Name);
				}
			}
		}
		
		protected virtual void OnRemoveTorrentButtonActivated (object sender, System.EventArgs e)
		{
			RemoveTorrent ();
		}
		
		private void PeerConnected (object o, PeerConnectionEventArgs e)
		{
			if (e.ConnectionDirection == MonoTorrent.Common.Direction.Incoming)
				natStatus.HasIncoming = true;
		}
        
		private void RemoveTorrent ()
		{
			MessageDialog messageDialog = new MessageDialog (this,
						DialogFlags.Modal,
						MessageType.Question, 
						ButtonsType.YesNo, _("Are you sure you want to remove the torrent?"));
			messageDialog.Title = _("Remove torrent"); 
			ResponseType result = (ResponseType)messageDialog.Run();
			messageDialog.Hide();
			messageDialog.Destroy ();
			
			if (result != ResponseType.Yes)
				return;
			
			TreePath [] treePaths;
			TreeModel model;
			List<Download> torrentsToRemove = new List<Download> ();
			
			//treePaths = torrentTreeView.Selection.GetSelectedRows();
			treePaths = torrentsSelected.GetSelectedRows (out model);
			
			foreach (TreePath treePath in treePaths) {
				TreeIter iter;
				Download torrentToRemove;
				model.GetIter (out iter, treePath);
				
				torrentToRemove = (Download) model.GetValue (iter,0);
				torrentsToRemove.Add (torrentToRemove);
			}
			
			torrentTreeView.Selection.UnselectAll ();
			
			foreach (Download download in torrentsToRemove) {
				TorrentManager toDelete = download.Manager;
				toDelete.PeerConnected -= PeerConnected;
				torrentController.removeTorrent (download);
				File.Delete(download.Manager.Torrent.TorrentPath);
			}
		}
		
		protected virtual void OnNewActivated (object sender, System.EventArgs e)
		{
			CreateTorrentDialog createTorrentDialog = new CreateTorrentDialog (torrentController);
			
			ResponseType result = (ResponseType) createTorrentDialog.Run ();
			
			if (result == ResponseType.Cancel || result == ResponseType.DeleteEvent) {
				createTorrentDialog.Destroy ();
				return;
			}
		
			createTorrentDialog.Destroy ();
			
		}
		private void OnDeleteTorrentButtonActivated (object o, EventArgs e)
		{
			DeleteAndRemoveSelection ();
		}
		
		private void DeleteAndRemoveSelection ()
		{
			List<Download> torrentsToRemove = new List<Download> ();
			MessageDialog messageDialog = new MessageDialog (this,
						DialogFlags.Modal,
						MessageType.Question, 
						ButtonsType.YesNo, _("Remove torrent and delete all data?"));
			messageDialog.Title = _("Delete torrent"); 
			ResponseType result = (ResponseType)messageDialog.Run();
			messageDialog.Hide();
			messageDialog.Destroy ();
			
			if (result == ResponseType.Yes) {
				TreePath [] treePaths;
				TreeModel model;
				//treePaths = torrentTreeView.Selection.GetSelectedRows();
				treePaths = torrentsSelected.GetSelectedRows (out model);
				foreach (TreePath treePath in treePaths) {
					TreeIter iter;
					Download torrentToRemove;
					model.GetIter (out iter, treePath);
					torrentToRemove = (Download) model.GetValue (iter,0);
					torrentsToRemove.Add(torrentToRemove);
				}
				
				torrentTreeView.Selection.UnselectAll();
				
				foreach(Download torrent in torrentsToRemove){
					torrentController.removeTorrent (torrent, true, true);
				}
				
			} else {
	  		   	logger.Info ("Selected NO to delete torrent");
			}
			

		}
		
		private void BuildOptionsPage ()
		{
			
			torrentUploadRateSpinButton.SetRange(0, int.MaxValue);
			torrentDownloadRateSpinButton.SetRange(0, int.MaxValue);
			torrentMaxConnectionsSpinButton.SetRange(0, int.MaxValue);
			torrentUploadSlotSpinButton.SetRange(0, 300);
		}
		
		private void BuildSpeedsPopup()
		{
			SpeedLimitMenu menu = new SpeedLimitMenu();
			statusUpButton.Clicked += Event.Wrap ((EventHandler) delegate {
				menu.ShowAll ();
				menu.IsUpload = true;
				menu.CalculateSpeeds (engineSettings.GlobalMaxUploadSpeed);
				menu.Popup ();
			});
			statusDownButton.Clicked += Event.Wrap ((EventHandler) delegate {
				menu.ShowAll ();
				menu.IsUpload = false;
				menu.CalculateSpeeds (engineSettings.GlobalMaxDownloadSpeed);
				menu.Popup ();
			});

			menu.ClickedItem += Event.Wrap ((EventHandler) delegate (object sender, EventArgs e) {
				menu.HideAll ();
				
				SpeedMenuItem item = (SpeedMenuItem)sender;
				int newSpeed = item.Speed;

				// Update the settings
				if (menu.IsUpload)
					engineSettings.GlobalMaxUploadSpeed = (int)newSpeed;
				else
					engineSettings.GlobalMaxDownloadSpeed = (int)newSpeed;
				updateStatusBar ();
			});
		}
		private void OnTorrentSettingsChanged (object sender, EventArgs args)
		{
			Download download = GetSelectedTorrent();	
						
			if (download == null)
				return;
				
			TorrentManager torrent = download.Manager;
			torrent.Settings.MaxConnections = (int) torrentMaxConnectionsSpinButton.Value;
			torrent.Settings.MaxDownloadSpeed = (int) torrentDownloadRateSpinButton.Value * 1024;
			torrent.Settings.MaxUploadSpeed = (int) torrentUploadRateSpinButton.Value * 1024;
			torrent.Settings.UploadSlots = Math.Max (2, (int) torrentUploadSlotSpinButton.Value);
		}

		protected virtual void OnColumnsActivated (object sender, System.EventArgs e)
		{
			EditColumnsDialog columnsDialog = new EditColumnsDialog (torrentTreeView);
			columnsDialog.Run ();
			columnsDialog.Destroy ();		
		}

		protected virtual void OnShowDetailedInfoActivated (object sender, System.EventArgs e)
		{
			if (ShowDetailedInfo.Active)
				detailNotebook.ShowAll();
			else
				detailNotebook.HideAll();
		}

		protected virtual void OnShowLabelsActivated (object sender, System.EventArgs e)
		{
			if (ShowLabels.Active)
				labelViewScrolledWindow.ShowAll();
			else
				labelViewScrolledWindow.HideAll();
		}

		protected virtual void OnPluginsActivated (object sender, System.EventArgs e)
		{
			RssManagerDialog rssDialog = new RssManagerDialog(rssManagerController);
			rssDialog.Run();
			rssDialog.Destroy ();
		}
		
		private static string _(string s)
		{
			return Mono.Unix.Catalog.GetString(s);
		}
		
		public void LoadTorrent (string path)
		{
			LoadTorrent (path, interfaceSettings.ShowLoadDialog);
		}
		
		public void LoadTorrent (string path, bool ask)
		{
			string error;

			if (!TorrentController.addTorrent (path, ask, out error)) {
				MessageDialog errorDialog = new MessageDialog(this, DialogFlags.DestroyWithParent,
				                                              MessageType.Error, ButtonsType.Close,
				                                              error);
				errorDialog.Run();
				errorDialog.Destroy();
			}
		}
		
		public void LoadTorrent(string path, bool autoStart, bool moveToStorage, bool removeOriginal, TorrentSettings settings, string savePath, bool isUrl)
		{
#warning URL based torrents arent being loaded
			//return torrentController.addTorrent(path, autoStart, moveToStorage, removeOriginal, settings, savePath, isUrl);
		}

		protected virtual void OnReportBugActivated (object sender, System.EventArgs e)
		{
			System.Diagnostics.Process.Start (@"""https://bugzilla.novell.com/enter_bug.cgi?classification=6841&product=Mono%3A+Tools&component=Monsoon""");
		}
	}
}
