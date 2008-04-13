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
		
		private static NLog.Logger logger = MainClass.DebugEnabled ? NLog.LogManager.GetCurrentClassLogger () : new EmptyLogger ();
		
		
		public RssManagerController(TorrentController controller)
		{
			history = new List<Monsoon.RssItem>();
			feeds = new List<string>();
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
					controller.MainWindow.LoadTorrent(item.Link, true, false, false, null, controller.Engine.Settings.SavePath, true);
				} catch {
					logger.Error("RSS Manager: Unable to add - " + item.Title);
				}
			}
			else {
				Console.Out.WriteLine("About to add with custom savepath, Path: " + filter.SavePath);
				try{
					controller.MainWindow.LoadTorrent(item.Link, true, false, false, null, filter.SavePath, true);
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
			XmlRssFeedsController controller = new XmlRssFeedsController();
			
			controller.Settings.Clear();
			
			foreach(String feed in feeds) {
				controller.Settings.Add(feed);
			}
			 
			controller.Save();
		}
		
		
		public void StoreHistory()
		{	
			XmlRssHistoryController controller = new XmlRssHistoryController();
			controller.Settings.Clear();
			
			foreach(RssItem item in history) {
				controller.Settings.Add(item);
			}
			
			controller.Save();
		}
		
		
		public void StoreFilters()
		{
			XmlRssFiltersController controller = new XmlRssFiltersController();
			controller.Settings.Clear();
			
			foreach(RssFilter filter in filters) {
				controller.Settings.Add(filter);
			}
			
			controller.Save();			
		}
		
		
		public void RestoreFeeds()
		{
			XmlRssFeedsController controller = new XmlRssFeedsController();
			controller.Load();
			feeds = controller.Settings;
		}
		
		
		public void RestoreHistory()
		{
			XmlRssHistoryController controller = new XmlRssHistoryController();
			controller.Load();
			
			foreach(RssItem item in controller.Settings){
					history.Add(item);
					historyListStore.AppendValues(item);
			}
		}
		
		
		public void RestoreFilters()
		{
			XmlRssFiltersController controller = new XmlRssFiltersController();
			
			controller.Load();
			filters = controller.Settings;
			
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
