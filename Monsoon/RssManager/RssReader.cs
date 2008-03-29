//
// RssReader.cs
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

namespace Monsoon
{
	
	
	public class RssReader
	{
		private string uri;
		private XmlDocument document;
		private XmlNode channelNode;
		private List<RssItem> items;
		
		public RssReader(string uri)
		{
			this.uri = uri;
			document = new XmlDocument();
			items = new List<Monsoon.RssItem>();
			UpdateDocument();
		}
		
		public void UpdateDocument()
		{
			document.Load(new XmlTextReader(uri));
			ParseDocument();
		}
		
		
		private void ParseDocument()
		{
			XmlNode rssNode;
			
			XmlElement root = document.DocumentElement;
			
			if(!(root.Name == "rss"))
				return;
				
			rssNode = root;
			channelNode = null;
			
			foreach (XmlNode childNode in rssNode.ChildNodes){
				if(childNode.Name == "channel")
					channelNode = childNode;
			}
						
			foreach(XmlNode childNode in channelNode.ChildNodes){
				RssItem rssItem = new RssItem();
				
				rssItem.Feed = uri;
				
				foreach(XmlNode childNodeNode in childNode.ChildNodes){
					if(childNodeNode.Name == "title")
						rssItem.Title = childNodeNode.InnerText;
					if(childNodeNode.Name == "link")
						rssItem.Link = childNodeNode.InnerText;
					if(childNodeNode.Name == "pubDate")
						rssItem.Pubdate = childNodeNode.InnerText;
					if(childNodeNode.Name == "description")
						rssItem.Pubdate = childNodeNode.InnerText;
				}
				
				if(rssItem.Link != string.Empty)
					items.Add(rssItem);
			}
			
		}
		
		public List<RssItem> Items {
			get { return items; }
		}
	}
	
	public class RssItem
	{
		string author;
		string description;
		string link;
		string pubdate;
		string title;
		string feed;
		
		public RssItem()
		{
			author = string.Empty;
			description = string.Empty;
			link = string.Empty;
			pubdate = string.Empty;
			title = string.Empty;
			feed = string.Empty;
		}
		
		public RssItem(string author, string description, string link, string pubdate, string title)
		{
			this.author = author;
			this.description = description;
			this.link = link;
			this.pubdate = pubdate;
			this.title = title;
		}
		
		public string Author {
			get { return author; }
			set { author = value; }
		}
		
		public string Description {
			get { return description; }
			set { description = value; }
		}
		
		public string Link { 
			get { return link; }
			set { link = value; }
		}
		
		public string Pubdate{
			get { return pubdate; }
			set { pubdate = value; }
		}
		
		public string Title{
			get { return title; }
			set { title = value; }
		}
		
		public string Feed {
			get { return feed; }
			set { feed = value; }
		}
		
		/*
		public override bool Equals (object o)
		{
			if(o == null)
				return false;
			
			if(this.GetType() != o.GetType())
				return false;
			
			RssItem item = (RssItem) o;
			
			if(this.Link != item.Link)
				return false;
			
			return true;
		}
		*/

	}
	
	
}
