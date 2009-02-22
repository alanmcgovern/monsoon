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
		public event EventHandler Added;
		public event EventHandler Removed;
		public event EventHandler SelectionChanged;
		
		public Download SelectedDownload {
			get { return GetSelectedTorrent (); }
		}
		
		private ClientEngine engine;
		private PreferencesSettings prefSettings;
		private MainWindow mainWindow;
		private Download completedManager;
		
		private List<Download> torrentsDownloading;
		private List<Download> torrentsSeeding;
		private List<Download> allTorrents;
		private List<TorrentLabel> labels;
		private List<FastResume> fastResume;
		public List<FastResume> FastResume
		{
			get { return fastResume; }
		}
		public MainWindow MainWindow
		{
			get { return mainWindow; }
		}
		
		private static NLog.Logger logger = MainClass.DebugEnabled ? NLog.LogManager.GetCurrentClassLogger () : new EmptyLogger ();
		
		public TorrentController(MainWindow mainWindow)
		{
			this.prefSettings = mainWindow.Preferences;
			this.labels = mainWindow.Labels;
			this.mainWindow = mainWindow;
			
			Ticker.Tick ();
			fastResume = LoadFastResume();
			Ticker.Tock ("Fast Resume");
			
			Ticker.Tick ();
			engine = new ClientEngine(mainWindow.EngineSettings);
			Ticker.Tock ("Client engine");

			torrentsDownloading = new List<Download>();
			torrentsSeeding = new List<Download>(); 
			allTorrents = new List<Download>();
		}
		
		public void StoreFastResume ()
		{
			try
			{
				logger.Info("storing fast resume");
				BEncodedList list = new BEncodedList();
				foreach (Download t in Torrents)
					list.Add(t.SaveFastResume().Encode ());
				File.WriteAllBytes(Defines.SerializedFastResume, list.Encode());
			}
			catch (Exception ex)
			{
				logger.Warn (string.Format("Couldn't store fast resume: {0}", ex));
			}
		}
		
		private List<FastResume> LoadFastResume()
		{
			List<FastResume> list = new List<FastResume>();
			try
			{
				logger.Info("loading fast resume");
				BEncodedList blist = BEncodedValue.Decode<BEncodedList> (File.ReadAllBytes (Defines.SerializedFastResume));
				
				foreach (BEncodedDictionary resume in blist)
					list.Add (new FastResume (resume));
			}
			catch (Exception ex)
			{
				logger.Warn (string.Format("Couldn't load fast resume: {0}", ex));
			}
			finally
			{
				try {
					File.Delete (Defines.SerializedFastResume);
				}
				catch {
					// I don't care if this happens
				}
			}
			
			return list;
		}

		// TODO: Refactor all of these functions!!!
		public Download addTorrent(Torrent torrent)
		{
			return addTorrent(torrent, prefSettings.StartNewTorrents);
		}
		
		public Download addTorrent (Torrent torrent, string savePath)
		{
			return addTorrent(torrent, prefSettings.StartNewTorrents, prefSettings.RemoveOnImport, null, savePath, false);
		}
		
		public Download addTorrent(Torrent torrent, bool startTorrent)
		{
			return addTorrent(torrent, startTorrent, prefSettings.RemoveOnImport, null);
		}
		public Download addTorrent(Torrent torrent, bool startTorrent, bool removeOriginal, TorrentSettings savedSettings)
		{
			return addTorrent(torrent, startTorrent, removeOriginal, savedSettings, engine.Settings.SavePath, false);
		}
		public Download addTorrent(Torrent torrent, bool startTorrent, bool removeOriginal, TorrentSettings savedSettings, string savePath, bool isUrl)
		{
			string originalPath = torrent.TorrentPath;
			Download manager;
			
			if(!Directory.Exists(savePath))
				throw new TorrentException("Torrent save path does not exist, " + savePath);
			
			// Check to see if torrent already exists
			if (engine.Contains (torrent)) {
				logger.Error ("Failed to add torrent, " + torrent.Name + " already exists.");
				throw new TorrentException ("Failed to add torrent, " + torrent.Name + " already exists.");
			}
			
			// Move the .torrent to the local storage folder if it's not there already
			MoveToStorage (ref torrent);

			TorrentSettings settings = savedSettings ?? mainWindow.DefaultTorrentSettings.Clone ();
			FastResume resume = this.fastResume.Find(delegate (FastResume f) { return Toolbox.ByteMatch(f.InfoHash, torrent.InfoHash); });
			
			if (resume != null)
				manager = new Download(new TorrentManager (torrent, savePath, settings, resume));
			else
				manager = new Download(new TorrentManager (torrent, savePath, settings));
					
			engine.Register(manager.Manager);
			
			if (removeOriginal) {
				logger.Info ("Removing {0}", originalPath);
				File.Delete (originalPath);
			}
			
			mainWindow.Torrents.Add (manager, mainWindow.TorrentListStore.AppendValues(manager));
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
			
			manager.StateChanged += delegate(object o, TorrentStateChangedEventArgs e) {
				GLib.Idle.Add(delegate {
					OnTorrentStateChanged(o, e);
					return false;
				});
			};
			
			// add to "All" label
			mainWindow.AllLabel.AddTorrent(manager);
			
			mainWindow.StoreTorrentSettings();
			
			return manager;
		}
		
		private void MoveToStorage (ref Torrent torrent)
		{
			string torrentPath = torrent.TorrentPath;
			
			if (torrentPath == null) {
				logger.Info ("Couldn't move torrent, path was null");
				return;
			}
			// Torrent already in storage
			if (prefSettings.TorrentStorageLocation == Directory.GetParent(torrentPath).ToString()) {
				logger.Info ("Torrent was already in the storage folder");
				return;
			}
			
			string newPath = Path.Combine(prefSettings.TorrentStorageLocation, Path.GetFileName(torrentPath));
			logger.Debug("Copying torrent to " + newPath);
			if (File.Exists (newPath))
				File.Delete (newPath);
			
			File.WriteAllBytes (newPath, File.ReadAllBytes (torrentPath));
			
			Torrent t = Torrent.Load (newPath);
			for (int i=0; i < torrent.Files.Length; i++)
				t.Files[i].Priority = torrent.Files[i].Priority;
			
			torrent = t;
		}

		private void OnTorrentStateChanged(object sender, TorrentStateChangedEventArgs args)
		{
			Download manager = (Download)sender;
			completedManager = manager;
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
			} else if (args.NewState == TorrentState.Stopped) {
				MainWindow.PeerListStore.Clear ();
			}
		
			if (!prefSettings.EnableNotifications)
				return;
			if (args.NewState != TorrentState.Seeding)
				return;
			if (args.OldState != TorrentState.Downloading)
				return;

			Notifications.Notification notify = new Notifications.Notification (_("Download Complete"), manager.Torrent.Name, Stock.GoDown);
			if (prefSettings.EnableTray)
				notify.AttachToWidget (mainWindow.TrayIcon);
			notify.Urgency = Notifications.Urgency.Low;
			notify.Timeout = 5000;
			notify.Show ();
			notify.AddAction("reveal-item", "Show", OnRevealActivated);
		}

		private void OnRevealActivated (object o, Notifications.ActionArgs args)
		{
			if (completedManager == null)
				return;
			System.Diagnostics.Process.Start("\"file://" + completedManager.SavePath + "\"");
		}

		public List<Download> TorrentsDownloading
		{
			get{ return torrentsDownloading; }
		}
		
		public List<Download> Torrents
		{
			get{ return allTorrents; }
		}
		
		public List<Download> TorrentsSeeding
		{
			get { return torrentsSeeding; }
		}

		public ClientEngine Engine {
			get {
				return engine;
			}
		}
		
		public void removeTorrent(Download torrent)
		{
			removeTorrent(torrent, false);
		}
		
		public void removeTorrent(Download torrent, bool deleteTorrent)
		{
			removeTorrent(torrent, deleteTorrent, false);
		}
		
		public void removeTorrent(Download torrent, bool deleteTorrent, bool deleteData)
		{
			if(torrent.State != TorrentState.Stopped)
				torrent.Stop();
			
			TreeIter iter = MainWindow.Torrents [torrent];
			mainWindow.TorrentListStore.Remove(ref iter);
			MainWindow.Torrents.Remove(torrent);
			allTorrents.Remove(torrent);
			
			if(deleteData){
				logger.Info("Deleting {0} data", torrent.Torrent.Name);
				try{
					if (Directory.Exists(Path.Combine(torrent.SavePath, torrent.Torrent.Name)))
						Directory.Delete(Path.Combine(torrent.SavePath, torrent.Torrent.Name), true);
					else
						File.Delete(Path.Combine(torrent.SavePath, torrent.Torrent.Name));
				} catch (Exception e) {
					logger.Error("Failed to delete {0}: {1}", Path.Combine(torrent.SavePath, torrent.Torrent.Name), e.Message);
				}
			}
			
			if(deleteTorrent){
				try{
					logger.Info("Deleting torrent file {0} ", torrent.Torrent.TorrentPath);
					File.Delete(torrent.Torrent.TorrentPath);
				} catch {
					logger.Error("Unable to delete " + torrent.Torrent.TorrentPath);
				}
				
				try{
                    logger.Info("Deleting torrent fast resume file " + torrent.Torrent.TorrentPath);
                    File.Delete(torrent.Torrent.TorrentPath + ".fresume");
                } catch {
                    logger.Error("Unable to delete " + torrent.Torrent.TorrentPath + ".fresume");
                }
			}
			
			foreach(TorrentLabel label in labels){
				label.RemoveTorrent(torrent);
			}
			
			logger.Info("Removed torrent " + torrent.Torrent.Name);
			mainWindow.AllLabel.RemoveTorrent(torrent);

			fastResume.RemoveAll (delegate (FastResume f) {
				return Toolbox.ByteMatch (f.InfoHash, torrent.Torrent.InfoHash); 
			});
			
			engine.Unregister(torrent.Manager);
			mainWindow.StoreTorrentSettings();
		}
		
		public void OnTorrentFound(object sender, TorrentWatcherEventArgs args)
		{
			if(!prefSettings.ImportEnabled)
				return;
				
			logger.Info("New torrent detected, adding " + args.TorrentPath);
			string newPath = Path.Combine(MainWindow.Preferences.TorrentStorageLocation, Path.GetFileName(args.TorrentPath));
			if (Path.GetDirectoryName (args.TorrentPath) != MainWindow.Preferences.TorrentStorageLocation)
			{
				logger.Info ("Copying: {0} to {1}", args.TorrentPath, newPath);
				File.WriteAllBytes (newPath, File.ReadAllBytes (args.TorrentPath));
				if(prefSettings.RemoveOnImport)
					File.Delete(args.TorrentPath);
			}
			
			GLib.Timeout.Add (1000, delegate {
				try {
					mainWindow.LoadTorrent (newPath, false);
					return false;
				}
				catch (Exception ex) {
					Console.WriteLine (ex);
					return false;
				}
			});
		}
		
		public void LoadStoredTorrents()
		{
			Download manager;
			
			XmlTorrentStorageController controller = new XmlTorrentStorageController();
			controller.Load();

			foreach(TorrentStorage torrentStore in controller.Settings){
				try{
					Torrent t = Torrent.Load (torrentStore.TorrentPath);
					manager = addTorrent(t, false, false, torrentStore.Settings);
				} catch (TorrentException e) {
					logger.Error(e.Message);
					continue;
				} catch (IOException) {
					logger.Warn ("Torrent '{0}' could not be restored. File didn't exist", torrentStore.TorrentPath);
					continue;
				} catch (Exception ex) {
					logger.Error ("Torrent '{0}' could not be restored: {0}", ex);
					continue;
				}
				
				foreach(TorrentFile file in manager.Manager.FileManager.Files) {
					foreach(TorrentFileSettingsModel settings in torrentStore.Files) {
						if (settings.Path != file.Path)
							continue;
						file.Priority = settings.Priority;
					}
				}				
				
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

		public void SetFilePriority(TorrentFile torrentFile, Priority priority)
		{
			logger.Info("Changing priority of " + torrentFile.Path + " to " + priority);
			torrentFile.Priority = priority;
		}
		
		public Download GetSelectedTorrent()
		{
			return mainWindow.GetSelectedTorrent();
		}
		
		private static string _(string s)
		{
			return Mono.Unix.Catalog.GetString(s);
		}
	}
}
