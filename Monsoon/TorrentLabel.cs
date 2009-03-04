//
// TorrentLabel.cs
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
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

using Gtk;

using MonoTorrent.Common;
using MonoTorrent.Client;

namespace Monsoon
{
	[XmlRoot("Label")]
	public class TorrentLabel
	{
		private bool immutable;
		private string name;
		private Gdk.Pixbuf icon;
		private string iconPath;
		private string[] torrentPaths;
		private List<Download> torrents;
		
		private static NLog.Logger logger = MainClass.DebugEnabled ? NLog.LogManager.GetCurrentClassLogger () : new EmptyLogger ();
		
		
		public TorrentLabel()
		{
			torrents = new List<Download> ();
			icon = Gtk.IconTheme.Default.LoadIcon("gtk-about", 16, 0);
		}
		
		
		public TorrentLabel(string name) : this(name, "gtk-about")
		{
		}
		
		public TorrentLabel(string name, string iconPath)
			: this (name, iconPath, false)
		{
		}
		
		public TorrentLabel(string name, string iconPath, bool immutable)
		{
			Gdk.Pixbuf icon;
			this.immutable = immutable;
			if(!System.IO.File.Exists(iconPath)){
				logger.Debug("File " + iconPath + " does not exist, trying stock icon");
				icon = Gtk.IconTheme.Default.LoadIcon(iconPath, 16, 0);
			} else {
				logger.Debug("Loading icon from path: " + iconPath);
				icon = new Gdk.Pixbuf(iconPath, 16, 16);
			}

			this.iconPath = iconPath;
			this.torrents = new List<Download> ();
			this.name = name;
			this.icon = icon;
		}
		
		[XmlIgnore]
		public List<Download> Torrents {
			get { return torrents; }
		}
		
		public string Name {
			get { return name; }
			set { name = value; }
		}
		
		public bool Immutable
		{
			get { return immutable; }
			set { immutable = value; }
		}
		
		[XmlElement("Torrent")]
		public string [] TorrentPaths
		{
			get { 
				List<string> list = new List<string> ();
				foreach(Download manager in torrents){
					list.Add(manager.Manager.Torrent.TorrentPath);
				}
				return list.ToArray ();
			}
			
			set{ torrentPaths = value; }
		}
		
		
		// Eeewww!
		[XmlIgnore]
		public string[] TruePaths
		{
			get { return torrentPaths; } 
		}
		
		public string IconPath	{
			get { return iconPath; }
			set { 
				if (System.IO.File.Exists(value)) {
					icon = new Gdk.Pixbuf(value, 16, 16);
					iconPath = value;
				}
			}
		}
		
		[XmlIgnore]
		public Gdk.Pixbuf Icon {
			get { return icon; }
		}
		
		[XmlIgnore]
		public int Size
		{
			get { return torrents.Count; }
		}

		public bool AddTorrent(Download manager)
		{
			if(torrents.Contains(manager))
				return false;
			
			torrents.Add(manager);
			
			return true;
		}
		
		
		public bool RemoveTorrent(Download manager)
		{
			if(!torrents.Contains(manager))
				return false;
			
			torrents.Remove(manager);
			
			return true;
		}
	}
}
