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
		
		private TorrentController torrentController;
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
		
		EngineSettings EngineSettings {
			get { return SettingsManager.EngineSettings; }
		}
		internal ListStore PeerListStore
		{
			get { return peerListStore; }
		}

		PreferencesSettings Preferences {
			get { return SettingsManager.Preferences; }
		}
		public GconfSettingsStorage SettingsStorage {
			get {
				return GconfSettingsStorage.Instance;
			}
		}
		
		public InterfaceSettings InterfaceSettings {
			get { return interfaceSettings; }
		}

		public TorrentController TorrentController {
			get {
				return torrentController;
			}
		}

		public MainWindow (): base (Gtk.WindowType.Toplevel)
		{
			this.portController = ServiceManager.Get <ListenPortController> ();
			this.torrentController = ServiceManager.Get <TorrentController> ();
			interfaceSettings = new InterfaceSettings ();
			
			Ticker.Tick ();
			LoadAllSettings ();
			Ticker.Tock ("Loaded all settings: {0}");
			
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
			
			if (SettingsManager.Preferences.UpnpEnabled)
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
			
			torrentController.Initialise ();
			folderWatcher = new TorrentFolderWatcher (new DirectoryInfo (SettingsManager.Preferences.ImportLocation));
			folderWatcher.TorrentFound += delegate(object o, TorrentWatcherEventArgs e) {
				GLib.Idle.Add(Event.Wrap ((GLib.IdleHandler) delegate {
					TorrentFound (o, e);
					return false;
				}));
			};
			
			torrentController.SelectionChanged += delegate {
				updateToolBar ();
			};
			
			torrentController.ShouldRemove += HandleShouldRemove;
			
			if (SettingsManager.Preferences.ImportEnabled) {
				logger.Info ("Starting import folder watcher");
				folderWatcher.Start ();
			}
			
			logger.Info ("Starting RSS manager");
			rssManagerController = new RssManagerController(SettingsManager.EngineSettings);
			rssManagerController.TorrentFound += delegate(object sender, TorrentRssWatcherEventArgs e) {
				string savePath = e.Filter == null ? SettingsManager.EngineSettings.SavePath : e.Filter.SavePath;
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

		void HandleShouldRemove (object sender, ShouldRemoveEventArgs e)
		{
			string title;
			string message;
			
			if (e.DeleteData) {
				title = _("Delete torrent");
				message = _("Remove torrent and delete all data?");
			} else {
				title = _("Remove torrent");
				message = _("Are you sure you want to remove the torrent?");
			}
			
			MessageDialog messageDialog = new MessageDialog (this,
			                                                 DialogFlags.Modal,
			                                                 MessageType.Question, 
			                                                 ButtonsType.YesNo, message);
			messageDialog.Title = title;
			e.ShouldRemove = (ResponseType) messageDialog.Run() == ResponseType.Yes;
			messageDialog.Hide();
			messageDialog.Destroy ();
		}
		
		void TorrentFound (object sender, TorrentWatcherEventArgs args)
		{
			if(!SettingsManager.Preferences.ImportEnabled)
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
			
			if (Preferences.EnableTray)
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
				logger.Error ("Couldn't load interface settings: {0}", ex);
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
			foreach (TorrentTreeView.Column c in torrentTreeView.Columns) {
				if (c.Ignore)
					continue;
				
				c.Visible = settings.ColumnVisibility [c.Name];
				c.FixedWidth = settings.ColumnWidth [c.Name];
			}
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
			foreach (TorrentTreeView.Column c in torrentTreeView.Columns) {
				if (c.Ignore)
					continue;
				
				interfaceSettings.ColumnVisibility [c.Name] = c.Visible;
				interfaceSettings.ColumnWidth [c.Name] = c.Width;
			}
			
			SettingsManager.Store <InterfaceSettings> (interfaceSettings);
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
			TorrentController.Added += delegate(object sender, DownloadAddedEventArgs e) {
				e.Download.StateChanged += HandleStateChanged;
			};
			
			TorrentController.Removed += delegate(object sender, DownloadAddedEventArgs e) {
				e.Download.StateChanged -= HandleStateChanged;
			};
			TorrentController.SelectionChanged += delegate {
				updateView ();
				
				// Update Files Page
				updateFilesPage ();
				
				//update Options Page
				updateOptionsPage ();
				
				peerFilter.Refilter();
			};
			torrentController.ShouldAdd += HandleShouldAdd;
			torrentTreeView = new TorrentTreeView ();
			torrentTreeView.DragDataReceived += TreeviewDragDataReceived;
			torrentViewScrolledWindow.Add (torrentTreeView);
		}

		void HandleShouldAdd(object sender, ShouldAddEventArgs e)
		{
			LoadTorrentDialog dialog = new LoadTorrentDialog(e.Torrent, e.SavePath);
			dialog.AlwaysAsk = interfaceSettings.ShowLoadDialog;
			
			try
			{
				e.ShouldAdd = dialog.Run () == (int)ResponseType.Ok;
				interfaceSettings.ShowLoadDialog = dialog.AlwaysAsk;
				e.SavePath = dialog.SelectedPath;
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

		void HandleStateChanged(object sender, StateChangedEventArgs args)
		{
			// Update toolbar
			updateToolBar ();
			
			Download manager = (Download) sender;

			if (args.NewState == Monsoon.State.Stopped)
				PeerListStore.Clear ();
			
			this.updateView ();
			
			if (!Preferences.EnableNotifications)
				return;
			if (args.NewState != Monsoon.State.Seeding)
				return;
			if (args.OldState != Monsoon.State.Downloading)
				return;

			try {
				Notifications.Notification notify = new Notifications.Notification (_("Download Complete"), manager.Torrent.Name, Stock.GoDown);
				if (Preferences.EnableTray)
					notify.AttachToWidget (TrayIcon);
				notify.Urgency = Notifications.Urgency.Low;
				notify.Timeout = 5000;
				notify.Show ();
				notify.AddAction("reveal-item", "Show", delegate {
					System.Diagnostics.Process.Start("\"file://" + manager.SavePath + "\"");
				});
			} catch (Exception ex) {
				logger.Error ("Could not display notification");
				logger.Error (ex.ToString());
			}
		}

		private void BuildLabelTreeView()
		{
			/* Move some stuff to LabelTreeView */
			LabelController = ServiceManager.Get <LabelController> ();
			labelTreeView = new LabelTreeView (LabelController, true);
			
			labelViewScrolledWindow.Add (labelTreeView);

			TargetEntry [] targetEntries = new TargetEntry[]{
				new TargetEntry("application/x-monotorrent-Download-objects", 0, 0)
			};

			torrentTreeView.DragBegin += Event.Wrap ((DragBeginHandler) delegate {
				TreeIter it;
				if (!labelTreeView.Selection.GetSelected (out it))
					return;
				
				TorrentLabel label = (TorrentLabel) labelTreeView.Model.GetValue (it, 0);
				if (!label.Immutable)
					LabelController.Add (LabelController.Delete);
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
					LabelController.Remove (LabelController.Delete);
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
			
			if (args.SelectionData.Format != 8)
				return;
			
			Download download = TorrentController.Torrents.Find (delegate (Download o) {
				return Toolbox.ByteMatch (o.Torrent.InfoHash, args.SelectionData.Data);
			});
			if (download == null)
				return;
			
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
			Download manager = TorrentController.SelectedDownload;
			
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
		
		private void updateOptionsPage ()
		{
			Download download = torrentController.SelectedDownload;

			torrentUploadRateSpinButton.ValueChanged -= OnTorrentSettingsChanged;
			torrentDownloadRateSpinButton.ValueChanged -= OnTorrentSettingsChanged;
			torrentMaxConnectionsSpinButton.ValueChanged -= OnTorrentSettingsChanged;
			torrentUploadSlotSpinButton.ValueChanged -= OnTorrentSettingsChanged;	
			
			torrentUploadRateSpinButton.Sensitive = download != null;
			torrentDownloadRateSpinButton.Sensitive = download != null;
			torrentMaxConnectionsSpinButton.Sensitive = download != null;
			torrentUploadSlotSpinButton.Sensitive = download != null;
			
			if (download == null) {
				torrentUploadRateSpinButton.Value = 0;
				torrentDownloadRateSpinButton.Value = 0;
				torrentMaxConnectionsSpinButton.Value = 0;
				torrentUploadSlotSpinButton.Value = 0;
			} else {
				TorrentSettings settings = download.Manager.Settings;
				torrentUploadRateSpinButton.Value = settings.MaxUploadSpeed / 1024;;
				torrentDownloadRateSpinButton.Value = settings.MaxDownloadSpeed / 1024;
				torrentMaxConnectionsSpinButton.Value = settings.MaxConnections;
				torrentUploadSlotSpinButton.Value = settings.UploadSlots;
			}

			torrentUploadRateSpinButton.ValueChanged += OnTorrentSettingsChanged;
			torrentDownloadRateSpinButton.ValueChanged += OnTorrentSettingsChanged;
			torrentMaxConnectionsSpinButton.ValueChanged += OnTorrentSettingsChanged;
			torrentUploadSlotSpinButton.ValueChanged += OnTorrentSettingsChanged;	
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
			if (EngineSettings.GlobalMaxDownloadSpeed == 0)
				limited = "";
			else
				limited = "[" + ByteConverter.ConvertSpeed (EngineSettings.GlobalMaxDownloadSpeed) + "]";
			
			statusDownButton.Label = string.Format("{0}{1}", limited,
			                                           ByteConverter.ConvertSpeed(torrentController.Engine.TotalDownloadSpeed));
			
			if (EngineSettings.GlobalMaxUploadSpeed == 0)
				limited = "";
			else
				limited = string.Format("[{0}]", ByteConverter.ConvertSpeed (EngineSettings.GlobalMaxUploadSpeed));
			
			statusUpButton.Label = string.Format("{0}{1}", limited, 
			                                         ByteConverter.ConvertSpeed (torrentController.Engine.TotalUploadSpeed));
		}
		
		
		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			StoreInterfaceSettings ();
			
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

			foreach (Download download in TorrentController.Torrents) {
				TorrentManager manager = download.Manager;
				TorrentStorage torrentToStore = new TorrentStorage();
				torrentToStore.TorrentPath = manager.Torrent.TorrentPath;
				torrentToStore.Priority = download.Priority;
				torrentToStore.SavePath = manager.SavePath;
				torrentToStore.Settings = manager.Settings;
				torrentToStore.State = download.State;
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
			
			SettingsManager.Store <TorrentSettings> (SettingsManager.DefaultTorrentSettings);
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
				else if (portController.MappedPort != EngineSettings.ListenPort) {
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
				
			// Update TreeView
			if (torrentTreeView.Model != null && torrentTreeView.Model.GetIterFirst (out iter)) {
				do {
					torrentTreeView.Model.EmitRowChanged (torrentTreeView.Model.GetPath (iter), iter);
				} while (torrentTreeView.Model.IterNext (ref iter));
			}
			
			// Update General Page
			updateGeneralPage ();
			
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
			this.fileTreeStore.Update (TorrentController.SelectedDownload);
		}
		
		private void updatePiecesPage()
		{
			piecesListStore.Clear ();
		
			Download manager = TorrentController.SelectedDownload;
			if(manager == null)
				return;

			List<Piece> pieces = manager.Manager.GetActiveRequests ();
			pieces.Sort ();
			
			foreach (Piece piece in pieces)
				piecesListStore.AppendValues(piece);
		}

		private void updateGeneralPage ()
		{
			Download download = TorrentController.SelectedDownload;
			
			if (download == null) {
				statusProgressBar.Fraction = 0;
				statusProgressBar.Text = string.Empty;
				
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
			} else {
				TorrentManager manager = download.Manager;
				statusProgressBar.Fraction = download.Progress;
				statusProgressBar.Text = string.Format("{0} {1:0.00}%", download.State, download.Progress * 100);
				
				downloadedValueLabel.Text = ByteConverter.ConvertSize (download.TotalDownloaded);
				uploadedValueLabel.Text = ByteConverter.ConvertSize (download.TotalUploaded);
				
				if (download.State != Monsoon.State.Stopped)
					elapsedTimeValueLabel.Text = DateTime.MinValue.Add ((DateTime.Now - manager.StartTime)).ToString("HH:mm:ss");
				else
					elapsedTimeValueLabel.Text = null;

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
				
				if (download.State != Monsoon.State.Stopped){
					if (tracker != null)
					{
						DateTime nextUpdate = manager.TrackerManager.LastUpdated.Add (tracker.UpdateInterval);
						
						if(nextUpdate > DateTime.Now)
							updateInLabel.Text =  DateTime.MinValue.Add ((nextUpdate - DateTime.Now)).ToString("HH:mm:ss");
					}
				}
				
				swarmSpeedLabel.Text = ByteConverter.ConvertSpeed (torrentController.SelectedDownload.SwarmSpeed);
				savePathValueLabel.Text = manager.SavePath;
				sizeValueLabel.Text = ByteConverter.ConvertSize (manager.Torrent.Size);
				createdOnValueLabel.Text = manager.Torrent.CreationDate.ToLongDateString ();
				commentValueLabel.Text = manager.Torrent.Comment;
			}
		}
		
		private void updateToolBar ()
		{
			int count = TorrentController.SelectedDownloads.Count;
			removeTorrentButton.Sensitive = count != 0;
			deleteTorrentButton.Sensitive = count != 0;
			
			if (count == 0) {
				startTorrentButton.Sensitive = false;
				stopTorrentButton.Sensitive = false;
			} else {
				bool anyNotActive = TorrentController.SelectedDownloads.Exists (delegate (Download d) {
					return d.State == Monsoon.State.Stopped || d.State == Monsoon.State.Stopping || d.State == Monsoon.State.Paused;
				});
				bool anyNotStopped = TorrentController.SelectedDownloads.Exists (delegate (Download d) {
					return d.State != Monsoon.State.Stopped;
				});
				bool allActive = TorrentController.SelectedDownloads.TrueForAll (delegate (Download d) {
					return !(d.State == Monsoon.State.Stopped || d.State == Monsoon.State.Stopping || d.State == Monsoon.State.Paused);
				});
				if (torrentController.SelectedDownload != null)
				Console.WriteLine ("State: {0}, AnyNotActive: {1}, AnyNotStopped: {2}, AllActive: {3}",
				                   TorrentController.SelectedDownload, anyNotActive, anyNotStopped, allActive);
				stopTorrentButton.Sensitive = anyNotStopped;
				startTorrentButton.Sensitive = true;
				
				if (allActive) {
					startTorrentButton.StockId = "gtk-media-pause";
					startTorrentButton.Label = _("Pause");
				} else if (anyNotActive) {
					startTorrentButton.StockId = "gtk-media-play";
					startTorrentButton.Label = _("Start");
				}
			}
		}

		protected virtual void OnStartTorrentActivated (object sender, System.EventArgs e)
		{
			bool pause = startTorrentButton.StockId == "gtk-media-pause";
			foreach (Download download in TorrentController.SelectedDownloads) {
				try {
					switch (download.State) {
					case Monsoon.State.Downloading:
					case Monsoon.State.Seeding:
						if (pause)
							download.Pause ();
						break;
						
					case Monsoon.State.Stopped:
						if (!pause)
							download.Start ();
						break;
						
					case Monsoon.State.Paused:
						if (!pause)
							download.Resume ();
						break;
					}
				} catch {
					logger.Error ("Torrent already started " + download.Manager.Torrent.Name);
				}
			}
		}
		
		protected virtual void OnStopTorrentActivated (object sender, System.EventArgs e)
		{
			foreach (Download download in TorrentController.SelectedDownloads)
			try {
				download.Stop ();
			} catch {
				logger.Error ("Torrent already stopped " + download.Manager.Torrent.Name);
			}
		}
	
		protected virtual void OnRemoveTorrentButtonActivated (object sender, System.EventArgs e)
		{
			TorrentController.RemoveTorrent ();
		}
		
		private void PeerConnected (object o, PeerConnectionEventArgs e)
		{
			if (e.ConnectionDirection == MonoTorrent.Common.Direction.Incoming)
				natStatus.HasIncoming = true;
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
			TorrentController.RemoveTorrent (true, true);
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
				menu.CalculateSpeeds (EngineSettings.GlobalMaxUploadSpeed);
				menu.Popup ();
			});
			statusDownButton.Clicked += Event.Wrap ((EventHandler) delegate {
				menu.ShowAll ();
				menu.IsUpload = false;
				menu.CalculateSpeeds (EngineSettings.GlobalMaxDownloadSpeed);
				menu.Popup ();
			});

			menu.ClickedItem += Event.Wrap ((EventHandler) delegate (object sender, EventArgs e) {
				menu.HideAll ();
				
				SpeedMenuItem item = (SpeedMenuItem)sender;
				int newSpeed = item.Speed;

				// Update the settings
				if (menu.IsUpload)
					EngineSettings.GlobalMaxUploadSpeed = (int)newSpeed;
				else
					EngineSettings.GlobalMaxDownloadSpeed = (int)newSpeed;
				updateStatusBar ();
			});
		}
		private void OnTorrentSettingsChanged (object sender, EventArgs args)
		{
			Download download = TorrentController.SelectedDownload;
						
			if (download != null) {
				TorrentManager torrent = download.Manager;
				torrent.Settings.MaxConnections = (int) torrentMaxConnectionsSpinButton.Value;
				torrent.Settings.MaxDownloadSpeed = (int) torrentDownloadRateSpinButton.Value * 1024;
				torrent.Settings.MaxUploadSpeed = (int) torrentUploadRateSpinButton.Value * 1024;
				torrent.Settings.UploadSlots = Math.Max (2, (int) torrentUploadSlotSpinButton.Value);
			}
		}

		protected virtual void OnColumnsActivated (object sender, System.EventArgs e)
		{
			EditColumnsDialog columnsDialog = new EditColumnsDialog (torrentTreeView.Columns);
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

		void OnPriorityLowest (object sender, System.EventArgs e)
		{
			Console.WriteLine ("Setting priority to: {0}", TorrentController.Torrents.Count);
			TorrentController.SetPriority (TorrentController.SelectedDownload, TorrentController.Torrents.Count);
		}
		
		void OnPriorityHighest (object sender, System.EventArgs e)
		{
			TorrentController.SetPriority (TorrentController.SelectedDownload, 1);
		}
	}
}
