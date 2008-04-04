//
// PeerTreeView.cs
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
using Gtk;
using MonoTorrent.Client;

namespace Monsoon
{
	public class PeerTreeView : TreeView
	{
		private TreeViewColumn addressColumn;
		private TreeViewColumn clientColumn;
		private TreeViewColumn downColumn;
		private TreeViewColumn upColumn;
		private TreeViewColumn seedingColumn;
		private TreeViewColumn interestedColumn;
		
		public PeerTreeView() : base()
		{
			buildColumns();
				
			Reorderable = true;
			HeadersVisible = true;
			HeadersClickable = true;
			Selection.Mode = SelectionMode.Multiple;
			
		}
			
		private void buildColumns()
		{
			addressColumn = new TreeViewColumn();
			clientColumn = new TreeViewColumn();
			downColumn = new TreeViewColumn();
			upColumn = new TreeViewColumn();
			seedingColumn = new TreeViewColumn();
			interestedColumn = new TreeViewColumn();
			
			addressColumn.Title = "IP Address";
			clientColumn.Title = "Client";
			downColumn.Title = "DL Speed";
			upColumn.Title = "UP Speed";
			seedingColumn.Title = "Seeding";
			interestedColumn.Title = "Interested";
			
			addressColumn.Expand = true;
			clientColumn.Expand = true;
			downColumn.Expand = true;
			upColumn.Expand = true;
			seedingColumn.Expand = true;
			interestedColumn.Expand = true;
			
			Gtk.CellRendererText addressCell = new Gtk.CellRendererText ();
			Gtk.CellRendererText clientCell = new Gtk.CellRendererText ();
			Gtk.CellRendererText downCell = new Gtk.CellRendererText ();
			Gtk.CellRendererText upCell = new Gtk.CellRendererText ();
			Gtk.CellRendererToggle seedingCell = new Gtk.CellRendererToggle ();
			Gtk.CellRendererToggle interestedCell = new Gtk.CellRendererToggle ();
			
			addressColumn.PackStart(addressCell, true);
			clientColumn.PackStart(clientCell, true);
			downColumn.PackStart(downCell, true);
			upColumn.PackStart(upCell, true);
			seedingColumn.PackStart(seedingCell, true);
			interestedColumn.PackStart(interestedCell, true);
			
			addressColumn.SetCellDataFunc (addressCell, new Gtk.TreeCellDataFunc (RenderAddress));
			clientColumn.SetCellDataFunc (clientCell, new Gtk.TreeCellDataFunc (RenderClient));
			downColumn.SetCellDataFunc (downCell, new Gtk.TreeCellDataFunc (RenderDown));
			upColumn.SetCellDataFunc (upCell, new Gtk.TreeCellDataFunc (RenderUp));
			seedingColumn.SetCellDataFunc (seedingCell, new Gtk.TreeCellDataFunc (RenderSeeding));
			interestedColumn.SetCellDataFunc (interestedCell, new Gtk.TreeCellDataFunc (RenderInterested));
			
			AppendColumn(addressColumn);
			AppendColumn(clientColumn);
			AppendColumn(downColumn);
			AppendColumn(upColumn);
			AppendColumn(seedingColumn);
			AppendColumn(interestedColumn);
			
		}
		
		private void RenderAddress (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			PeerId peer = (PeerId) model.GetValue (iter, 0);
			
			if(peer.IsValid)
				(cell as Gtk.CellRendererText).Text = peer.Location.ToString();
		}
		
		
		private void RenderClient (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			PeerId peer = (PeerId) model.GetValue (iter, 0);
			if(peer.IsValid)
			(cell as Gtk.CellRendererText).Text = peer.ClientSoftware.Client.ToString();
		}
		
		private void RenderDown (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			PeerId peer = (PeerId) model.GetValue (iter, 0);
			if(peer.IsValid)
			(cell as Gtk.CellRendererText).Text = ByteConverter.ConvertSpeed (peer.Monitor.DownloadSpeed);
		}
		
		private void RenderUp (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			PeerId peer = (PeerId) model.GetValue (iter, 0);
			if(peer.IsValid)
			(cell as Gtk.CellRendererText).Text = ByteConverter.ConvertSpeed (peer.Monitor.UploadSpeed);
		}
		
		private void RenderSeeding (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			PeerId peer = (PeerId) model.GetValue (iter, 0);
			if(peer.IsValid)
			(cell as Gtk.CellRendererToggle).Active = peer.IsSeeder;
		}
		
		private void RenderInterested (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			PeerId peer = (PeerId) model.GetValue (iter, 0);
			if(peer.IsValid)
			(cell as Gtk.CellRendererToggle).Active = peer.AmInterested;
		}
		
		
	}
}