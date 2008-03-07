//
// TorrentController.cs
//
// Author:
//   Jared Hendry (buchan@gmail.com)
//
// Copyright (C) 2007 Jared Hendry
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

using MonoTorrent.Client;
using MonoTorrent.Common;
using MonoTorrent.BEncoding;

using Gtk;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Xml.Serialization;
using MonoTorrent.TorrentWatcher;

namespace Monsoon
{

	public class TorrentController
	{
		public ClientEngine engine;	
		internal EngineSettings engineSettings;
		private TorrentSettings torrentSettings;
		private ListStore torrentListStore;
		private UserTorrentSettings userTorrentSettings;
		private UserEngineSettings userEngineSettings;
		private PreferencesSettings prefSettings;
		private Dictionary<TorrentManager, TreeIter> torrents;
		private Dictionary<TorrentManager, int> torrentSwarm;
		private Dictionary<TorrentManager, int> hashProgress;
		private Dictionary<TorrentManager, long> torrentPreviousUpload;
		private Dictionary<TorrentManager, long> torrentPreviousDownload;
		private MainWindow mainWindow;
		
		private List<TorrentManager> torrentsDownloading;
		private List<TorrentManager> torrentsSeeding;
		private List<TorrentManager> allTorrents;
		private ArrayList labels;
		private List<BlockEventArgs> pieces;
				
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		public TorrentController(MainWindow mainWindow)
		{
			this.userTorrentSettings = mainWindow.userTorrentSettings;
			this.userEngineSettings = mainWindow.userEngineSettings;
			this.prefSettings = mainWindow.prefSettings;
			this.labels = mainWindow.labels;
			this.torrentListStore = mainWindow.torrentListStore;
			this.torrents = mainWindow.torrents;
			this.mainWindow = mainWindow;
			this.pieces = mainWindow.pieces;
			this.torrentPreviousUpload = new Dictionary<MonoTorrent.Client.TorrentManager,long>();
			this.torrentPreviousDownload = new Dictionary<MonoTorrent.Client.TorrentManager,long>();
			
			engineSettings = new EngineSettings(userEngineSettings.SavePath, userEngineSettings.ListenPort, userEngineSettings.GlobalMaxConnections, userEngineSettings.GlobalMaxHalfOpenConnections, userEngineSettings.GlobalMaxDownloadSpeed, userEngineSettings.GlobalMaxUploadSpeed, EncryptionType.None, userEngineSettings.AllowLegacyConnections);
			torrentSettings = new TorrentSettings(userTorrentSettings.UploadSlots, userTorrentSettings.MaxConnections, userTorrentSettings.MaxDownloadSpeed, userTorrentSettings.MaxUploadSpeed, userTorrentSettings.FastResumeEnabled);
			
			engine = new ClientEngine(engineSettings);
			
			engine.ConnectionManager.PeerMessageTransferred += OnPeerMessageTransferred;
			
			hashProgress = new Dictionary<MonoTorrent.Client.TorrentManager,int>();
			torrentSwarm = new Dictionary<MonoTorrent.Client.TorrentManager,int>();
			torrentsDownloading = new List<TorrentManager>();
			torrentsSeeding = new List<TorrentManager>(); 
			allTorrents = new List<TorrentManager>();
		}
		
		private void OnPeerMessageTransferred(object sender, PeerMessageEventArgs args)
		{
			// FIXME: Swarm speed infinitely grows :S
			if(args.Direction != Direction.Incoming)
				return;
			
			if(!torrentSwarm.ContainsKey(args.TorrentManager))
				torrentSwarm.Add(args.TorrentManager, 0);
			
			torrentSwarm[args.TorrentManager]++;
		}
		
		public int GetTorrentSwarm(TorrentManager manager)
		{
			if(!torrentSwarm.ContainsKey(manager))
				return 0;
			
			int i = torrentSwarm[manager];
			torrentSwarm[manager] = 0;
			
			return i;
		}
		
		public void reloadEngineSettings()
		{
			engine.Settings.SavePath = userEngineSettings.SavePath;
			engine.Settings.ListenPort = userEngineSettings.ListenPort;
			engine.Settings.GlobalMaxConnections = userEngineSettings.GlobalMaxConnections;
			engine.Settings.GlobalMaxDownloadSpeed = userEngineSettings.GlobalMaxDownloadSpeed;
			engine.Settings.GlobalMaxUploadSpeed = userEngineSettings.GlobalMaxUploadSpeed;
			
			logger.Info("Engine settings reloaded");
		}
		
		
		// TODO: Refactor all of these functions!!!
		public TorrentManager addTorrent(string torrentPath)
		{
			return addTorrent(torrentPath, prefSettings.StartNewTorrents, true);
		}
		
		public TorrentManager addTorrent(string torrentPath, bool startTorrent)
		{
			return addTorrent(torrentPath, startTorrent, true);
		}
		
		public TorrentManager addTorrent(string torrentPath, bool startTorrent, bool moveToStorage)
		{
			return addTorrent(torrentPath, startTorrent, moveToStorage, false, null);
		}
		public TorrentManager addTorrent(string torrentPath, bool startTorrent, bool moveToStorage, bool removeOriginal, TorrentSettings savedSettings)
		{
			return addTorrent(torrentPath, startTorrent, moveToStorage, removeOriginal, savedSettings, engine.Settings.SavePath, false);
		}
		public TorrentManager addTorrent(string torrentPath, bool startTorrent, bool moveToStorage, bool removeOriginal, TorrentSettings savedSettings, string savePath, bool isUrl)
		{
			Torrent torrent;
			Torrent torrentCheck;
			TorrentManager manager;
			string newPath;
			
			if(!Directory.Exists(savePath))
				throw new TorrentException("Torrent save path does not exist, " + savePath);
		
			if(!isUrl && !File.Exists(torrentPath))
				throw new TorrentException("Torrent path does not exist, " + torrentPath);
			
			// Check to see if Torrent is valid
			if (!isUrl && !Torrent.TryLoad(torrentPath, out torrentCheck)) {
				logger.Error("Failed to add torrent, " + Path.GetFileName(torrentPath) + " is not valid.");
				throw new TorrentException("Failed to add torrent, " + Path.GetFileName(torrentPath) + " is not valid.");
			} else if(isUrl && !Torrent.TryLoad( new System.Uri(torrentPath), Path.Combine(prefSettings.TorrentStorageLocation, Path.GetFileName(torrentPath)), out torrentCheck)) {
				logger.Error("Failed to add URL, " + torrentPath + " is not valid.");
				throw new TorrentException("Failed to add URL, " + torrentPath + " is not valid.");
			}
			
			// Check to see if torrent already exists
			if (engine.Contains (torrentCheck)) {
				logger.Error ("Failed to add torrent, " + torrentCheck.Name + " already exists.");
				throw new TorrentException ("Failed to add torrent, " + torrentCheck.Name + " already exists.");
			}
			
			// Move torrent to storage folder
			if (!isUrl && moveToStorage && (prefSettings.TorrentStorageLocation != Directory.GetParent(torrentPath).ToString()) ) {
				newPath = Path.Combine(prefSettings.TorrentStorageLocation, Path.GetFileName(torrentPath));
				logger.Debug("Copying torrent to " + newPath);	
				File.Copy(torrentPath, newPath, true);
				
				if (removeOriginal) {
					logger.Info("Deleting original torrent " + torrentPath);
					try{
						File.Delete(torrentPath);
					} catch{
						logger.Error("Unable to delete " + Path.GetFileName(torrentPath) + ".");
						throw new Exception("Unable to delete " + Path.GetFileName(torrentPath) + ".");
					}
				}

			} else {
				newPath = torrentPath;
			}
			
			
			
			// Load and register torrent
			if(!isUrl && !Torrent.TryLoad(newPath, out torrent)){
				logger.Error("Failed to register " + Path.GetFileName(newPath));
				throw new TorrentException("Failed to register " + Path.GetFileName(newPath));
			} else if (isUrl && !Torrent.TryLoad(new System.Uri(newPath), Path.Combine(prefSettings.TorrentStorageLocation, Path.GetFileName(newPath)), out torrent)){
				logger.Error("Failed to register " + newPath);
				throw new TorrentException("Failed to register " + newPath);
			}
			
			if (savedSettings == null)
				manager = new TorrentManager(torrent, savePath, (TorrentSettings)torrentSettings.Clone());
			else
				manager = new TorrentManager(torrent, savePath, savedSettings);
					
			engine.Register(manager);
			
			torrents.Add (manager, torrentListStore.AppendValues(manager));
			allTorrents.Add (manager);
			
			if (startTorrent) {
				logger.Info("Auto starting torrent " + manager.Torrent.Name);
				manager.Start();
				// Add to label
				if (manager.State == TorrentState.Downloading)
					mainWindow.DownloadingLabel.AddTorrent(manager);
				else if (manager.State == TorrentState.Seeding)
					mainWindow.SeedingLabel.AddTorrent(manager);
			}
					
			logger.Info ("Added torrent " + manager.Torrent.Name);

			manager.TorrentStateChanged += OnTorrentStateChanged;
			manager.PieceManager.BlockRequested += OnBlockRequested;
			manager.PieceHashed += OnPieceHashed;
			manager.PeerConnected += OnPeerConnected;
			manager.PeerDisconnected += OnPeerDisconnected;

			// add to "All" label
			mainWindow.AllLabel.AddTorrent(manager);
						
			return manager;
		}
		
		private void OnPeerConnected (object sender, PeerConnectionEventArgs a)
		{
			if(!a.PeerID.IsValid)
				return;
			
			Gtk.Application.Invoke (delegate {
				mainWindow.Peers.Add (a.PeerID, mainWindow.PeerListStore.AppendValues (a.PeerID));
			});
		}
		
		private void OnPeerDisconnected (object sender, PeerConnectionEventArgs a)
		{
			TreeIter iter;
			
			if(a.PeerID == null)
				return;
			
			Gtk.Application.Invoke (delegate {
				lock(mainWindow.Peers){
					if(!mainWindow.Peers.ContainsKey(a.PeerID))
						return;
					iter = mainWindow.Peers [a.PeerID];
					mainWindow.PeerListStore.Remove (ref iter);
					mainWindow.Peers.Remove (a.PeerID);
				}
			});
		}
		
		private void OnBlockRequested (object sender, BlockEventArgs args)
		{
			// add a requested piece
			lock(pieces)
			{
				if(args.Piece.AllBlocksWritten)
					return;
				
				if (pieces.Exists(delegate (BlockEventArgs e) { return e.Piece == args.Piece; }))
					return;

				pieces.Add(args);
			}
		}
		
		private void OnPieceHashed (object sender, PieceHashedEventArgs args)
		{
	
			if(!hashProgress.ContainsKey(args.TorrentManager))
				hashProgress.Add(args.TorrentManager, 0);
			else
				hashProgress[args.TorrentManager] = (int) ((args.PieceIndex / (float)args.TorrentManager.Torrent.Pieces.Count) * 100);
			
			// remove hashed piece from pieces
			lock(pieces)
			foreach (BlockEventArgs blockEvent in pieces) {
				
				if (blockEvent.Piece.Index != args.PieceIndex){
					pieces.Remove(blockEvent);
					return;
				}
			}
		}
		
		public int GetTorrentHashProgress(TorrentManager manager)
		{
			if(!hashProgress.ContainsKey(manager))
				return 0;
			
			return hashProgress[manager];
		}
		
		private void OnTorrentStateChanged(object sender, TorrentStateChangedEventArgs args)
		{
			TorrentManager manager = (TorrentManager)sender;
			
			Gtk.Application.Invoke (delegate {
				if (args.OldState == TorrentState.Downloading) {
					logger.Debug("Removing " + manager.Torrent.Name + " from download label");
					mainWindow.DownloadingLabel.RemoveTorrent(manager);
				} else if (args.OldState == TorrentState.Seeding) {
					logger.Debug("Removing " + manager.Torrent.Name + " from upload label");
					mainWindow.SeedingLabel.RemoveTorrent(manager);
				}
				
				if (args.NewState == TorrentState.Downloading) {
					logger.Debug("Adding " + manager.Torrent.Name + " to download label");
					mainWindow.DownloadingLabel.AddTorrent(manager);
				} else if (args.NewState == TorrentState.Seeding) {
					logger.Debug("Adding " + manager.Torrent.Name + " to upload label");
					mainWindow.SeedingLabel.AddTorrent(manager);
				}
			
				if (!prefSettings.EnableNotifications)
					return;
				if (args.NewState != TorrentState.Seeding)
					return;
				if (args.OldState != TorrentState.Downloading)
					return;
			
				Notifications.Notification notify = new Notifications.Notification ("Download Complete", manager.Torrent + " has finished downloading.");
				notify.AttachToWidget (mainWindow.TrayIcon);
				notify.Timeout = 5000;
				notify.Show ();
			});
			
		}
		
		public List<TorrentManager> TorrentsDownloading
		{
			get{ return torrentsDownloading; }
		}
		
		public List<TorrentManager> Torrents
		{
			get{ return allTorrents; }
		}
		
		public List<TorrentManager> TorrentsSeeding
		{
			get { return torrentsSeeding; }
		}
		
		public void removeTorrent(TorrentManager torrent)
		{
			removeTorrent(torrent, false);
		}
		
		public void removeTorrent(TorrentManager torrent, bool deleteTorrent)
		{
			removeTorrent(torrent, deleteTorrent, false);
		}
		
		public void removeTorrent(TorrentManager torrent, bool deleteTorrent, bool deleteData)
		{
			if(torrent.State != TorrentState.Stopped)
				torrent.Stop();
			
			TreeIter iter = torrents[torrent];
			torrentListStore.Remove(ref iter);
			torrents.Remove(torrent);
			allTorrents.Remove(torrent);
			
			if(deleteData){
				logger.Info("Deleting torrent data " + torrent.Torrent.Name);
				foreach(TorrentFile torrentFile in torrent.Torrent.Files){
					try{
						File.Delete(Path.Combine(torrent.SavePath, torrentFile.Path));
					} catch {
						logger.Error("Unable to delete " + Path.Combine(torrent.SavePath, torrentFile.Path));
					}
				}
			}
			
			if(deleteTorrent){
				logger.Info(" Deleting torrent file " + torrent.Torrent.TorrentPath);
				if(torrent.Settings.FastResumeEnabled){
					try{
						File.Delete(torrent.Torrent.TorrentPath + ".fresume");
					} catch {
						logger.Error("Unable to delete " + torrent.Torrent.TorrentPath + ".fresume");
					}
				}
				
				try{
					File.Delete(torrent.Torrent.TorrentPath);
				} catch {
					logger.Error("Unable to delete " + torrent.Torrent.TorrentPath);
				}
			}
			
			foreach(TorrentLabel label in labels){
				label.RemoveTorrent(torrent);
			}
			
			logger.Info("Removed torrent " + torrent.Torrent.Name);
			mainWindow.AllLabel.RemoveTorrent(torrent);
			
			if(torrentSwarm.ContainsKey(torrent))
				torrentSwarm.Remove(torrent);
			if(hashProgress.ContainsKey(torrent))
				hashProgress.Remove(torrent);
			
			engine.Unregister(torrent);
		}
		
		public void OnTorrentFound(object sender, TorrentWatcherEventArgs args)
		{
			if(!prefSettings.ImportEnabled)
				return;
				
			logger.Info("New torrent detected, adding " + args.TorrentPath);
			addTorrent(args.TorrentPath, prefSettings.StartNewTorrents, true, prefSettings.RemoveOnImport, null);
		}
		
		public void LoadStoredTorrents()
		{
			TorrentManager manager;

			string storedTorrentFile = System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "monotorrent"), "torrents.xml");
			
			TorrentStorage [] torrentsToRestore = null;
			XmlSerializer xs = new XmlSerializer(typeof(TorrentStorage[]));
			
			logger.Info("Loading stored torrents from " + prefSettings.TorrentStorageLocation);
			try
			{
				if(!File.Exists(storedTorrentFile))
					return;
				
				using (FileStream fs = File.Open(storedTorrentFile, FileMode.Open))
					torrentsToRestore = (TorrentStorage[]) xs.Deserialize(fs);
			}
			catch
			{
				logger.Error("Error opening torrents.xml");
				return;
			}

			foreach(TorrentStorage torrentStore in torrentsToRestore){
				try{
					manager = addTorrent(torrentStore.TorrentPath, false, false, false, torrentStore.Settings);
				} catch (TorrentException e) {
					logger.Error(e.Message);
					continue;
				}
				
				torrentPreviousUpload.Add(manager, torrentStore.UploadedData);
				torrentPreviousDownload.Add(manager, torrentStore.DownloadedData);
				
				if(torrentStore.State == TorrentState.Downloading || torrentStore.State == TorrentState.Seeding){
					try{
						manager.Start();
					}catch{
						logger.Error("Could not restore state of " + manager.Torrent.Name);
						continue;
					}
				}
			}
		}
		
		public long GetPreviousDownload(TorrentManager manager){
			if(!torrentPreviousDownload.ContainsKey(manager))
				return 0;
				
			return torrentPreviousDownload[manager];
		}
		
		public long GetPreviousUpload(TorrentManager manager){
			if(!torrentPreviousUpload.ContainsKey(manager))
				return 0;
			
			return torrentPreviousUpload[manager];
		}
		
		public void SetFilePriority(TorrentFile torrentFile, Priority priority)
		{
			logger.Info("Changing priority of " + torrentFile.Path + " to " + priority);
			torrentFile.Priority = priority;
		}
		
		
		public TorrentManager GetSelectedTorrent()
		{
			return mainWindow.GetSelectedTorrent();
		}
	}
}
