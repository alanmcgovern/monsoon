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
		public event EventHandler<TorrentRssWatcherEventArgs> TorrentFound;
		
		private List<RssItem> history;
		private List<string> feeds;
		private List<RssFilter> filters;
		private Dictionary<string, TorrentRssWatcher> watchers;
		private List<RssItem> items;
		
		// Eerrrmm
		private Gtk.ListStore historyListStore;
		private MonoTorrent.Client.EngineSettings settings;
		private static NLog.Logger logger = MainClass.DebugEnabled ? NLog.LogManager.GetCurrentClassLogger () : new EmptyLogger ();
		
		public string SavePath {
			get { return settings.SavePath; }
		}
		
		public RssManagerController(MonoTorrent.Client.EngineSettings settings)
		{
			this.settings = settings;
			history = new List<Monsoon.RssItem>();
			feeds = new List<string>();
			filters = new List<Monsoon.RssFilter>();
			watchers = new Dictionary<string,Monsoon.TorrentRssWatcher>();
			items = new List<Monsoon.RssItem>();
			
			historyListStore = new Gtk.ListStore(typeof(RssItem));
			
		
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
			watcher.TorrentFound += delegate(object o, TorrentRssWatcherEventArgs e) {
				GLib.Idle.Add(delegate {
					OnTorrentMatched(o, e);
					return false;
				});
			};
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
			foreach (string feed in feeds)
			{
				TorrentRssWatcher watcher = new TorrentRssWatcher(feed);
				watcher.TorrentFound += delegate(object o, TorrentRssWatcherEventArgs e) {
					GLib.Idle.Add(delegate {
						OnTorrentMatched(o, e);
						return false;
					});
				};
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
			
			AddTorrent(args);
		}
		
		
		// FIXME: Adding torrents not on the main loop, will throw up!
		// Solutions: Pop every add onto main loop resulting in blocking
		// or add async Load(uri, location) to library, or let the GUI
		// program handle downloading the torrent file 
		public void AddTorrent(TorrentRssWatcherEventArgs args)
		{
			history.Add(args.Item);
			historyListStore.AppendValues(args.Item);
			
			Event.Raise<TorrentRssWatcherEventArgs>(TorrentFound, this, args);
		}
		
		
		
		public void Store()
		{
			StoreFeeds();
			StoreHistory();
			StoreFilters();
		}
		
		public void StoreFeeds()
		{
			SettingsManager.Store <List <string>> (feeds);
		}
		
		
		public void StoreHistory()
		{	
			SettingsManager.Store <List <RssItem>> (history);
		}
		
		
		public void StoreFilters()
		{
			SettingsManager.Store <List <RssFilter>> (filters);	
		}
		
		
		public void RestoreFeeds()
		{
			SettingsManager.Restore <List <string>> (feeds);
		}
		
		
		public void RestoreHistory()
		{
			SettingsManager.Restore <List <RssItem>> (history);
			foreach (RssItem item in history)
				historyListStore.AppendValues (item);
		}
		
		
		public void RestoreFilters()
		{
			SettingsManager.Restore <List <RssFilter>> (filters);
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
	}
}
