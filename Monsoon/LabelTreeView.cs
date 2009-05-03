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
using System.Collections.Generic;
using MonoTorrent.Client;

namespace Monsoon
{
	public class LabelTreeView : TreeView
	{
		public TreeViewColumn iconColumn;
		public TreeViewColumn nameColumn;
		public TreeViewColumn sizeColumn;

		private bool contextActive;
		private Gtk.Menu contextMenu;
		private ImageMenuItem createItem;
		ImageMenuItem removeItem;
		
		LabelController Controller {
			get; set;
		}
		
		public new ListStore Model {
			get { return (ListStore) base.Model; }
			set { base.Model = value; }
		}
		
		public LabelTreeView(LabelController labels, bool contextActive)
		{
			Controller = labels;
			this.contextActive = contextActive;
			
			Reorderable = false;
			HeadersVisible = false;
			HeadersClickable = false;
			
			buildColumns();
			BuildContextMenu();

            Controller.Added += LabelAdded;
            Controller.Removed += LabelRemoved;

			Selection.Changed += Event.Wrap (delegate(object sender, EventArgs e) {
				TreeIter iter;
				if (Selection.GetSelected (out iter))
					Controller.Selection = ((TorrentLabel) Model.GetValue (iter, 0));
			});
			
			Controller.Labels.ForEach (Add);
			Remove (Controller.Delete);
		}

        void LabelRemoved(object sender, LabelEventArgs e)
        {
            Remove(e.Label);
        }

        void LabelAdded(object sender, LabelEventArgs e)
        {
            Add(e.Label);
        }

        public override void Destroy()
        {
            foreach (TorrentLabel label in Controller.Labels)
                Remove(label);
            Controller.Added -= LabelAdded;
            Controller.Removed -= LabelRemoved;
            base.Destroy();
		}
		
		void Add (TorrentLabel label)
		{
			UpdateSize (Model.AppendValues (label, label.Icon, label.Name, "", !label.Immutable));
			label.Added += LabelChanged;
			label.Removed += LabelChanged;
		}

		void LabelChanged (object sender, DownloadAddedEventArgs e)
		{
			TreeIter iter;
			if (Model.GetIterFirst (out iter)) {
				do {
					if (Model.GetValue (iter, 0) == sender) {
						UpdateSize (iter);
						return;
					}
				} while (Model.IterNext (ref iter));
			}
		}
		
		void Remove (TorrentLabel label)
		{
			TreeIter iter;
			if (Model.GetIterFirst (out iter)) {
				do {
					if (Model.GetValue (iter, 0) != label)
						continue;

					label.Added -= LabelChanged;
					label.Removed -= LabelChanged;
					Model.Remove (ref iter);
					return;
				} while (Model.IterNext (ref iter));
			}
		}
					
		private void buildColumns()
		{
			Model = new ListStore (typeof (TorrentLabel), typeof (Gdk.Pixbuf),
			                       typeof (string), typeof (string), typeof (bool));
			
			iconColumn = new TreeViewColumn();
			nameColumn = new TreeViewColumn();
			sizeColumn = new TreeViewColumn();
			
			Gtk.CellRendererPixbuf iconRendererCell = new Gtk.CellRendererPixbuf ();
			Gtk.CellRendererText nameRendererCell = new Gtk.CellRendererText { Editable = true };
			Gtk.CellRendererText sizeRendererCell = new Gtk.CellRendererText();

			iconColumn.PackStart(iconRendererCell, true);
			nameColumn.PackStart(nameRendererCell, true);
			sizeColumn.PackStart(sizeRendererCell, true);
			
			iconColumn.AddAttribute (iconRendererCell, "pixbuf", 1);
			nameColumn.AddAttribute (nameRendererCell, "text", 2);
			sizeColumn.AddAttribute (sizeRendererCell, "text", 3);
			nameColumn.AddAttribute (nameRendererCell, "editable", 4);
			
			AppendColumn (iconColumn);  
			AppendColumn (nameColumn);
			AppendColumn (sizeColumn);

			nameRendererCell.Edited += Event.Wrap ((EditedHandler) delegate (object o, Gtk.EditedArgs args) {
				Gtk.TreeIter iter;
				if (Model.GetIter (out iter, new Gtk.TreePath (args.Path))) {
					TorrentLabel label = (TorrentLabel) Model.GetValue (iter, 0);
					label.Name = args.NewText;
				}
			});
		}

		private void BuildContextMenu ()
		{
			contextMenu = new Menu ();
			
			createItem = new ImageMenuItem (_("Create"));
			createItem.Image = new Image (Stock.Add, IconSize.Menu);
			createItem.Activated += Event.Wrap ((EventHandler) delegate (object o, EventArgs e) {
				Controller.Add(new TorrentLabel(_("New Label")));
			});
			contextMenu.Append(createItem);
			
			removeItem = new ImageMenuItem (_("Remove"));
			removeItem.Image = new Image (Stock.Remove, IconSize.Menu);
			contextMenu.Add (removeItem);
			removeItem.Activated += Event.Wrap ((EventHandler) delegate (object o, EventArgs e) {
				
				TreeIter iter;
				if (!Selection.GetSelected(out iter))
					return;
				
				TorrentLabel label = (TorrentLabel) Model.GetValue(iter, 0);
				if (label.Immutable)
					return;
				
				Controller.Remove(label);
			});
		}
		
		protected override bool	OnButtonPressEvent (Gdk.EventButton e)
		{
			// Call this first so context menu has a selected torrent
			base.OnButtonPressEvent(e);
			
			if(!contextActive)
				return false;
			
			if(e.Button == 3)
			{
				TreeIter iter;
				if (Selection.GetSelected(out iter))
					removeItem.Sensitive = !((TorrentLabel) Model.GetValue(iter, 0)).Immutable;
				else
					removeItem.Sensitive = false;
				
				contextMenu.ShowAll();
				contextMenu.Popup();
			}
			
			return false;
		}

		void UpdateSize (TreeIter iter)
		{
			TorrentLabel label = (TorrentLabel) Model.GetValue (iter, 0);
			Model.SetValue (iter, 3, "(" + label.Size + ")");
		}
		
		private static string _(string s)
		{
			return Mono.Unix.Catalog.GetString(s);
		}
	}
}
