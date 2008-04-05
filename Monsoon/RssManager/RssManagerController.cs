//
// RssManagerController.cs
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace Monsoon
{
	
	
	public class RssManagerController
	{
		
		private List<RssItem> history;
		private List<string> feeds;
		private List<RssFilter> filters;
		private Dictionary<string, TorrentRssWatcher> watchers;
		private List<RssItem> items;
	
		private TorrentController controller;
		
		// Eerrrmm
		private Gtk.ListStore historyListStore;
		
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		
		public RssManagerController(TorrentController controller)
		{
			history = new List<Monsoon.RssItem>();
			feeds = new List<string> ();
			filters = new List<Monsoon.RssFilter>();
			watchers = new Dictionary<string,Monsoon.TorrentRssWatcher>();
			items = new List<Monsoon.RssItem>();
			
			historyListStore = new Gtk.ListStore(typeof(RssItem));
			
			this.controller = controller;
		
			RestoreFeeds();
			RestoreHistory();
			RestoreFilters();
			
			
			RestoreWatchers();
			RefreshWatchers();
		}
		
		
		public bool AddWatcher(string url)
		{
			if(watchers.ContainsKey(url))
				return false;
			
			TorrentRssWatcher watcher = new TorrentRssWatcher(url);
			feeds.Add(url);
			watcher.TorrentFound += OnTorrentMatched;
			watchers.Add(url, watcher);
			watcher.StartWatching();
			return true;
		}
		
		public bool RemoveWatcher(string url)
		{
			if(!watchers.ContainsKey(url))
				return false;
			
			feeds.Remove(url);
			watchers[url].StopWatching();
			watchers.Remove(url);
				
			return true;
		}
		
		
		public void RestoreWatchers()
		{
			foreach(string feed in feeds){
				TorrentRssWatcher watcher = new TorrentRssWatcher(feed);
            			watcher.TorrentFound += OnTorrentMatched;
            			watchers.Add(feed, watcher);
            		}
			
		}
	
		public void StartWatchers()
		{
			foreach(TorrentRssWatcher watcher in watchers.Values){
				watcher.StartWatching();
			}
		}
		
		
		private void OnTorrentMatched(object sender, TorrentRssWatcherEventArgs args)
		{
			Console.Out.WriteLine("Found torrent: " + args.Item.Title + " Matched filter: " + args.Filter.Name);
		
			foreach (RssItem item in history) {
				if(item.Link == args.Item.Link){
					logger.Debug("Torrent already previously downloaded - " + item.Title);
					return;
				}
			}
			
			AddTorrent(args.Item, args.Filter);
		}
		
		
		// FIXME: Adding torrents not on the main loop, will throw up!
		// Solutions: Pop every add onto main loop resulting in blocking
		// or add async Load(uri, location) to library, or let the GUI
		// program handle downloading the torrent file 
		public void AddTorrent(RssItem item, RssFilter filter)
		{
			history.Add(item);
			historyListStore.AppendValues(item);
			
			if(filter == null){
				Console.Out.WriteLine("About to add with default savepath, URL: " + item.Link);
				try {
					controller.addTorrent(item.Link, true, false, false, null, controller.Engine.Settings.SavePath, true);
				} catch {
					logger.Error("RSS Manager: Unable to add - " + item.Title);
				}
			}
			else {
				Console.Out.WriteLine("About to add with custom savepath, Path: " + filter.SavePath);
				try{
					controller.addTorrent(item.Link, true, false, false, null, filter.SavePath, true);
				} catch {
					logger.Error("RSS Manager: Unabled to add - " + item.Title);
				}
			}
		}
		
		
		
		public void Store()
		{
			StoreFeeds();
			StoreHistory();
			StoreFilters();
		}
		
		public void StoreFeeds()
		{
			logger.Info ("Storing feeds");
			
			foreach(string feed in feeds){
				Console.Out.WriteLine("Storing feed: " + feed);
			}
			
			using (Stream fs = new FileStream (Defines.SerializedRssFeeds, FileMode.Create))
			{
				XmlWriter writer = new XmlTextWriter (fs, Encoding.UTF8);
				
				XmlSerializer s = new XmlSerializer (typeof(string[]));
				s.Serialize(writer, feeds.ToArray());
			}
		}
		
		
		public void StoreHistory()
		{	
			logger.Info ("Storing history");
	
			using (Stream fs = new FileStream (Defines.SerializedRssHistroy, FileMode.Create))
			{
				XmlWriter writer = new XmlTextWriter (fs, Encoding.UTF8);
				
				XmlSerializer s = new XmlSerializer (typeof(RssItem[]));
				s.Serialize(writer, history.ToArray());
			}
		}
		
		
		public void StoreFilters()
		{
			logger.Info ("Storing filters");
	
			using (Stream fs = new FileStream (Defines.SerializedRssFilters, FileMode.Create))
			{
				XmlWriter writer = new XmlTextWriter (fs, Encoding.UTF8);
				
				XmlSerializer s = new XmlSerializer (typeof(RssFilter[]));
				s.Serialize(writer, filters.ToArray ());
			}
		}
		
		
		public void RestoreFeeds()
		{
			string[] feedsToRestore = null;
			XmlSerializer xs = new XmlSerializer (typeof(string[]));
			
			logger.Info ("Restoring RSS feeds");
			
			try
			{
				if (!System.IO.File.Exists(Defines.SerializedRssFeeds))
					return;
				
				using (FileStream fs = File.OpenRead(Defines.SerializedRssFeeds))
					feedsToRestore = (string[]) xs.Deserialize(fs);
			}
			catch
			{
				logger.Error("Error opening rssfeeds.xml");
				return;
			}
			feeds.AddRange(feedsToRestore);
		}
		
		
		public void RestoreHistory()
		{
			RssItem[] historyToRestore = null;
			XmlSerializer xs = new XmlSerializer (typeof(RssItem[]));
			
			logger.Info ("Restoring RSS history");
			
			if (System.IO.File.Exists(Defines.SerializedRssHistroy)) {
				FileStream fs = null;
				
				try {
					fs = System.IO.File.Open(Defines.SerializedRssHistroy, System.IO.FileMode.Open);
				} catch {
					logger.Error("Error opening rsshistory.xml");
				}
				
				try {				
					historyToRestore = (RssItem[]) xs.Deserialize(fs);
				} catch {
					logger.Error("Failed to restore history");
					return;
				} finally {				
					fs.Close();
				}
				
				foreach(RssItem item in historyToRestore){
					history.Add(item);
					historyListStore.AppendValues(item);
				}
			}
		}
		
		
		public void RestoreFilters()
		{
			RssFilter [] filtersToRestore = null;
			XmlSerializer xs = new XmlSerializer (typeof(RssFilter[]));
			
			logger.Info ("Restoring RSS feeds");
			
			if (System.IO.File.Exists(Defines.SerializedRssFilters)) {
				FileStream fs = null;
				
				try {
					fs = System.IO.File.Open(Defines.SerializedRssFilters, System.IO.FileMode.Open);
				} catch {
					logger.Error("Error opening rssfilters.xml");
				}
				
				try {				
					filtersToRestore = (RssFilter[]) xs.Deserialize(fs);
				} catch {
					logger.Error("Failed to restore RSS filters");
					return;
				} finally {				
					fs.Close();
				}
				
				foreach(RssFilter filter in filtersToRestore){
					Console.Out.WriteLine("Filter:" + filter.Name);
					filters.Add(filter);
				}
			}
			
			RefreshWatchers();
		}
		
		public void AddFilter(RssFilter filter){
			if(!Filters.Contains(filter)){
				Filters.Add(filter);
				RefreshWatchers();
			}
		}
		
		public void RemoveFilter(RssFilter filter){
			if(Filters.Contains(filter)){
				Filters.Remove(filter);
				RefreshWatchers();
			}
		}
	
		
		public void RefreshWatchers()
		{
			foreach(TorrentRssWatcher watcher in watchers.Values){
	//			watcher.Includes.Clear();
	//			watcher.Excludes.Clear();
				watcher.Filters = filters;
			}
	/*	
			foreach(RssFilter filter in filters) {
				foreach(TorrentRssWatcher watcher in watchers.Values){
					if(filter.Feed == watcher.Feed || filter.Feed == "All" | filter.Feed == string.Empty){
						watcher.Includes.Add(filter.Include);
						watcher.Excludes.Add(filter.Exclude);
					}
				}
			}
	*/
		}
		
		public Gtk.ListStore HistoryListStore {
			get { return historyListStore; }
		}
		
		public List<RssItem> History {
			get { return history; }
		}
		
		public List<string> Feeds {
			get { return feeds; }
		}
		
		public List<RssFilter> Filters {
			get { return filters; }
		}
		
		public List<RssItem> Items {
			get { return items; }
		}
		
		public TorrentController TorrentController {
			get { return controller; }
		}
	}
}
