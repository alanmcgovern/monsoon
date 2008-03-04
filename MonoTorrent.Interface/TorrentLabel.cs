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
using System.Collections;
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
		private bool canRemove;
		private string name;
		private Gdk.Pixbuf icon;
		private string iconPath;
		private string[] torrentPaths;
		
		// Temporary solution until TreeModelFilter is able to be subclassed
		private ListStore model;
		private ArrayList torrents;
		
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		
		public TorrentLabel()
		{
			torrents = new ArrayList();
			icon = Gtk.IconTheme.Default.LoadIcon("gtk-about", 16, 0);
			model = new ListStore (typeof(TorrentManager));
			canRemove = true;
		}
		
		
		public TorrentLabel(ArrayList torrents) : this(torrents, null)
		{		
		}
		
		
		public TorrentLabel(ArrayList torrents, string name) : this(torrents, name, "gtk-about")
		{
		}
		
		public TorrentLabel(ArrayList torrents, string name, string iconPath)
			: this (torrents, name, "gtk-about", true)
		{
			
		}
		
		public TorrentLabel(ArrayList torrents, string name, string iconPath, bool canRemove)
		{
			Gdk.Pixbuf icon;
			this.canRemove = canRemove;
			if(!System.IO.File.Exists(iconPath)){
				logger.Info("File " + iconPath + " does not exist, trying stock icon");
				//icon = Gtk.IconTheme.Default.LoadIcon("gtk-about", 16, 0);
				icon = Gtk.IconTheme.Default.LoadIcon(iconPath, 16, 0);
			} else {
				logger.Info("Loading icon from path: " + iconPath);
				icon = new Gdk.Pixbuf(iconPath, 16, 16);
			}

			this.iconPath = iconPath;
			this.torrents = torrents;
			this.name = name;
			this.icon = icon;
			model = new ListStore (typeof(TorrentManager));
		}
		
		[XmlIgnore]
		public ArrayList Torrents {
			get { return torrents; }
		}
		
		[XmlAttribute("Name")]
		public string Name {
			get { return name; }
			set { name = value; }
		}
		
		[XmlAttribute("CanRemove")]
		public bool CanRemove
		{
			get { return canRemove; }
			set { canRemove = value; }
		}
		
		[XmlElement("Torrent")]
		public string [] TorrentPaths
		{
			get { 
				ArrayList list = new ArrayList();
				foreach(TorrentManager manager in torrents){
					list.Add(manager.Torrent.TorrentPath);
				}
				return (string[])list.ToArray(typeof(string));
			}
			
			set{ torrentPaths = value; }
		}
		
		
		// Eeewww!
		[XmlIgnore]
		public string[] TruePaths
		{
			get { return torrentPaths; } 
		}
		
		
		//[XmlElement("Icon")]
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
			get { 
				/*if(name == "All"){
					return torrents.Count;				
				} else if(name == "Downloading"){
					return getTotalStates(TorrentState.Downloading);
				} else if(name == "Seeding"){
					return getTotalStates(TorrentState.Seeding);
				} else{
					return torrents.Count;
				}*/
				if(model == null)
					return 0;
				else
					return model.IterNChildren();
			}
		}
		
		
		[XmlIgnore]
		public ListStore Model {
			get { return model; }
			set { model = value; }
		}
	
	
		public bool AddTorrent(TorrentManager manager)
		{
			if(torrents.Contains(manager))
				return false;
			
			torrents.Add(manager);
			model.AppendValues(manager);
			
			return true;
		}
		
		
		public bool RemoveTorrent(TorrentManager manager)
		{
			TreeIter iter = TreeIter.Zero;
			
			if(!torrents.Contains(manager))
				return false;
			
			torrents.Remove(manager);
			
			if(!GetTorrentIter(manager, out iter))
				return false;
		
			if(!model.Remove(ref iter))
				return false;
			
			return true;
		}
		
		
		private bool GetTorrentIter(TorrentManager manager, out TreeIter iter)
		{
			
			if(!model.GetIterFirst(out iter))
				return false;
			
			do{
				if(manager == (TorrentManager) model.GetValue (iter, 0))
					return true;
			} while (model.IterNext(ref iter));
			
			return false;
		}
		
	}
}
