//
// TorrentRssWatcher.cs
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
using System.Xml;
using System.Collections.Generic;
using System.Timers;
using System.Text.RegularExpressions;

using MonoTorrent.Common;
using Monsoon;

namespace Monsoon
{
	
	public class TorrentRssWatcher //: ITorrentWatcher
	{
		public event EventHandler<TorrentRssWatcherEventArgs> TorrentFound;
		public event EventHandler<TorrentRssWatcherEventArgs> TorrentLost;
		
		private string uri;
		
		private Timer timer;
		private List<RssFilter> filters;
		
		public TorrentRssWatcher(string uri) : this(uri, new List<Monsoon.RssFilter>())
		{
		}
		
		public TorrentRssWatcher(string uri, List<RssFilter> filters)
		{
			if(uri == null)
				throw new ArgumentNullException("uri");
			if(filters == null)
				throw new ArgumentNullException("filters");
			
			this.uri = uri;
			this.filters = filters;
		}
		
		
		public void ForceScan()
		{
			OnTimedEvent(null, null);	
		}
		
		
		public void StopWatching()
		{
			timer.Stop();
		}
		
		
		public void StartWatching()
		{
			if(timer != null)
				return;
			
			
			timer = new Timer();
			timer.Elapsed += OnTimedEvent;
			
			// Set the Interval to 30 seconds.
			timer.Interval=30000;
			timer.Enabled=true;
			
			timer.Start();
		}

		private void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			RssReader rssReader = new RssReader(uri);
			Console.Out.WriteLine("Ding!");
			foreach(RssItem item in rssReader.Items){
				//Console.Out.WriteLine("Item: " + item.Title);
				lock(filters) {
					foreach(RssFilter filter in filters){					
						Console.Out.WriteLine("Filter: " + filter.Include + " Item: " + item.Title);
						if(uri != filter.Feed && filter.Feed != "All"){
							continue;
						}
						
						if(filter.Include == string.Empty){
							continue;
						}
						
						Match m = Regex.Match(item.Title, filter.Include);
						if(!m.Success)
							continue;
						
						if(filter.Exclude != string.Empty){
							Match n = Regex.Match(item.Title, filter.Exclude);
							if(n.Success)
								continue;
						}
						
						RaiseTorrentFound(filter, item);
					}
				}
			}
		}
		
		public string Feed {
			get { return uri; }
		}
		
		public List<RssFilter> Filters {
			set { filters = value; }
		}
		
		private void RaiseTorrentFound(RssFilter filter, RssItem item)
		{
			if(TorrentFound != null)
				TorrentFound(this, new TorrentRssWatcherEventArgs(filter, item));
		}
		
//		private void RaiseTorrentLost(RssFilter filter, RssItem item)
//		{
//			if(TorrentLost != null)
//				TorrentLost(this, new TorrentRssWatcherEventArgs(filter, item));
//		}
		
	}
}
