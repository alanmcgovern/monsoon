//
// FileTreeView.cs
//
// Author:
//   Jared Hendry (buchan@gmail.com)
//   Mirco Bauer (meebey@meebey.net)
//
// Copyright (C) 2007 Jared Hendry
// Copyright (C) 2008 Mirco Bauer
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
using System.IO;
using Gtk;
using MonoTorrent.Common;
using MonoTorrent.Client;

namespace Monsoon
{
	public class FileTreeView : TreeView
	{
		private TorrentFileModel treeStore;
		private TreeViewColumn priorityColumn;
		private TreeViewColumn filenameColumn;
		private TreeViewColumn progressColumn;
		//private TreeStore treeStore;
		private TorrentController torrentController;
		
		private Gtk.Menu contextMenu;
		private ImageMenuItem highItem;
		private ImageMenuItem highestItem;
		private ImageMenuItem immediateItem;
		private ImageMenuItem lowItem;
		private ImageMenuItem lowestItem;
		private ImageMenuItem normalItem;
		private ImageMenuItem nodownItem;
		
		
		public FileTreeView(TorrentController torrentController, TorrentFileModel treeStore) : base()
		{
			this.treeStore = treeStore;
			this.Model = treeStore;
			this.torrentController = torrentController;
			this.Selection.Mode = SelectionMode.Multiple;
			HeadersVisible = true;

			BuildColumns ();
			BuildContextMenu ();
		}
		
		private void BuildColumns ()
		{
			priorityColumn = new TreeViewColumn ();
			filenameColumn = new TreeViewColumn ();
			progressColumn = new TreeViewColumn ();
			
			filenameColumn.Resizable = true;
			
			priorityColumn.Title = "";
			filenameColumn.Title = _("Filename");
			progressColumn.Title = _("Progress");
			
			Gtk.CellRendererPixbuf priorityCell = new CellRendererPixbuf ();
			Gtk.CellRendererText filenameCell = new CellRendererText ();
			Gtk.CellRendererProgress progressCell = new CellRendererProgress ();
			
			priorityColumn.PackStart (priorityCell, true);
			priorityColumn.SetAttributes (priorityCell, "pixbuf", 2);
			filenameColumn.PackStart (filenameCell, true);
			filenameColumn.SetAttributes (filenameCell, "text", 3);
			progressColumn.PackStart(progressCell, true);
			progressColumn.SetCellDataFunc (progressCell, new Gtk.TreeCellDataFunc (RenderProgress));
			
			AppendColumn (priorityColumn);
			AppendColumn (filenameColumn);
			AppendColumn (progressColumn);
		}
		
		private void BuildContextMenu ()
		{
			contextMenu = new Menu ();
			
			highItem = new ImageMenuItem ("High");
			highestItem = new ImageMenuItem ("Highest");
			immediateItem = new ImageMenuItem ("Immediate");
			lowItem = new ImageMenuItem ("Low");
			lowestItem = new ImageMenuItem ("Lowest");
			normalItem = new ImageMenuItem ("Normal");
			nodownItem = new ImageMenuItem ("Do Not Download");
			highItem.Image = new Gtk.Image (treeStore.GetPixbuf (Priority.High));
			highestItem.Image = new Image (treeStore.GetPixbuf (Priority.Highest));
			immediateItem.Image = new Image (treeStore.GetPixbuf (Priority.Immediate));
			lowItem.Image = new Image (treeStore.GetPixbuf (Priority.Low));
			lowestItem.Image = new Image (treeStore.GetPixbuf (Priority.Lowest));
			nodownItem.Image = new Image (treeStore.GetPixbuf (Priority.DoNotDownload));
			
			
			highItem.Activated += OnContextMenuItemClicked;
			highestItem.Activated += OnContextMenuItemClicked;
			immediateItem.Activated += OnContextMenuItemClicked;
			lowItem.Activated += OnContextMenuItemClicked;
			lowestItem.Activated += OnContextMenuItemClicked;
			normalItem.Activated += OnContextMenuItemClicked;
			nodownItem.Activated += OnContextMenuItemClicked;
			
			contextMenu.Append (immediateItem);
			contextMenu.Append (highestItem);
			contextMenu.Append (highItem);
			contextMenu.Append (normalItem);
			contextMenu.Append (lowItem);
			contextMenu.Append (lowestItem);
			contextMenu.Append (nodownItem);
			
		}
		
		private void OnContextMenuItemClicked (object sender, EventArgs args)
		{
			TorrentFile file;
			Priority priority;
			Selection.SelectedForeach(delegate (TreeModel model, TreePath path, TreeIter iter) {
				ImageMenuItem item = (ImageMenuItem) sender;
				
				// determine priority
				if (item == highItem)
					priority = Priority.High;
				else if (item == highestItem)
					priority = Priority.Highest;
				else if (item == immediateItem)
					priority = Priority.Immediate;
				else if (item == lowItem)
					priority = Priority.Low;
				else if (item == lowestItem)
					priority = Priority.Lowest;
				else if (item == normalItem)
					priority = Priority.Normal;
				else if (item == nodownItem)
					priority = Priority.DoNotDownload;
				else
					priority = Priority.Normal;
				
				file = (TorrentFile) Model.GetValue (iter, 1);
				file.Priority = priority;
				((TorrentFileModel)Model).UpdateRow (iter);
			});
		}
		
		protected override bool	OnButtonPressEvent (Gdk.EventButton e)
		{
			int count = Selection.CountSelectedRows ();
			if (count > 1 && e.Button == 3)
			{
				contextMenu.ShowAll();
				contextMenu.Popup();
				return true;
			}
			else if (count <= 1 && e.Button == 3)
			{
				base.OnButtonPressEvent(e);
				contextMenu.ShowAll();
				contextMenu.Popup();
				return true;
			}
			
			base.OnButtonPressEvent(e);
			return false;
		}
		
		private void RenderProgress (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			TorrentFile torrentFile = (TorrentFile) model.GetValue ( iter, 1);
			
			(cell as Gtk.CellRendererProgress).Value = (int)torrentFile.BitField.PercentComplete;
		}
		
		private static string _(string s)
		{
			return Mono.Unix.Catalog.GetString(s);
		}
	}
}
