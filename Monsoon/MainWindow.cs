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
		internal static EventHandler WrappedHandler (EventHandler h)
		{
			return delegate (object o, EventArgs e) {
				h (o, e);
			};
		}
		internal static EnterNotifyEventHandler WrappedHandler (EnterNotifyEventHandler h)
		{
			return delegate (object o, EnterNotifyEventArgs e) {
				h (o, e);
			};
		}
		internal static DragBeginHandler WrappedHandler (DragBeginHandler h)
		{
			return delegate (object o, DragBeginArgs e) {
				h (o, e);
			};
		}
		internal static DragEndHandler WrappedHandler (DragEndHandler h)
		{
			return delegate (object o, DragEndArgs e) {
				h (o, e);
			};
		}
		internal static GLib.IdleHandler WrappedHandler (GLib.IdleHandler h)
		{
			return delegate {
				return h ();
			};
		}
		internal static EditedHandler WrappedHandler (EditedHandler h)
		{
			return delegate (object o, EditedArgs e) {
				h (o, e);
			};
		}
		
		private static NLog.Logger logger = MainClass.DebugEnabled ? NLog.LogManager.GetCurrentClassLogger () : new EmptyLogger ();
		
		private LabelTreeView labelTreeView;
		private ListStore labelListStore;
		
		private TorrentLabel allLabel;
		private TorrentLabel deleteLabel;
		private TorrentLabel downloadingLabel;
		private TorrentLabel uploadLabel;
		private Button statusDownButton;
		private Button statusUpButton;
		
		private	TorrentTreeView torrentTreeView;
		private TreeSelection torrentsSelected;
		private ListStore torrentListStore;
		private Dictionary<TorrentManager, TreeIter> torrents;
		
		private TorrentController torrentController;

		private EngineSettings engineSettings;
		private SettingsController<TorrentSettings> defaultTorrentSettings;
		private SettingsController<PreferencesSettings> prefSettings;
		private SettingsController<InterfaceSettings> interfaceSettings;
		
		private PeerTreeView peerTreeView;
		private ListStore peerListStore;
		private Dictionary<PeerId, TreePath> peers;
		private TreeModelFilter peerFilter;
		
		private FileTreeView fileTreeView;
		private TorrentFileModel fileTreeStore;
		
		private PiecesTreeView piecesTreeView;
		private ListStore piecesListStore;
		private List<BlockEventArgs> pieces;
		
		private List<TorrentLabel> labels;
		private ListenPortController portController;
		
		private TorrentFolderWatcher folderWatcher;
		
		private Menu trayMenu;
		//private ImageMenuItem quitItem;
		private Egg.TrayIcon trayIcon;
		
		private RssManagerController rssManagerController;
		
		internal Dictionary<PeerId, TreePath> Peers
		{
			get { return peers; }
		}
		
		internal ListStore PeerListStore
		{
			get { return peerListStore; }
		}
		
		public TorrentLabel AllLabel {
			get { return allLabel; }
		}
		
		public TorrentLabel DownloadingLabel {
			get { return downloadingLabel; }
		}
		
		public TorrentLabel SeedingLabel {
			get { return uploadLabel; }
		}

		public GconfSettingsStorage SettingsStorage {
			get {
				return GconfSettingsStorage.Instance;
			}
		}
		
		public InterfaceSettings InterfaceSettings {
			get { return interfaceSettings.Settings; }
		}

		public EngineSettings EngineSettings {
			get {
				return engineSettings;
			}
		}

		public PreferencesSettings Preferences {
			get {
				return prefSettings.Settings;
			}
		}
		
		public TorrentController TorrentController {
			get {
				return torrentController;
			}
		}

		public TorrentSettings DefaultTorrentSettings {
			get {
				return defaultTorrentSettings.Settings;
			}
		}

		public List<TorrentLabel> Labels {
			get {
				return labels;
			}
		}

		public ListStore TorrentListStore {
			get {
				return torrentListStore;
			}
		}

		public Dictionary<TorrentManager, TreeIter> Torrents {
			get {
				return torrents;
			}
		}

		public ListStore LabelListStore {
			get {
				return labelListStore;
			}
		}

		public List<BlockEventArgs> Pieces {
			get {
				return pieces;
			}
		}
		
		public MainWindow (EngineSettings engineSettings, ListenPortController portController): base (Gtk.WindowType.Toplevel)
		{
			this.engineSettings = engineSettings;
			this.portController = portController;
			portController.PortMapped += WrappedHandler ((EventHandler) delegate {
				GLib.Idle.Add(delegate {
					natStatus.PortForwarded = true;
					return false;
				});
			});

			portController.RouterFound += WrappedHandler ((EventHandler) delegate {
				GLib.Idle.Add(delegate {
					natStatus.RouterFound = true;
					return false;
				});
			});
            
			interfaceSettings = new GConfInterfaceSettingsController ();
			defaultTorrentSettings = new GconfTorrentSettingsController ();
			prefSettings = new GconfPreferencesSettingsController ();
			
			Ticker.Tick ();
			LoadAllSettings ();
			Ticker.Tock ("Loaded all settings: {0}");
			
			labels = new  List<TorrentLabel> ();
			torrents = new Dictionary<MonoTorrent.Client.TorrentManager,Gtk.TreeIter> ();
			
			Ticker.Tick ();
			Ticker.Tick ();
			
			Build ();
			
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
			RestoreLabels ();
			Ticker.Tock ("Restored labels");
			
			folderWatcher = new TorrentFolderWatcher (new DirectoryInfo (Preferences.ImportLocation));
			folderWatcher.TorrentFound += delegate(object o, TorrentWatcherEventArgs e) {
				GLib.Idle.Add(WrappedHandler ((GLib.IdleHandler) delegate {
					torrentController.OnTorrentFound(o, e);
					return false;
				}));
			};
			
			if (Preferences.ImportEnabled) {
				logger.Info ("Starting import folder watcher");
				folderWatcher.Start ();
			}
			
			rssManagerController = new RssManagerController(torrentController);
			rssManagerController.StartWatchers();
            
			ShowAll();
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
			quitItem.Activated += WrappedHandler (delegate(object sender, EventArgs args) {
				DeleteEventHandler h = OnDeleteEvent;
				h (sender ,new DeleteEventArgs ());
			});
			
			ImageMenuItem stop = new ImageMenuItem (_("Stop All"));
			stop.Image = new Image (Stock.MediaStop, IconSize.Menu);
			stop.Activated += WrappedHandler ((EventHandler) delegate {
				foreach (TorrentManager m in torrentController.Torrents)
					m.Stop ();
			});
			
			ImageMenuItem start = new ImageMenuItem (_("Start All"));
			start.Image = new Image (Stock.MediaPlay, IconSize.Menu);
			start.Activated += WrappedHandler ((EventHandler) delegate {
				foreach (TorrentManager m in torrentController.Torrents)
					m.Start ();
			});

			CheckMenuItem notifications = new CheckMenuItem (_("Show Notifications"));
			notifications.Active = Preferences.EnableNotifications;
			notifications.Activated += WrappedHandler ((EventHandler) delegate {
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
			
			trayIcon.EnterNotifyEvent += WrappedHandler ((EnterNotifyEventHandler) delegate { 
				trayTip.SetTip(trayIcon, "Monsoon - D: " + ByteConverter.ConvertSpeed(torrentController.Engine.TotalDownloadSpeed) +
				               " U: " + ByteConverter.ConvertSpeed(torrentController.Engine.TotalUploadSpeed), null);
			});
			
			if (this.prefSettings.Settings.EnableTray)
				trayIcon.ShowAll ();
		}
		
		private void OnTrayClicked(object sender, ButtonPressEventArgs args)
		{
			Gdk.EventButton eventButton = args.Event;
			
			if(eventButton.Button == 1){
				if (Visible) {
					int x, y;
					GetPosition (out x, out y);
					interfaceSettings.Settings.WindowXPos = x;
					interfaceSettings.Settings.WindowYPos = y;
					Hide();
				} else {
					Show();
					Move (interfaceSettings.Settings.WindowXPos, interfaceSettings.Settings.WindowYPos);
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
				interfaceSettings.Load ();
			}
			catch (Exception ex)
			{
				logger.Error ("Couldn't load interface settings: {0}", ex.Message);
			}
			
			try	
			{
				prefSettings.Load ();
			}
			catch (Exception ex)
			{
				logger.Error("Could not load preferences: {0}", ex);
			}
			
			try	
			{
				defaultTorrentSettings.Load ();
			}
			catch (Exception ex)
			{
				logger.Error("Could not load default torrent settings: {0}", ex);
			}
		}
		
		private void RestoreInterfaceSettings ()
		{
			InterfaceSettings settings = interfaceSettings.Settings;
			
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
			InterfaceSettings interfaceSettings = this.interfaceSettings.Settings;
			
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
			
			this.interfaceSettings.Save ();
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
			torrentListStore = new ListStore (typeof(TorrentManager));
			torrentController = new TorrentController (this);
			torrentTreeView = new TorrentTreeView (torrentController);
			torrentTreeView.DeleteTorrent += WrappedHandler ((EventHandler) delegate { DeleteAndRemoveSelection (); });
			torrentTreeView.RemoveTorrent += WrappedHandler ((EventHandler) delegate { RemoveTorrent (); });
			//torrentTreeView.Model = torrentListStore;
			torrentTreeView.Selection.Changed += OnTorrentSelectionChanged;
			
			torrentViewScrolledWindow.Add (torrentTreeView);
			//torrentTreeView.Show ();
		}

		private void BuildLabelTreeView()
		{
			/* Move some stuff to LabelTreeView */
			labelTreeView = new LabelTreeView (this, true);
			labelListStore = new ListStore (typeof (TorrentLabel));
			
			labelTreeView.Model = labelListStore;
			labelTreeView.Selection.Changed += OnLabelSelectionChanged;
			labelViewScrolledWindow.Add (labelTreeView);
			//labelTreeView.Show ();

			torrentTreeView.Model = torrentListStore;

			allLabel = new TorrentLabel (_("All"), "gtk-home", true);
			deleteLabel = new TorrentLabel (_("Remove"), "gtk-remove", true);
			downloadingLabel = new TorrentLabel (_("Downloading"), "gtk-go-down", true);
			uploadLabel = new TorrentLabel (_("Seeding"), "gtk-go-up", true);
		
			labelListStore.AppendValues (allLabel);
			labelListStore.AppendValues (downloadingLabel);
			labelListStore.AppendValues (uploadLabel);
			
			TargetEntry [] targetEntries = new TargetEntry[]{
				new TargetEntry("application/x-monotorrent-torrentmanager-objects", 0, 0)
			};

			torrentTreeView.DragBegin += WrappedHandler ((DragBeginHandler) delegate {
				TreeIter it;
				if (!labelTreeView.Selection.GetSelected (out it))
					return;
				
				TorrentLabel label = (TorrentLabel) labelTreeView.Model.GetValue (it, 0);
				if (!label.Immutable)
					labelListStore.AppendValues(deleteLabel); 
			});
			
			torrentTreeView.DragEnd += WrappedHandler ((DragEndHandler) delegate {
				TreeIter iter;
				if (!labelListStore.GetIterFirst (out iter))
					return;
				
				TreeIter prev = iter;
				while (labelListStore.IterNext(ref iter))
					prev = iter;
				
				TorrentLabel label = (TorrentLabel) labelTreeView.Model.GetValue (prev, 0);
				if (label == deleteLabel)
					labelListStore.Remove (ref prev);
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
			if(label == allLabel || label == DownloadingLabel || label == SeedingLabel)
				return;
				
			
			foreach (TorrentManager manager in torrents.Keys)
			{
				if(!Toolbox.ByteMatch (manager.Torrent.InfoHash, args.SelectionData.Data))
					continue;
				
				if (label != deleteLabel)
				{
					label.AddTorrent(manager);
				}
				else
				{
					
					if (!labelTreeView.Selection.GetSelected (out iter))
						return;
					
					label = (TorrentLabel)labelTreeView.Model.GetValue (iter, 0);
					label.RemoveTorrent (manager);
				}
			}
			
		}
		
		private void BuildPiecesTreeView()
		{
			pieces = new List<BlockEventArgs>();
			piecesListStore = new ListStore (typeof(BlockEventArgs));
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
	 		
	 		TorrentManager manager = (TorrentManager) model.GetValue (iter, 0);
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
            peers = new Dictionary<MonoTorrent.Client.PeerId, Gtk.TreePath>();
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
			TorrentManager manager = GetSelectedTorrent();
			
			if(manager == null)
				return false;
				
			if(peer == null)
				return false;
			
			if(!peer.IsConnected)
				return false;
				
			if (peer.TorrentManager == manager)
				return true;
			
			return false;
		}
		
		private void OnTorrentSelectionChanged (object sender, System.EventArgs args)
		{
			torrentsSelected = (TreeSelection) sender;

			updateView ();
			
			// Update Files Page
			updateFilesPage ();
			
			//update Options Page
			updateOptionsPage ();
			
			peerFilter.Refilter();
			
		}
		
		private void updateOptionsPage ()
		{
			TorrentManager torrent = null;
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
				
				torrent = (TorrentManager) model.GetValue (torrentIter,0);	
			}
			
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
			label = (TorrentLabel) labelListStore.GetValue(iter, 0);
			torrentTreeView.Model = label.Model;
			logger.Debug("Label " + label.Name + " selected." );
			
			//torrentTreeView.Selection.UnselectAll();
			//torrentFilter.Refilter();
		}

        private void updatePeersPage()
        {
            lock (peers)
            {
                peerListStore.Foreach(delegate(TreeModel model, TreePath path, TreeIter iter) {
                    peerListStore.EmitRowChanged(path, iter);
                    return true;
                });
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
//			foreach (TorrentManager manager in torrents.Keys){
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
			StoreLabels ();
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
			XmlTorrentStorageController controller = new XmlTorrentStorageController();
			
			logger.Info ("Storing torrent settings");

			foreach (TorrentManager manager in torrents.Keys){
				TorrentStorage torrentToStore = new TorrentStorage();
				torrentToStore.TorrentPath = manager.Torrent.TorrentPath;
				torrentToStore.SavePath = manager.SavePath;
				torrentToStore.Settings = manager.Settings;
				torrentToStore.State = manager.State;
				torrentToStore.UploadedData = torrentController.GetPreviousUpload(manager) + manager.Monitor.DataBytesUploaded;
				torrentToStore.DownloadedData = torrentController.GetPreviousDownload(manager) + manager.Monitor.DataBytesDownloaded;
				torrentToStore.InfoHash = Convert.ToString(manager.GetHashCode());
				foreach(TorrentFile file in manager.FileManager.Files) {
					TorrentFileSettingsModel fileSettings = new TorrentFileSettingsModel();
					fileSettings.Path = file.Path;
					fileSettings.Priority = file.Priority;
					torrentToStore.Files.Add(fileSettings);
				}
				controller.Settings.Add(torrentToStore);	
			}
			controller.Save();
		}

		private void StoreLabels ()
		{
			XmlTorrentLabelController labelController = new XmlTorrentLabelController();
			
			logger.Info ("Storing labels");

			labelController.Settings.Clear();
			
			// FIXME: This is bad -- differentiate between application and user created labels properly
			foreach (TorrentLabel label in labels) {
				if (label.Name == _("All") || label.Name == _("Downloading") || label.Name == _("Seeding"))
					continue;
				labelController.Settings.Add(label);
			}
			
			labelController.Save();
		}
		
		private void RestoreLabels()
		{			
			XmlTorrentLabelController labelController = new XmlTorrentLabelController();
			logger.Info ("Restoring labels");
			
			labelController.Load();
			
			foreach (TorrentLabel label in labelController.Settings) {
				labelListStore.AppendValues (label);
				labels.Add (label);
				
				// Restore previously labeled torrents
				foreach (TorrentManager torrentManager in torrents.Keys){
					if(label.TruePaths == null)
						continue;
					foreach (string path in label.TruePaths){
						if (path == torrentManager.Torrent.TorrentPath)
							label.AddTorrent(torrentManager);
					}
				}
			}
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
			
			defaultTorrentSettings.Save ();
			prefSettings.Save ();
			
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
				foreach (String fileName in fileChooser.Filenames) {
					try {
						torrentController.MainWindow.LoadTorrent (fileName);
					} catch (Exception ex) {
						MessageDialog errorDialog = new MessageDialog(this, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, ex.Message);
						errorDialog.Run();
						errorDialog.Destroy();
					}
				}
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
		
			TorrentManager manager = GetSelectedTorrent ();
			if(manager == null)
				return;

			lock(pieces)
			{
				pieces.RemoveAll(delegate (BlockEventArgs b) { return b.Piece.AllBlocksWritten; });
				
				pieces.Sort(delegate(BlockEventArgs a, BlockEventArgs b) {
					return a.Piece.Index.CompareTo(b.Piece.Index);
				});
				
				foreach (BlockEventArgs blockEvent in pieces)
					if(blockEvent.ID.TorrentManager == manager)
						piecesListStore.AppendValues(blockEvent);
			}
		}
		
		public TorrentManager GetSelectedTorrent ()
		{
			TreePath [] treePaths;
			TreeModel filteredModel;
			
			if(torrentsSelected == null || torrentsSelected.CountSelectedRows() != 1) 
				return null;
				
			treePaths = torrentsSelected.GetSelectedRows( out filteredModel);
			TorrentManager manager = null;
			
			// Should only be one item but have to use GetSelectedRows
			// because of TreeView is set to allow multiple selection
			TreeIter iter;
			filteredModel.GetIter (out iter, treePaths [0]);
			manager = (TorrentManager)filteredModel.GetValue(iter,0);	
			
			return manager;
		}
		
		private void updateLabels ()
		{
			TreeIter iter;
			if (labelListStore.GetIterFirst (out iter)) {
				do {
					if (((TorrentLabel)labelListStore.GetValue (iter, 0)).Immutable)
						labelListStore.EmitRowChanged(labelListStore.GetPath(iter), iter);
				} while (labelListStore.IterNext(ref iter));
			}
		}
		
		private void updateGeneralPage ()
		{
			TreePath [] treePaths;
			
			TreeModel filteredModel;
			treePaths = torrentsSelected.GetSelectedRows (out filteredModel);
			
			TorrentManager manager = null;
			
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
				
				manager = (TorrentManager) filteredModel.GetValue (iter,0);	
			}
			
			if(manager.State == TorrentState.Hashing) {
			 	statusProgressBar.Fraction = (torrentController.GetTorrentHashProgress(manager) / 100f);
			 	statusProgressBar.Text = string.Format("{0} {1:D}%", manager.State, torrentController.GetTorrentHashProgress(manager));
			} else {
				statusProgressBar.Fraction = manager.Progress / 100f;
				statusProgressBar.Text = string.Format("{0} {1:F}%", manager.State, manager.Progress);
			}
			
			if (manager.State != TorrentState.Stopped)
				elapsedTimeValueLabel.Text = DateTime.MinValue.Add(DateTime.Now.Subtract(manager.StartTime)).ToString("HH:mm:ss");
			else
				elapsedTimeValueLabel.Text = null;
			
			downloadedValueLabel.Text = ByteConverter.ConvertSize (torrentController.GetPreviousDownload(manager) + manager.Monitor.DataBytesDownloaded);
			uploadedValueLabel.Text = ByteConverter.ConvertSize (torrentController.GetPreviousUpload(manager) + manager.Monitor.DataBytesUploaded);
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
			
			swarmSpeedLabel.Text = ByteConverter.ConvertSpeed (torrentController.GetTorrentSwarm(manager));
			savePathValueLabel.Text = manager.SavePath;
			sizeValueLabel.Text = ByteConverter.ConvertSize (manager.Torrent.Size);
			createdOnValueLabel.Text = manager.Torrent.CreationDate.ToLongDateString ();
			commentValueLabel.Text = manager.Torrent.Comment;
		}
		
		private void updateToolBar ()
		{
			TreePath [] treePaths;	
			TreeModel model;
			
			TorrentManager previousTorrent = null;
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
				TorrentManager torrent;
				model.GetIter (out iter, treePath);
				
				torrent = (TorrentManager) model.GetValue (iter,0);
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
				TorrentManager torrent;
				model.GetIter(out iter, treePath);
				
				torrent = (TorrentManager) model.GetValue (iter, 0);
				try {
					if (startTorrentButton.StockId == "gtk-media-pause") {
						torrent.Pause ();
						logger.Info ("Torrent paused " + torrent.Torrent.Name);
					} else {
						torrent.Start ();
						logger.Info ("Torrent started " + torrent.Torrent.Name);
					}
				} catch {
					logger.Error ("Torrent already started " + torrent.Torrent.Name);
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
				TorrentManager torrent;
				model.GetIter (out iter, treePath);
				torrent = (TorrentManager) model.GetValue (iter,0);
				try {
					torrent.Stop ();
				} catch {
					logger.Error ("Torrent already stopped " + torrent.Torrent.Name);
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
			List<TorrentManager> torrentsToRemove = new List<TorrentManager> ();
			
			//treePaths = torrentTreeView.Selection.GetSelectedRows();
			treePaths = torrentsSelected.GetSelectedRows (out model);
			
			foreach (TreePath treePath in treePaths) {
				TreeIter iter;
				TorrentManager torrentToRemove;
				model.GetIter (out iter, treePath);
				
				torrentToRemove = (TorrentManager) model.GetValue (iter,0);
				torrentsToRemove.Add (torrentToRemove);
			}
			
			torrentTreeView.Selection.UnselectAll ();
			
			foreach (TorrentManager toDelete in torrentsToRemove) {
				toDelete.PeerConnected -= PeerConnected;
				torrentController.removeTorrent (toDelete);
				File.Delete(toDelete.Torrent.TorrentPath);
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
			List<TorrentManager> torrentsToRemove = new List<TorrentManager> ();
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
					TorrentManager torrentToRemove;
					model.GetIter (out iter, treePath);
					torrentToRemove = (TorrentManager) model.GetValue (iter,0);
					torrentsToRemove.Add(torrentToRemove);
				}
				
				torrentTreeView.Selection.UnselectAll();
				
				foreach(TorrentManager torrent in torrentsToRemove){
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
			statusUpButton.Clicked += WrappedHandler ((EventHandler) delegate {
				menu.ShowAll ();
				menu.IsUpload = true;
				menu.CalculateSpeeds (engineSettings.GlobalMaxUploadSpeed);
				menu.Popup ();
			});
			statusDownButton.Clicked += WrappedHandler ((EventHandler) delegate {
				menu.ShowAll ();
				menu.IsUpload = false;
				menu.CalculateSpeeds (engineSettings.GlobalMaxDownloadSpeed);
				menu.Popup ();
			});

			menu.ClickedItem += WrappedHandler ((EventHandler) delegate (object sender, EventArgs e) {
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
			TorrentManager torrent = GetSelectedTorrent();	
						
			if (torrent == null)
				return;
				
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
			LoadTorrent (path, interfaceSettings.Settings.ShowLoadDialog);
		}
		
		public void LoadTorrent (string path, bool ask)
		{
			Torrent torrent;
			
			if (!Torrent.TryLoad (path, out torrent)) {
				MessageDialog errorDialog = new MessageDialog(this, DialogFlags.DestroyWithParent,
				                                              MessageType.Error, ButtonsType.Close,
				                                              _("Invalid torrent selected"));
				errorDialog.Run();
				errorDialog.Destroy();
				return;
			}
			
			if(torrentController.Engine.Contains(torrent)) {
				MessageDialog errorDialog = new MessageDialog(this, DialogFlags.DestroyWithParent,
				                                              MessageType.Error, ButtonsType.Close,
				                                              _("Torrent has already been added"));
				errorDialog.Run();
				errorDialog.Destroy();
				return;
			}
			
			string savePath = engineSettings.SavePath;
			if (ask)
			{
				LoadTorrentDialog dialog = new LoadTorrentDialog(torrent, savePath);
				dialog.AlwaysAsk = interfaceSettings.Settings.ShowLoadDialog;
				
				try
				{
					int response = dialog.Run ();
					interfaceSettings.Settings.ShowLoadDialog = dialog.AlwaysAsk;
					if (response != (int)ResponseType.Ok) {
						return;
					}
					
					savePath = dialog.SelectedPath;
				}
				finally
				{
					dialog.Destroy ();
				}
			}
			
			try
			{
				TorrentManager manager = torrentController.addTorrent (torrent, savePath);
				manager.PeerConnected += delegate(object o, PeerConnectionEventArgs e) {
					GLib.Idle.Add(delegate {
						PeerConnected(o, e);
						return false;
					});
				};
			}
			catch (Exception ex)
			{
				string error = _("An unexpected error occured while loading the torrent. {0}");
				MessageDialog errorDialog = new MessageDialog(this, DialogFlags.DestroyWithParent,
				                                              MessageType.Error, ButtonsType.Close,
				                                              string.Format (error, ex.Message));
				errorDialog.Run ();
				errorDialog.Destroy ();
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
