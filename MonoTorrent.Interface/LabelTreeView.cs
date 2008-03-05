//
// LabelTreeView.cs
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

using Gtk;
using System;
using System.Collections;


namespace Monsoon
{
	public class LabelTreeView : TreeView
	{
		public TreeViewColumn iconColumn;
		public TreeViewColumn nameColumn;
		public TreeViewColumn sizeColumn;
		
		private Gtk.Menu contextMenu;
		private ImageMenuItem createItem;
		private ImageMenuItem removeItem;
		
		private MainWindow mainWindow;
		private bool contextActive;
		
		public LabelTreeView(MainWindow mainWindow, bool contextActive)
		{
			this.mainWindow = mainWindow;
			this.contextActive = contextActive;
			buildColumns();
									
			Reorderable = false;
			HeadersVisible = false;
			HeadersClickable = false;
			
			BuildContextMenu();
		}
					
		private void buildColumns()
		{
			iconColumn = new TreeViewColumn();
			nameColumn = new TreeViewColumn();
			sizeColumn = new TreeViewColumn();
			
			Gtk.CellRendererPixbuf iconRendererCell = new Gtk.CellRendererPixbuf ();
			Gtk.CellRendererText nameRendererCell = new Gtk.CellRendererText();
			Gtk.CellRendererText sizeRendererCell = new Gtk.CellRendererText();
			
			//nameRendererCell.Editable = true;
			nameRendererCell.EditingStarted += delegate (object o, Gtk.EditingStartedArgs args) {
				Console.WriteLine("Starting Editing");
				Gtk.TreeIter iter;
				mainWindow.labelListStore.GetIter (out iter, new Gtk.TreePath(args.Path));
				
				TorrentLabel l = (TorrentLabel) mainWindow.labelListStore.GetValue(iter, 0);
				if (l.Immutable)
				{
					nameRendererCell.CancelEditing();
					nameRendererCell.StopEditing(true);
					
					args.Editable.FinishEditing();
					args.Editable.RemoveWidget();
					
					Console.WriteLine("Can't touch this: {0}", l.Name);
				}
			};
			
			nameRendererCell.EditingCanceled += delegate { Console.WriteLine("Cancelled"); };
			
			nameRendererCell.Edited += delegate (object o, Gtk.EditedArgs args) {
				Console.WriteLine("Edit done");
				Gtk.TreeIter iter;
				mainWindow.labelListStore.GetIter (out iter, new Gtk.TreePath (args.Path));
			 
				TorrentLabel label = (TorrentLabel) mainWindow.labelListStore.GetValue (iter, 0);
				label.Name = args.NewText;
			};
			
			iconColumn.PackStart(iconRendererCell, true);
			nameColumn.PackStart(nameRendererCell, true);
			sizeColumn.PackStart(sizeRendererCell, true);
			
			iconColumn.SetCellDataFunc (iconRendererCell, new Gtk.TreeCellDataFunc (RenderIcon));
			nameColumn.SetCellDataFunc (nameRendererCell, new Gtk.TreeCellDataFunc (RenderName));
			sizeColumn.SetCellDataFunc (sizeRendererCell, new Gtk.TreeCellDataFunc (RenderSize));
			
			AppendColumn (iconColumn);  
			AppendColumn (nameColumn);
			AppendColumn (sizeColumn);
		}
		
		
		
		private void BuildContextMenu ()
		{
			contextMenu = new Menu ();
			
			createItem = new ImageMenuItem ("Create");
			createItem.Image = new Image (Stock.Add, IconSize.Menu);
			createItem.Activated += delegate (object o, EventArgs e) {
				TorrentLabel l = new TorrentLabel(new ArrayList(), "New Label");
				mainWindow.labelListStore.AppendValues(l);
				mainWindow.labels.Add(l);
			};
			contextMenu.Append(createItem);
			
			removeItem = new ImageMenuItem ("Remove");
			removeItem.Image = new Image (Stock.Remove, IconSize.Menu);
			contextMenu.Add (removeItem);
			removeItem.Activated += delegate (object o, EventArgs e) {
				
				TreeIter iter;
				if (!Selection.GetSelected(out iter))
					return;
				
				TorrentLabel label = (TorrentLabel) mainWindow.labelListStore.GetValue(iter, 0);
				if (label.Immutable)
					return;
				
				mainWindow.labelListStore.Remove(ref iter);
				mainWindow.labels.Remove(label);
			};
		}
		
		protected override bool	OnButtonPressEvent (Gdk.EventButton e)
		{
			// Call this first so context menu has a selected torrent
			base.OnButtonPressEvent(e);
			
			if(!contextActive)
				return false;
			
			if(e.Button == 3)
			{
				int selected = Selection.CountSelectedRows();
				removeItem.Sensitive = selected == 1;
				contextMenu.ShowAll();
				contextMenu.Popup();
			}
			
			return false;
		}
		
		private void RenderIcon (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			TorrentLabel label = (TorrentLabel) model.GetValue (iter, 0);
			(cell as Gtk.CellRendererPixbuf).Pixbuf = label.Icon;
		}
		
		private void RenderName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			TorrentLabel label = (TorrentLabel) model.GetValue (iter, 0);
			(cell as Gtk.CellRendererText).Text = label.Name;
		}
		
		private void RenderSize (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			TorrentLabel label = (TorrentLabel) model.GetValue (iter, 0);
			(cell as Gtk.CellRendererText).Text = "(" + label.Size + ")";
		}
		
	}
}
