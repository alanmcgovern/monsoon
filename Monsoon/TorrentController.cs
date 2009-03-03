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

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Xml.Serialization;
using MonoTorrent.TorrentWatcher;

namespace Monsoon
{
	public class TorrentController : IService
	{
		public event EventHandler<DownloadAddedEventArgs> Added;
		public event EventHandler<DownloadAddedEventArgs> Removed;
		public event EventHandler<ShouldAddEventArgs> ShouldAdd;
		public event EventHandler<ShouldRemoveEventArgs> ShouldRemove;
		public event EventHandler SelectionChanged;

		public bool Initialised {
			get; private set;
		}
		
		public Download SelectedDownload {
			get { return SelectedDownloads.Count == 1 ? SelectedDownloads [0] : null; }
		}
		
		public List <Download> SelectedDownloads {
			get; private set;
		}
		
		private ClientEngine engine;

		private List<Download> allTorrents;
		private List<FastResume> fastResume;
		public List<FastResume> FastResume
		{
			get { return fastResume; }
		}
		
		private static NLog.Logger logger = MainClass.DebugEnabled ? NLog.LogManager.GetCurrentClassLogger () : new EmptyLogger ();
		
		TorrentSettings defaultTorrentSettings;
		public TorrentController()
		{
			this.defaultTorrentSettings = SettingsManager.DefaultTorrentSettings;
			this.SelectedDownloads = new List<Download> ();
			
			Ticker.Tick ();
			fastResume = LoadFastResume();
			Ticker.Tock ("Fast Resume");
			
			Ticker.Tick ();
			engine = new ClientEngine(SettingsManager.EngineSettings);
			Ticker.Tock ("Client engine");

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

		public void Initialise ()
		{
			Initialised = true;
		}
		
		public bool addTorrent (string path, bool ask, out string error)
		{
			Torrent torrent;
			error = null;
			
			if (!Torrent.TryLoad (path, out torrent)) {
				error = _("Invalid torrent selected");
				return false;
			}
			
			if (Engine.Contains(torrent)) {
				error = _("Torrent has already been added");
				return false;
			}
			
			string savePath = engine.Settings.SavePath;
			if (ask) {
				EventHandler<ShouldAddEventArgs> h = ShouldAdd;
				if (h != null) {
					ShouldAddEventArgs e = new ShouldAddEventArgs (torrent, savePath);
					h (this, e);
					if (!e.ShouldAdd)
						return true;
					savePath = e.SavePath;
				}
			}

			try
			{
				addTorrent (torrent, savePath);
			}
			catch (Exception ex)
			{
				Console.WriteLine (ex);
				error = _("An unexpected error occured while loading the torrent. {0}");
				error = string.Format (error, ex.Message);
				return  false;
			}
			return true;
		}
		// TODO: Refactor all of these functions!!!
		public Download addTorrent(Torrent torrent)
		{
			return addTorrent(torrent, SettingsManager.Preferences.StartNewTorrents);
		}
		
		public Download addTorrent (Torrent torrent, string savePath)
		{
			return addTorrent(torrent, SettingsManager.Preferences.StartNewTorrents, SettingsManager.Preferences.RemoveOnImport, null, savePath, false);
		}
		
		public Download addTorrent(Torrent torrent, bool startTorrent)
		{
			return addTorrent(torrent, startTorrent, SettingsManager.Preferences.RemoveOnImport, null);
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

			TorrentSettings settings = savedSettings ?? defaultTorrentSettings.Clone ();
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
			
			Event.Raise<DownloadAddedEventArgs> (Added, this, new DownloadAddedEventArgs (manager));
			allTorrents.Add (manager);
			
			if (startTorrent) {
				logger.Info("Auto starting torrent " + manager.Torrent.Name);
				manager.Start();
			}
					
			logger.Info ("Added torrent " + manager.Torrent.Name);
			
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
			if (SettingsManager.Preferences.TorrentStorageLocation == Directory.GetParent(torrentPath).ToString()) {
				logger.Info ("Torrent was already in the storage folder");
				return;
			}
			
			string newPath = Path.Combine(SettingsManager.Preferences.TorrentStorageLocation, Path.GetFileName(torrentPath));
			logger.Debug("Copying torrent to " + newPath);
			if (File.Exists (newPath))
				File.Delete (newPath);
			
			File.WriteAllBytes (newPath, File.ReadAllBytes (torrentPath));
			
			Torrent t = Torrent.Load (newPath);
			for (int i=0; i < torrent.Files.Length; i++)
				t.Files[i].Priority = torrent.Files[i].Priority;
			
			torrent = t;
		}

		public List<Download> Torrents
		{
			get{ return allTorrents; }
		}
		
		public ClientEngine Engine {
			get {
				return engine;
			}
		}

		public void Select (IEnumerable <Download> downloads)
		{
			SelectedDownloads.Clear ();
			SelectedDownloads.AddRange (downloads);
			Event.Raise (SelectionChanged, this, EventArgs.Empty);
		}
		
		public void RemoveTorrent()
		{
			RemoveTorrent (false);
		}
		
		public void RemoveTorrent(bool deleteTorrent)
		{
			RemoveTorrent(deleteTorrent, false);
		}
		
		public void RemoveTorrent(bool deleteTorrent, bool deleteData)
		{
			EventHandler <ShouldRemoveEventArgs> h = ShouldRemove;
			if (h != null) {
				ShouldRemoveEventArgs e = new ShouldRemoveEventArgs (SelectedDownloads, deleteData, deleteTorrent); 
				h (this, e);
				if (!e.ShouldRemove)
					return;
			}
			
			foreach (Download torrent in SelectedDownloads) {
				if(torrent.State != Monsoon.State.Stopped)
					torrent.Stop();
	
				allTorrents.Remove (torrent);
				
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
					
					// FIXME: Fast resume is central now, not individual for each torrent.
					try{
	                    logger.Info("Deleting torrent fast resume file " + torrent.Torrent.TorrentPath);
	                    File.Delete(torrent.Torrent.TorrentPath + ".fresume");
	                } catch {
	                    logger.Error("Unable to delete " + torrent.Torrent.TorrentPath + ".fresume");
	                }
				}
				
				engine.Unregister(torrent.Manager);
				fastResume.RemoveAll (delegate (FastResume f) {
					return Toolbox.ByteMatch (f.InfoHash, torrent.Torrent.InfoHash); 
				});
				
				logger.Info("Removed torrent " + torrent.Torrent.Name);
			}
			foreach (Download download in new List<Download> (SelectedDownloads))
				Event.Raise<DownloadAddedEventArgs> (Removed, this, new DownloadAddedEventArgs (download));
			SelectedDownloads.Clear ();
			Select (SelectedDownloads);
		}
		
		public void LoadStoredTorrents()
		{
			Download manager;
			
			List<TorrentStorage> torrents =new List<TorrentStorage> ();
			SettingsManager.Restore <List<TorrentStorage>> (torrents);
			
			foreach(TorrentStorage torrentStore in torrents){
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
				
				if(torrentStore.State == Monsoon.State.Downloading ||
				   torrentStore.State == Monsoon.State.Seeding ||
				   torrentStore.State == Monsoon.State.Queued) {
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
		
		private static string _(string s)
		{
			return Mono.Unix.Catalog.GetString(s);
		}
	}
}
