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
		private LabelTreeView labelTreeView;
		private ListStore labelListStore;
		
		private TorrentLabel allLabel;
		private TorrentLabel deleteLabel;
		private TorrentLabel downloadingLabel;
		private TorrentLabel uploadLabel;
		
		private	TorrentTreeView torrentTreeView;
		private TreeSelection torrentsSelected;
		private ListStore torrentListStore;
		private Dictionary<TorrentManager, TreeIter> torrents;
		
		private TorrentController torrentController;

		private UserEngineSettings userEngineSettings;
		private UserTorrentSettings userTorrentSettings;
		private PreferencesSettings prefSettings;
		private InterfaceSettings interfaceSettings;
		
		private PeerTreeView peerTreeView;
		private ListStore peerListStore;
		private Dictionary<PeerId, TreeIter> peers;
		private TreeModelFilter peerFilter;
		
		private FileTreeView fileTreeView;
		private TreeStore fileTreeStore;
		
		private PiecesTreeView piecesTreeView;
		private ListStore piecesListStore;
		private List<BlockEventArgs> pieces;
		
		private ArrayList labels;
		private ListenPortController portController;
		
		private TorrentFolderWatcher folderWatcher;
		
		private Menu trayMenu;
		//private ImageMenuItem quitItem;
		private Egg.TrayIcon trayIcon;
		
		private RssManagerController rssManagerController;
		
		internal Dictionary<PeerId, TreeIter> Peers
		{
			get { return peers; }
		}
		
		internal ListStore PeerListStore
		{
			get { return peerListStore; }
		}
		
		//private MemoryTarget memoryTarget;
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger (); 
		
		public MainWindow (GconfSettingsStorage settingsStorage, UserEngineSettings userEngineSettings, ListenPortController portController, bool isFirstRun): base (Gtk.WindowType.Toplevel)
		{
			prefSettings = new PreferencesSettings ();
			this.userEngineSettings = userEngineSettings;
			this.portController = portController;
			userTorrentSettings = new UserTorrentSettings ();
			interfaceSettings = new InterfaceSettings ();
			
			labels = new ArrayList ();
			torrents = new Dictionary<MonoTorrent.Client.TorrentManager,Gtk.TreeIter> ();
			
			Build ();
			BuildTray();
			BuildPiecesTreeView();
			BuildTorrentTreeView();
			BuildPeerTreeView();
			BuildFileTreeView();
			BuildLabelTreeView();
			BuildOptionsPage();
			BuildSpeedsPopup();
			
			GLib.Timeout.Add (1000, new GLib.TimeoutHandler (updateView));
			
			RestoreInterface ();
			
			//portController = new ListenPortController(userEngineSettings);
			if (prefSettings.UpnpEnabled)
				portController.Start();
			
			torrentController.LoadStoredTorrents ();
			
			// auto-start torrents
			TorrentSettingsController torrentSettingsController =
				new TorrentSettingsController(settingsStorage);
			TorrentFileSettingsController fileSettingsController =
				new TorrentFileSettingsController(settingsStorage);
			foreach (TorrentManager manager in torrentController.Torrents) {
				TorrentSettingsModel torrentSettings =
					torrentSettingsController.GetTorrentSettings(manager.Torrent.InfoHash);
				
				if (torrentSettings.LastState == TorrentState.Downloading) {
					// restore priority
					foreach (TorrentFile torrentFile in manager.Torrent.Files) {
						TorrentFileSettingsModel fileSettings =
							fileSettingsController.GetFileSettings(
								manager.Torrent.InfoHash,
								torrentFile.Path
							);
						Console.WriteLine("restoring priority for: " + torrentFile.Path +
						                  "  to " + fileSettings.Priority);
						torrentFile.Priority = fileSettings.Priority;
					}
				}
				    
				if (torrentSettings.LastState == TorrentState.Downloading ||
					torrentSettings.LastState == TorrentState.Seeding) {
					Console.WriteLine("auto-starting: " + manager.Torrent.Name);
					manager.Start();
				}
			}
			
			RestoreLabels ();
			
			folderWatcher = new TorrentFolderWatcher (new DirectoryInfo (prefSettings.ImportLocation));
			folderWatcher.TorrentFound += torrentController.OnTorrentFound;
			
			if (prefSettings.ImportEnabled) {
				logger.Info ("Starting import folder watcher");
				folderWatcher.Start ();
			}
			
			rssManagerController = new RssManagerController(torrentController);
			rssManagerController.StartWatchers();
			
			if(isFirstRun)
				OpenDruid();
		}	
		
		public Egg.TrayIcon TrayIcon {
			get { return trayIcon; }
		}
		
		private void BuildTray()
		{
			EventBox eventBox = new EventBox ();
			trayMenu = new Menu ();
			
			ImageMenuItem quitItem = new ImageMenuItem ("Quit");
			quitItem.Image = new Image (Stock.Quit, IconSize.Menu);
			quitItem.Activated += delegate(object sender, EventArgs args)
			{
				OnDeleteEvent (sender ,new DeleteEventArgs ());
			};
			
			ImageMenuItem stop = new ImageMenuItem("Stop All");
			stop.Image = new Image (Stock.Stop, IconSize.Menu);
			stop.Activated += delegate {
				foreach (TorrentManager m in torrentController.Torrents)
					m.Stop ();
			};
			
			ImageMenuItem start = new ImageMenuItem ("Start All");
			start.Image = new Image (Stock.MediaPlay, IconSize.Menu);
			start.Activated += delegate {
				foreach (TorrentManager m in torrentController.Torrents)
					m.Start ();
			};
			
			trayMenu.Append (start);
			trayMenu.Append (stop);
			trayMenu.Append (quitItem);
			
			eventBox.Add (new Image (Stock.GoDown, IconSize.Menu));
			eventBox.ButtonPressEvent += OnTrayClicked;
			trayIcon = new Egg.TrayIcon (Defines.ApplicationName);
			trayIcon.Icon = new Image (Stock.Network, IconSize.Menu).Pixbuf;
			trayIcon.Add (eventBox);
			
			if(prefSettings.EnableTray)
				trayIcon.ShowAll ();
		}
		
		private void OnTrayClicked(object sender, ButtonPressEventArgs args)
		{
			Gdk.EventButton eventButton = args.Event;
			
			if(eventButton.Button == 1){
				if (Visible) {
					Hide();
				} else {
					Show();
				}	
			}
			
			if(eventButton.Button == 3){
				// show context menu
				trayMenu.ShowAll ();
				trayMenu.Popup();
			}
		}
		
		private void RestoreInterface()
		{
			logger.Info ("Restoring interface settings");
			SetDefaultSize (interfaceSettings.WindowWidth, interfaceSettings.WindowHeight);
			
			// moved here
			ShowAll();
			
			if (interfaceSettings.WindowYPos == 0 && interfaceSettings.WindowXPos == 0)
				SetPosition (WindowPosition.Center);
			else
				Move (interfaceSettings.WindowXPos, interfaceSettings.WindowYPos);
			
			vPaned.Position = interfaceSettings.VPaned;
			hPaned.Position = interfaceSettings.HPaned;
			
			ShowDetailedInfo.Active = interfaceSettings.ShowDetails;
			ShowLabels.Active = interfaceSettings.ShowLabels;
			labelViewScrolledWindow.Visible = interfaceSettings.ShowLabels;
			detailNotebook.Visible = interfaceSettings.ShowDetails;
			
			// Restore columns
			torrentTreeView.nameColumn.FixedWidth = interfaceSettings.NameColumnWidth;
			torrentTreeView.doneColumn.FixedWidth = interfaceSettings.DoneColumnWidth;
			torrentTreeView.statusColumn.FixedWidth = interfaceSettings.StatusColumnWidth;
			torrentTreeView.seedsColumn.FixedWidth = interfaceSettings.SeedsColumnWidth;
			torrentTreeView.peersColumn.FixedWidth = interfaceSettings.PeersColumnWidth;
			torrentTreeView.downSpeedColumn.FixedWidth = interfaceSettings.DlSpeedColumnWidth;
			torrentTreeView.upSpeedColumn.FixedWidth = interfaceSettings.UpSpeedColumnWidth;
			torrentTreeView.ratioColumn.FixedWidth = interfaceSettings.RatioColumnWidth;
			torrentTreeView.sizeColumn.FixedWidth = interfaceSettings.SizeColumnWidth;
			
			torrentTreeView.nameColumn.Visible = interfaceSettings.NameColumnVisible;
			torrentTreeView.doneColumn.Visible = interfaceSettings.DoneColumnVisible;
			torrentTreeView.statusColumn.Visible = interfaceSettings.StatusColumnVisible;
			torrentTreeView.seedsColumn.Visible = interfaceSettings.SeedsColumnVisible;
			torrentTreeView.peersColumn.Visible = interfaceSettings.PeersColumnVisible;
			torrentTreeView.downSpeedColumn.Visible = interfaceSettings.DlSpeedColumnVisible;
			torrentTreeView.upSpeedColumn.Visible = interfaceSettings.UpSpeedColumnVisible;
			torrentTreeView.ratioColumn.Visible = interfaceSettings.RatioColumnVisible;
			torrentTreeView.sizeColumn.Visible = interfaceSettings.SizeColumnVisible;
		}
		
		private void StoreInterface ()
		{
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
			interfaceSettings.SizeColumnWidth = torrentTreeView.ratioColumn.Width;
			interfaceSettings.SizeColumnVisible = torrentTreeView.ratioColumn.Visible;
					
			interfaceSettings.Store ();
		}
		
		
		private void BuildFileTreeView ()
		{
			fileTreeStore = new TreeStore (typeof(TorrentManager), typeof(TorrentFile), typeof(Gdk.Pixbuf), typeof(string));
			fileTreeView = new FileTreeView (GconfSettingsStorage.Instance, torrentController, fileTreeStore);
			fileTreeView.Model = fileTreeStore;
			filesScrolledWindow.Add (fileTreeView);
			fileTreeView.Show();
		}
		
		private void BuildTorrentTreeView ()
		{
			torrentListStore = new ListStore (typeof(TorrentManager));
			torrentController = new TorrentController (this);
			torrentTreeView = new TorrentTreeView (torrentController);
			
			//torrentTreeView.Model = torrentListStore;
			torrentTreeView.Selection.Changed += OnTorrentSelectionChanged;
			
			torrentViewScrolledWindow.Add (torrentTreeView);
			torrentTreeView.Show ();
		}

		private void BuildLabelTreeView()
		{
			/* Move some stuff to LabelTreeView */
			labelTreeView = new LabelTreeView (this, true);
			labelListStore = new ListStore (typeof (TorrentLabel));
			
			labelTreeView.Model = labelListStore;
			labelTreeView.Selection.Changed += OnLabelSelectionChanged;
			labelViewScrolledWindow.Add (labelTreeView);
			labelTreeView.Show ();
				
			//torrentFilter = new Gtk.TreeModelFilter (torrentListStore, null);
			//torrentFilter = new TorrentFilterModel(torrentListStore, null);
			//torrentFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterTorrentTree);
			
			torrentTreeView.Model = torrentListStore;
			//torrentTreeView.Model = torrentFilter;
			
			//ArrayList allList = new ArrayList ();
			//foreach(TorrentManager manager in torrents.Keys){
			//	allList.Add (manager);
			//}
			
			allLabel = new TorrentLabel (new ArrayList(), "All", "gtk-home", true);
			deleteLabel = new TorrentLabel (new ArrayList(), "Remove", "gtk-remove", true);
			downloadingLabel = new TorrentLabel (new ArrayList(), "Downloading", "gtk-go-down", true);
			uploadLabel = new TorrentLabel (new ArrayList(), "Seeding", "gtk-go-up", true);
		
			labelListStore.AppendValues (allLabel);
			labelListStore.AppendValues (downloadingLabel);
			labelListStore.AppendValues (uploadLabel);
			
			TargetEntry [] targetEntries = new TargetEntry[]{
				new TargetEntry("application/x-monotorrent-torrentmanager-objects", 0, 0)
			};

			torrentTreeView.DragBegin += delegate {
				TreeIter it;
				if (!labelTreeView.Selection.GetSelected (out it))
					return;
				
				TorrentLabel label = (TorrentLabel) labelTreeView.Model.GetValue (it, 0);
				if (!label.Immutable)
					labelListStore.AppendValues(deleteLabel); 
			};
			
			torrentTreeView.DragEnd += delegate {
				TreeIter iter;
				if (!labelListStore.GetIterFirst (out iter))
					return;
				
				TreeIter prev = iter;
				while (labelListStore.IterNext(ref iter))
					prev = iter;
				
				TorrentLabel label = (TorrentLabel) labelTreeView.Model.GetValue (prev, 0);
				if (label == deleteLabel)
					labelListStore.Remove (ref prev);
			};
			
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
			piecesScrolledWindow.ShowAll();
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
			peers = new Dictionary<MonoTorrent.Client.PeerId,Gtk.TreeIter> ();
			peerTreeView = new PeerTreeView ();
			peerListStore = new ListStore (typeof(PeerId));
			
			peerFilter = new TreeModelFilter (peerListStore, null);
			peerFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterPeerTree);
			
			peerTreeView.Model = peerFilter;
			peersScrolledWindow.Add (peerTreeView);
			peerTreeView.Show ();
		}

		private bool FilterPeerTree (Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			PeerId peer = (PeerId) model.GetValue(iter, 0);
			TorrentManager manager = GetSelectedTorrent();
			
			if(manager == null)
				return false;
				
			if(peer == null)
				return false;
				
			if(!peer.IsValid)
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
			torrentUploadRateSpinButton.Value = torrent.Settings.MaxUploadSpeed;
			torrentDownloadRateSpinButton.Value = torrent.Settings.MaxDownloadSpeed;
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
			lock(peers)
				foreach(TreeIter iter in peers.Values){
					peerListStore.EmitRowChanged(peerListStore.GetPath(iter), iter);
				}
		}

		private void updateStatusBar()
		{
			string limited;
			if (userEngineSettings.GlobalMaxDownloadSpeed == 0)
				limited = "";
			else
				limited = "[" + ByteConverter.ConvertSpeed (userEngineSettings.GlobalMaxDownloadSpeed) + "]";
			
			statusDownloadLabel.Markup = string.Format("{0}{1}{2}{3}", "<small>D: ", 
			                                           limited,
			                                           ByteConverter.ConvertSpeed(torrentController.Engine.TotalDownloadSpeed),
			                                           "</small>");
			
			if (userEngineSettings.GlobalMaxUploadSpeed == 0)
				limited = "";
			else
				limited = string.Format("[{0}]", ByteConverter.ConvertSpeed (userEngineSettings.GlobalMaxUploadSpeed));
			
			statusUploadLabel.Markup = string.Format("{0}{1}{2}{3}",  "<small>U: ",
			                                         limited, 
			                                         ByteConverter.ConvertSpeed (torrentController.Engine.TotalUploadSpeed),
			                                         "</small>");
		}
		
		
		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Hide ();
			
			if(prefSettings.QuitOnClose && sender == this){
				a.RetVal = true;
				return;
			}
			
			StoreTorrentSettings ();
			List<WaitHandle> handles = new List<WaitHandle> ();
			foreach (TorrentManager manager in torrents.Keys){
				if(manager.State == TorrentState.Stopped)
					continue;
				try{
					handles.Add (manager.Stop ());
				}
				catch{
					logger.Error ("Cannot stop torrent " + manager.Torrent.Name);
				}	
			}
			
			StoreInterface ();
			StoreLabels ();
			rssManagerController.Store();
			torrentController.StoreFastResume ();
			
			foreach (WaitHandle h in handles)
				h.WaitOne(TimeSpan.FromSeconds(1.5), false);
			
			Application.Quit ();

			a.RetVal = true;
		}
		
		private void StoreTorrentSettings ()
		{
			ArrayList torrentsToStore = new ArrayList ();
			
			logger.Info ("Storing torrent settings");
			
			foreach (TorrentManager manager in torrents.Keys){
				torrentsToStore.Add (new TorrentStorage(manager.Torrent.TorrentPath, manager.SavePath, (UserTorrentSettings)manager.Settings, manager.State, torrentController.GetPreviousUpload(manager) + manager.Monitor.DataBytesUploaded, torrentController.GetPreviousDownload(manager) + manager.Monitor.DataBytesDownloaded));	
			}
			
			using (Stream fs = new FileStream (Defines.SerializedTorrentSettings, FileMode.Create))
			{
				XmlWriter writer = new XmlTextWriter (fs, Encoding.UTF8);
				
				XmlSerializer s = new XmlSerializer (typeof(TorrentStorage[]));
				s.Serialize (writer, torrentsToStore.ToArray (typeof(TorrentStorage)));
			}
		}

		private void StoreLabels ()
		{
			ArrayList labelsToStore = new ArrayList ();
			
			logger.Info ("Storing labels");
			
			foreach (TorrentLabel label in labels) {
				if (label.Name == "All" || label.Name == "Downloading" || label.Name == "Seeding")
					continue;
				labelsToStore.Add (label);
			}
			
			using (Stream fs = new FileStream (Defines.SerializedLabels, FileMode.Create))
			{
				XmlWriter writer = new XmlTextWriter (fs, Encoding.UTF8);
				XmlSerializer s = new XmlSerializer (typeof(TorrentLabel[]));
				s.Serialize(writer, labelsToStore.ToArray (typeof(TorrentLabel)));
			}
		}
		
		private void RestoreLabels()
		{			
			TorrentLabel [] labelsToRestore = null;
			XmlSerializer xs = new XmlSerializer (typeof(TorrentLabel[]));
			
			logger.Info ("Restoring labels");
			
			try
			{
				if (!System.IO.File.Exists(Defines.SerializedLabels))
					return;

				using (FileStream fs = System.IO.File.OpenRead(Defines.SerializedLabels))
					labelsToRestore = (TorrentLabel[]) xs.Deserialize(fs);
			}
			catch
			{
				logger.Error("Error opening " + Defines.SerializedLabels);
				return;
			}
					
			foreach (TorrentLabel label in labelsToRestore) {
				labelListStore.AppendValues (label);
				labels.Add (label);
				// Restore previously labeled torrents
				foreach (TorrentManager torrentManager in torrents.Keys){
					if(label.TruePaths == null)
						continue;
					foreach (string path in label.TruePaths){
						if (path == torrentManager.Torrent.TorrentPath){
							label.AddTorrent(torrentManager);
						}
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
			foreach (WaitHandle h in this.torrentController.Engine.StopAll())
				h.WaitOne (TimeSpan.FromSeconds(10), false);
			
			this.torrentController.Engine.Dispose();
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
			
			userTorrentSettings.Store ();
			userEngineSettings.Store ();
			prefSettings.Store ();
			
			if (prefSettings.ImportEnabled) {
				logger.Info ("Starting import folder watcher");
				folderWatcher.Start ();
			} else {
				// If it hasn't been started before, running stop will crash it -- fix in library?
				folderWatcher.Start ();
				logger.Info ("Stoping import folder watcher");
				folderWatcher.Stop ();
			}
			
			if (prefSettings.UpnpEnabled) {
				if (!portController.IsRunning) {
					portController.Start ();
				}
				else if (portController.MappedPort != userEngineSettings.ListenPort) {
					portController.ChangePort ();
				} else {
					portController.MapPort ();
				}
			} else if (!prefSettings.UpnpEnabled && portController.IsRunning) {
				portController.RemoveMap();
			}
		}

		protected virtual void OnOpenActivated (object sender, System.EventArgs e)
		{
			FileFilter torrentFilter = new FileFilter ();
			FileFilter allFilter = new FileFilter ();
			FileChooserDialog fileChooser = new FileChooserDialog ("Open torrent(s)", this, FileChooserAction.Open, Gtk.Stock.Cancel, ResponseType.Cancel, Gtk.Stock.Open, ResponseType.Accept);
			
			torrentFilter.Name = "Torrent files";
			torrentFilter.AddPattern ("*.torrent");
			
			allFilter.Name = "All files";
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
						torrentController.addTorrent(fileName);
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
		
        
		public static Gdk.Pixbuf GetIconPixbuf(string iconName)
		{
			if (iconName == null) {
				return new Gdk.Pixbuf(IntPtr.Zero);
			}
			
			return new Gdk.Pixbuf(System.IO.Path.Combine(Defines.IconPath, iconName));
		}
        
		private void updateFilesPage ()
		{
			TorrentManager manager;
			
			fileTreeStore.Clear ();
			manager = GetSelectedTorrent ();
			
			if (manager == null)
				return;
			
			Console.WriteLine("Updating files page of: " + manager.Torrent.Name);
			
			TorrentFileSettingsController fileSettingsController =
				new TorrentFileSettingsController(GconfSettingsStorage.Instance);
			foreach (TorrentFile torrentFile in manager.Torrent.Files) {
				TorrentFileSettingsModel fileSettings =
					fileSettingsController.GetFileSettings(
						manager.Torrent.InfoHash,
						torrentFile.Path
					);
				torrentFile.Priority = fileSettings.Priority;
				
				fileTreeStore.AppendValues(manager, torrentFile,
					GetIconPixbuf(
						FileTreeView.GetPriorityIconName(torrentFile.Priority)
					),
					torrentFile.Path);
			}
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
			
			if(torrentsSelected.CountSelectedRows() != 1) 
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
			/*if (labelListStore.GetIterFirst (out iter)) {
				do {
					labelListStore.EmitRowChanged(labelListStore.GetPath(iter), iter);
				} while (labelListStore.IterNext(ref iter));
			}*/
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
			 	statusProgressBar.Fraction = (int)torrentController.GetTorrentHashProgress(manager) / 100f;
			 	statusProgressBar.Text = manager.State + (torrentController.GetTorrentHashProgress(manager) / 100f).ToString (" 0%");
			} else {
				if ((manager.Progress/100f) >= 0 && (manager.Progress/100f) <= 1 ) {
					statusProgressBar.Fraction = manager.Progress / 100f;
					statusProgressBar.Text = manager.State + (manager.Progress / 100f).ToString (" 0%");
				}
			}
			
			if (manager.State != TorrentState.Stopped)
				elapsedTimeValueLabel.Text = DateTime.MinValue.Add(DateTime.Now.Subtract(manager.StartTime)).ToString("HH:mm:ss");
			else
				elapsedTimeValueLabel.Text = null;
			
			downloadedValueLabel.Text = ByteConverter.ConvertSize (torrentController.GetPreviousDownload(manager) + manager.Monitor.DataBytesDownloaded);
			uploadedValueLabel.Text = ByteConverter.ConvertSize (torrentController.GetPreviousUpload(manager) + manager.Monitor.DataBytesUploaded);
			trackerUrlValueLabel.Text = manager.TrackerManager.CurrentTracker.ToString ();
			trackerStatusValueLabel.Text = manager.TrackerManager.CurrentTracker.State.ToString ();
			lastUpdatedLabel.Text = manager.TrackerManager.CurrentTracker.LastUpdated.ToString ("HH:mm:ss") ;
			hashFailsLabel.Text = manager.HashFails.ToString ();
			
			if (manager.State != TorrentState.Stopped){
				DateTime nextUpdate = manager.TrackerManager.LastUpdated.AddSeconds(manager.TrackerManager.CurrentTracker.UpdateInterval);
				if(nextUpdate > DateTime.Now)
					updateInLabel.Text =  DateTime.MinValue.Add (nextUpdate - DateTime.Now).ToString("HH:mm:ss");
			}
			
			swarmSpeedLabel.Text = ByteConverter.ConvertSpeed (torrentController.GetTorrentSwarm(manager) * manager.Torrent.PieceLength);
			savePathValueLabel.Text = manager.SavePath;
			sizeValueLabel.Text = ByteConverter.ConvertSize (manager.Torrent.Size);
			createdOnValueLabel.Text = manager.Torrent.CreationDate.ToLongDateString ();
			commentValueLabel.Text = manager.Torrent.Comment;
			messageLabel.Text = manager.TrackerManager.CurrentTracker.WarningMessage + manager.TrackerManager.CurrentTracker.FailureMessage;
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
					startTorrentButton.Label = "Pause";
					stopTorrentButton.Sensitive = true;
				} else if(state == TorrentState.Paused) {
					stopTorrentButton.Sensitive = true;
					startTorrentButton.StockId = "gtk-media-play";
					startTorrentButton.Label = "Start";
				} else if(state == TorrentState.Hashing) {
					startTorrentButton.StockId = "gtk-media-play";
					stopTorrentButton.Sensitive = true;
				} else {
					startTorrentButton.StockId = "gtk-media-play";
					startTorrentButton.Label = "Start";
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
					
					UpdateTorrentSettings(torrent);
				} catch {
					logger.Error ("Torrent already started " + torrent.Torrent.Name);
				}
			}
		}

		private void UpdateTorrentSettings(TorrentManager manager)
		{
			TorrentSettingsController torrentSettingsController =
				new TorrentSettingsController(GconfSettingsStorage.Instance);
			TorrentSettingsModel torrentSettings =
				torrentSettingsController.GetTorrentSettings(manager.Torrent.InfoHash);
			torrentSettings.LastState = manager.State;
			torrentSettingsController.SetTorrentSettings(torrentSettings);
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
					
					UpdateTorrentSettings(torrent);
				} catch {
					logger.Error ("Torrent already stopped " + torrent.Torrent.Name);
				}
			}
		}
		
		protected virtual void OnRemoveTorrentButtonActivated (object sender, System.EventArgs e)
		{
			TreePath [] treePaths;
			TreeModel model;
			ArrayList torrentsToRemove = new ArrayList ();
			
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
				torrentController.removeTorrent (toDelete);
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
	
		protected virtual void OnDeleteTorrentButtonActivated (object sender, System.EventArgs e)
		{
			ArrayList torrentsToRemove = new ArrayList();
			MessageDialog messageDialog = new MessageDialog (this,
						DialogFlags.DestroyWithParent,
						MessageType.Question, 
						ButtonsType.YesNo, "Remove torrent and delete data?");
			messageDialog.Title = "Delete torrent"; 
			ResponseType result = (ResponseType)messageDialog.Run();
			messageDialog.Hide();
			
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
					torrentController.removeTorrent (torrent, false, true);
				}
				
			} else {
	  		   	logger.Info ("Selected NO to delete torrent");
			}
			
			messageDialog.Destroy ();
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
			eventUpload.ButtonPressEvent += delegate {
				menu.ShowAll ();
				menu.IsUpload = true;
				menu.CalculateSpeeds (userEngineSettings.GlobalMaxUploadSpeed);
				menu.Popup ();
			};
			eventDownload.ButtonPressEvent += delegate {
				menu.ShowAll ();
				menu.IsUpload = false;
				menu.CalculateSpeeds (userEngineSettings.GlobalMaxDownloadSpeed);
				menu.Popup ();
			};

			menu.ClickedItem += delegate (object sender, EventArgs e) {
				menu.HideAll ();
				
				SpeedMenuItem item = (SpeedMenuItem)sender;
				int newSpeed = item.Speed;

				// Update the settings
				if (menu.IsUpload)
					userEngineSettings.GlobalMaxUploadSpeed = (int)newSpeed;
				else
					userEngineSettings.GlobalMaxDownloadSpeed = (int)newSpeed;
				updateStatusBar ();
			};
		}
		private void OnTorrentSettingsChanged (object sender, EventArgs args)
		{
			TorrentManager torrent = GetSelectedTorrent();	
						
			if (torrent == null)
				return;
				
			torrent.Settings.MaxConnections = (int) torrentMaxConnectionsSpinButton.Value;
			torrent.Settings.MaxDownloadSpeed = (int) torrentDownloadRateSpinButton.Value * 1024;
			torrent.Settings.MaxUploadSpeed = (int) torrentUploadRateSpinButton.Value * 1024;
			torrent.Settings.UploadSlots = (int) torrentUploadSlotSpinButton.Value;
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

		protected virtual void OnDruidDebugActivated (object sender, System.EventArgs e)
		{
			OpenDruid();
		}
		
		private void OpenDruid()
		{
			OnDruidFinishedClicked (null, EventArgs.Empty);
		}

		private void OnDruidFinishedClicked(object o, EventArgs args)
		{
			userEngineSettings.ListenPort = new System.Random().Next(30000, 36000);
			userEngineSettings.SavePath = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			prefSettings.TorrentStorageLocation = Defines.TorrentFolder;
			prefSettings.UpnpEnabled = true;
			userEngineSettings.GlobalMaxDownloadSpeed = 0;
			userEngineSettings.GlobalMaxUploadSpeed = 0;
			
			userEngineSettings.Store ();
			prefSettings.Store ();
			
			if (prefSettings.UpnpEnabled)
				portController.Start();
			
			logger.Info("First run wizard complete!");
		}

		protected virtual void OnPluginsActivated (object sender, System.EventArgs e)
		{
			RssManagerDialog rssDialog = new RssManagerDialog(rssManagerController);
			rssDialog.Run();
			rssDialog.Destroy ();
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

		public UserEngineSettings UserEngineSettings {
			get {
				return userEngineSettings;
			}
		}

		public PreferencesSettings PrefSettings {
			get {
				return prefSettings;
			}
		}

		public UserTorrentSettings UserTorrentSettings {
			get {
				return userTorrentSettings;
			}
		}

		public ArrayList Labels {
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
	}
}