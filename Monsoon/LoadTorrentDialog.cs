// LoadTorrentDialog.cs created with MonoDevelop
// User: alan at 22:02 09/04/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;
using MonoTorrent.Common;
using System.Collections.Generic;

namespace Monsoon
{
	public partial class LoadTorrentDialog : Gtk.Dialog
	{
		private Gtk.TreeStore store;
		private Torrent torrent;
		
		public LoadTorrentDialog(Torrent torrent)
		{
			this.Build();
			PopulateStore (torrent);
			BuildColumns();
			
			this.lblName.Text = torrent.Name;
			this.lblSize.Text = ByteConverter.ConvertSize (torrent.Size);
		}
		
		private void BuildColumns ()
		{
			Gtk.CellRendererToggle toggler = new Gtk.CellRendererToggle ();
			torrentTreeView.AppendColumn ("Checkbox", toggler, "active", 1);
			torrentTreeView.AppendColumn ("Filename", new CellRendererText(), "text", 0);
			
			toggler.Toggled += OnToggled;
		}
		
		private void OnToggled (object o, Gtk.ToggledArgs e)
		{
			TreeIter iter;
			store.GetIter (out iter, new TreePath(e.Path));
			bool value = !(bool)store.GetValue(iter, 1);
			
			store.SetValue (iter, 1, value);
			TorrentFile file = (TorrentFile)store.GetValue (iter, 2);
			if (file != null)
				file.Priority = value ? Priority.Normal : Priority.DoNotDownload;

			if (store.IterHasChild (iter))
			{
				store.IterChildren (out iter, iter);
				RecurseToggle (iter, value);
			}
		}
		
		private void PopulateStore (Torrent torrent)
		{
			store = new Gtk.TreeStore (typeof (string), typeof (bool), typeof (TorrentFile));
			
			TreeIter iter = store.AppendValues ("Files", true);
			foreach (TorrentFile file in torrent.Files)
			{
				string[] parts = file.Path.Split (System.IO.Path.DirectorySeparatorChar);
				RecursiveAdd (iter, new List<string>(parts), file);
			}

			torrentTreeView.Model = store;
		}
		
		private void RecursiveAdd (TreeIter parent, List<string> parts, TorrentFile file)
		{
			if (parts.Count == 0)
			{
				store.SetValue (parent, 2, file);
				return;
			}
			TreeIter siblings;
			if (!store.IterChildren (out siblings, parent))
			{
				siblings = store.AppendValues (parent, parts[0], true);
				parts.RemoveAt(0);
				RecursiveAdd (siblings, parts, file);
				return;
			}

			do
			{
				if (store.GetValue (siblings, 0).Equals (parts[0]))
				{
					parts.RemoveAt(0);
					RecursiveAdd (siblings, parts, file);
					return;
				}
			} while (store.IterNext (ref siblings));
			
			siblings = store.AppendValues (parent, parts[0], true);
			parts.RemoveAt(0);
			RecursiveAdd (siblings, parts, file);
		}
		
		private void RecurseToggle (TreeIter iter, bool value)
		{
			do
			{
				if (store.IterHasChild (iter))
				{
					TreeIter child;
					store.IterChildren (out child, iter);
					RecurseToggle (child, value);
				}
				
				store.SetValue (iter, 1, value);
				TorrentFile file = (TorrentFile)store.GetValue (iter, 2);
				if (file != null)
					file.Priority = value ? Priority.Normal : Priority.DoNotDownload;
				
			} while (store.IterNext (ref iter));
		}
	}
}
