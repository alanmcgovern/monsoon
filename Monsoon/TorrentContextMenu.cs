//
// TorrentContextMenu.cs
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
using System.Diagnostics;
using Gtk;
using MonoTorrent.Client;
using MonoTorrent.Common;

namespace Monsoon
{
	
	
	public class TorrentContextMenu : Gtk.Menu
	{
		public event EventHandler DeleteTorrent;
		public event EventHandler RemoveTorrent;
		
		private TorrentController torrentController;
		private TorrentManager selectedTorrent;
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		public TorrentContextMenu(TorrentController torrentController)
		{
			this.torrentController = torrentController;
			
			ImageMenuItem openItem = new ImageMenuItem("Open");
			ImageMenuItem startItem = new ImageMenuItem("Start/Pause");
			ImageMenuItem stopItem  = new ImageMenuItem("Stop");
			ImageMenuItem removeItem  = new ImageMenuItem("Remove");
			ImageMenuItem deleteItem  = new ImageMenuItem("Delete");
			ImageMenuItem recheckItem  = new ImageMenuItem("Force Re-_check");
			//ImageMenuItem hashItem  = new ImageMenuItem("Force Re-_hash");
			ImageMenuItem announceItem  = new ImageMenuItem("Force _announce");
			
			openItem.Image = new Image(Stock.Open, IconSize.Menu);
			startItem.Image = new Image(Stock.MediaPlay, IconSize.Menu);
			stopItem.Image = new Image(Stock.MediaStop, IconSize.Menu);
			removeItem.Image = new Image(Stock.Remove, IconSize.Menu);
			deleteItem.Image = new Image(Stock.Delete, IconSize.Menu);
			recheckItem.Image = new Image(Stock.Refresh, IconSize.Menu);
			//hashItem.Image = new Image(Stock.Convert, IconSize.Menu);
			announceItem.Image = new Image(Stock.Network, IconSize.Menu);
			
			openItem.Activated += OnOpenItemActivated;
			startItem.Activated += OnStartItemActivated;
			stopItem.Activated += OnStopItemActivated;
			removeItem.Activated += delegate {
				if (RemoveTorrent != null)
					RemoveTorrent (this, EventArgs.Empty);
			};
			
			deleteItem.Activated += delegate {
				if (DeleteTorrent != null)
					DeleteTorrent(this, EventArgs.Empty);
			};
			recheckItem.Activated += OnRecheckItemActivated;
			//hashItem.Activated += OnHashItemActivated;
			announceItem.Activated += OnAnnounceItemActivated;
			
			Append(openItem);
			Append(new SeparatorMenuItem());
			Append(startItem);
			Append(stopItem);
			Append(removeItem);
			Append(deleteItem);
			Append(new SeparatorMenuItem());
			Append(recheckItem);
			//Append(hashItem);
			Append(announceItem);
			
			selectedTorrent = torrentController.GetSelectedTorrent();
			if (selectedTorrent == null)
				return;
			
			switch(selectedTorrent.State)
			{
				case TorrentState.Downloading:
					startItem.Image = new Image(Stock.MediaPause, IconSize.Menu);
					break;
				case TorrentState.Seeding:
					startItem.Image = new Image(Stock.MediaPause, IconSize.Menu);
					break;
				case TorrentState.Stopped:
					stopItem.Sensitive = false;
					break;
				default:
					break;
			}
		}
		
		private void OnStartItemActivated(object sender, EventArgs args)
		{
			selectedTorrent = torrentController.GetSelectedTorrent();
			if (selectedTorrent == null)
				return;
			
			if(selectedTorrent.State == TorrentState.Seeding || selectedTorrent.State == TorrentState.Downloading){
				try{
					selectedTorrent.Pause();
				} catch(Exception){
					logger.Warn("Unable to pause " + selectedTorrent.Torrent.Name);
				}
			}else{
				try{
					selectedTorrent.Start();
				}catch(Exception){
				
				}	logger.Warn("Unable to start " + selectedTorrent.Torrent.Name);
			}
		}
		
		private void OnStopItemActivated(object sender, EventArgs args)
		{
			selectedTorrent = torrentController.GetSelectedTorrent();
			if (selectedTorrent == null)
				return;
			
			try{
				selectedTorrent.Stop();
			} catch(Exception)
			{
				logger.Warn("Unable to stop " + selectedTorrent.Torrent.Name);
			}
		}
		
		private void OnRecheckItemActivated(object sender, EventArgs args)
		{
			selectedTorrent = torrentController.GetSelectedTorrent();
			if (selectedTorrent == null)
				return;
			
			try{
				if(selectedTorrent.State != TorrentState.Stopped)
					selectedTorrent.Stop();
				selectedTorrent.HashCheck(false);
			} catch(Exception){
				logger.Warn("Unable to force re-hash on " + selectedTorrent.Torrent.Name);
			}
		}
				
		private void OnAnnounceItemActivated(object sender, EventArgs args)
		{
			selectedTorrent = torrentController.GetSelectedTorrent();
			if (selectedTorrent == null)
				return;
			
			try{
				selectedTorrent.TrackerManager.Announce();
			} catch(Exception){
				logger.Warn("Unable to force announce on " + selectedTorrent.Torrent.Name);
			}
		}
		
		private void OnOpenItemActivated(object sender, EventArgs args)
		{
			selectedTorrent = torrentController.GetSelectedTorrent();
			if (selectedTorrent == null)
				return;
			
			if (selectedTorrent.FileManager.Files.Length == 1) {
				logger.Warn("Launching file: " + selectedTorrent.SavePath + System.IO.Path.DirectorySeparatorChar + selectedTorrent.FileManager.Files[0].Path); 
				Process.Start(selectedTorrent.SavePath + System.IO.Path.DirectorySeparatorChar + selectedTorrent.FileManager.Files[0].Path);
			} else {
				logger.Info("Opening folder: " + selectedTorrent.SavePath + System.IO.Path.DirectorySeparatorChar + selectedTorrent.FileManager.BaseDirectory);
				Process.Start("\"file://" + selectedTorrent.SavePath + System.IO.Path.DirectorySeparatorChar + selectedTorrent.FileManager.BaseDirectory + "\"");
			}
		}
	}
}
