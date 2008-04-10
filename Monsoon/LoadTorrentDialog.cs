// LoadTorrentDialog.cs created with MonoDevelop
// User: alan at 22:02 09/04/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;
using MonoTorrent.Common;

namespace Monsoon
{
	public partial class LoadTorrentDialog : Gtk.Dialog
	{
		private Gtk.TreeStore store;
		public LoadTorrentDialog()
		{
			this.Build();
			store = new Gtk.TreeStore (typeof (string), typeof (ToggleButton));

			TreeIter music = store.AppendValues ("Music");
			TreeIter radiohead = store.AppendValues (music, "Radiohead");
			store.AppendValues (radiohead, "Kid A");
			
			TreeIter hail = store.AppendValues (radiohead, "Hail To The Thief");
			store.AppendValues (hail, "Track 1");
			store.AppendValues (hail, "Track 2");
			store.AppendValues (hail, "Track 3");
			
			TreeIter rem = store.AppendValues (music, "R.E.M.");
			store.AppendValues (rem, "The Best Of");
			store.AppendValues (rem, "Green");
			
			store.AppendValues ("Stuff.txt");
			store.AppendValues ("What do ya think?");
			
			torrentTreeView.Model = store;
			
			Gtk.TreeViewColumn secondColumn = new Gtk.TreeViewColumn();
			secondColumn.Title = "Checkbox";
			Gtk.CellRendererToggle secondCell = new Gtk.CellRendererToggle ();
			secondColumn.PackStart (secondCell, true);
			secondColumn.AddAttribute (secondCell, "toggle", 0);
			torrentTreeView.AppendColumn (secondColumn);
			
			Gtk.TreeViewColumn firstColumn = new Gtk.TreeViewColumn();
			firstColumn.Title = "First";
			Gtk.CellRendererText firstCell = new Gtk.CellRendererText ();
			firstColumn.PackStart (firstCell, true);
			firstColumn.AddAttribute (firstCell, "text", 0);
			torrentTreeView.AppendColumn (firstColumn);
			
			torrentTreeView.EnableTreeLines = true;
			torrentTreeView.ExpanderColumn.Alignment = 0;
		}
	}
}
