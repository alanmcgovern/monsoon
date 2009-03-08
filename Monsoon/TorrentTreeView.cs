//
// TorrentTreeView.cs
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
using System.Text;
using MonoTorrent.Common;
using MonoTorrent.Client;

namespace Monsoon
{
	public class TorrentTreeView : TreeView
	{
		public class Column : TreeViewColumn
		{
			public string Name {
				get; set;
			}
			
			public bool Ignore {
				get; set;
			}
		}

		public readonly Column nameColumn = new Column { Name = "name", Title = _("Name"), SortColumnId = 1 };
		public readonly Column statusColumn = new Column { Name = "status", Title = _("Status"), SortColumnId = 2 };
		public readonly Column doneColumn = new Column { Name = "done", Title = _("Done"), SortColumnId = 3 };
		public readonly Column seedsColumn = new Column { Name = "seeds", Title = _("Seeds"), SortColumnId = 4 };
		public readonly Column peersColumn = new Column { Name = "peers", Title = _("Peers"), SortColumnId = 5 };
		public readonly Column priorityColumn = new Column { Name = "priority", Title = _("Priority"), SortColumnId = 12 };
		public readonly Column downSpeedColumn = new Column { Name = "downspeed", Title = _("DL Speed"), SortColumnId = 6 };
		public readonly Column upSpeedColumn = new Column { Name = "upspeed", Title = _("UP speed"), SortColumnId = 7 };
		public readonly Column ratioColumn = new Column { Name = "ratio", Title = _("Ratio"), SortColumnId = 8 };
		public readonly Column sizeColumn = new Column { Name = "size", Title = _("Size"), SortColumnId = 9 };
		public readonly Column etaColumn = new Column { Name = "eta", Title = _("ETA"), SortColumnId = 10 };

		private Predicate<Download> filter;
		private TorrentController torrentController;
		private TorrentContextMenu menu;
		
		private TargetEntry[] targetEntries;
		private TargetEntry[] sourceEntries;

		public ListStore Torrents {
			get; set;
		}

		public Predicate <Download> Filter {
			get { return filter; }
			set {
				filter = value;
				UpdateAll ();
			}
		}

		public TreeModelFilter FilterModel {
			get; set;
		}

		public TorrentTreeView() : base()
		{
			Torrents = new ListStore (typeof (Download), typeof (string), typeof (string),
			                       typeof (int), typeof (string), typeof (string),
			                       typeof (string), typeof (string), typeof (string),
			                       typeof (string), typeof (string), typeof (string), typeof (string));
			
			FilterModel = new Gtk.TreeModelFilter (Torrents, null);
			FilterModel.VisibleFunc = delegate (TreeModel model, TreeIter iter) {
				return Filter == null ? true : Filter ((Download) model.GetValue (iter, 0));
			};
			Model = FilterModel;
			this.torrentController = ServiceManager.Get <TorrentController> ();
			
			targetEntries = new TargetEntry[]{
				new TargetEntry("text/uri-list", 0, 0) 
			};
			
			sourceEntries = new TargetEntry[]{
				new TargetEntry("application/x-monotorrent-Download-objects", 0, 0)
			};
			
			buildColumns();
				
			Reorderable = true;
			HeadersVisible = true;
			HeadersClickable = true;
			Selection.Mode = SelectionMode.Multiple;
			Selection.Changed += Event.Wrap (delegate (object o, EventArgs e) {
				TreeIter iter;
				TreePath [] selectedTorrents = Selection.GetSelectedRows ();
				
				List <Download> downloads = new List<Download> ();
				foreach (TreePath path in Selection.GetSelectedRows ()) {
					if (Torrents.GetIter (out iter, path)) {
						downloads.Add ((Download) Torrents.GetValue (iter, 0));
					}
				}
				
				torrentController.Select (downloads);
			});

			EnableModelDragDest(targetEntries, Gdk.DragAction.Copy);
			//this.DragDrop += OnTest;
			
			
			this.EnableModelDragSource(Gdk.ModifierType.Button1Mask, sourceEntries, Gdk.DragAction.Copy);
			DragDataGet += OnTorrentDragDataGet;

			
			menu = new TorrentContextMenu ();
			torrentController.Added += delegate(object sender, DownloadAddedEventArgs e) {
				AddDownload (e.Download);
			};
			
			torrentController.Removed += delegate(object sender, DownloadAddedEventArgs e) {
				RemoveDownload (e.Download);
			};

			LabelController lc = ServiceManager.Get <LabelController> ();
			lc.SelectionChanged += delegate {
				TorrentLabel label = lc.Selection;
				Filter = delegate (Download download) {
					return label.Torrents.Contains (download);
				};
			};
			
			// FIXME: This shouldn't be necessary
			torrentController.Torrents.ForEach (AddDownload);
		}
		
		void AddDownload (Download download)
		{
			download.StateChanged += HandleStateChanged;
			download.PriorityChanged += HandlePriorityChanged;
			Update (Torrents.AppendValues (download));
		}

		void HandlePriorityChanged(object sender, EventArgs e)
		{
			UpdateAll ();
		}

		void HandleStateChanged(object sender, StateChangedEventArgs e)
		{
			UpdateAll ();
		}
		
		void RemoveDownload (Download download)
		{
			TreeIter iter;
			if (Torrents.GetIterFirst (out iter)) {
				do {
					if (download != Torrents.GetValue (iter, 0))
						continue;

					download.PriorityChanged -= HandlePriorityChanged;
					download.StateChanged -= HandleStateChanged;
					Torrents.Remove (ref iter);
					Selection.UnselectAll ();
					break;
				} while (Torrents.IterNext (ref iter));
			}
		}

		protected override bool	OnButtonPressEvent (Gdk.EventButton e)
		{
			// Call this first so context menu has a selected torrent
			base.OnButtonPressEvent(e);
			
			if(e.Button == 3 && Selection.CountSelectedRows() == 1){
				menu.ShowAll ();
				menu.Popup ();
				return true;
			}
			
			return false;
		}
		
		private void OnTorrentDragDataGet (object o, DragDataGetArgs args)
		{
			// TODO: Support dragging multiple torrents to a label
			Download manager;
			
			manager = torrentController.SelectedDownload;
			if(manager == null)
				return;
			
			args.SelectionData.Set(Gdk.Atom.Intern("application/x-monotorrent-Download-objects", false), 8, manager.Torrent.InfoHash);
		}
			
		private void buildColumns()
		{
			Column downloadColumn = new Column { Ignore = true, Visible = false, Title = "N/A" };
			
			Gtk.CellRendererText torrentNameCell = new Gtk.CellRendererText ();
			Gtk.CellRendererText torrentStatusCell = new Gtk.CellRendererText();
			Gtk.CellRendererProgress torrentDoneCell = new Gtk.CellRendererProgress();
			Gtk.CellRendererText torrentSeedsCell = new Gtk.CellRendererText();
			Gtk.CellRendererText torrentPeersCell = new Gtk.CellRendererText();
			Gtk.CellRendererText torrentPriorityCell = new Gtk.CellRendererText();
			Gtk.CellRendererText torrentDownSpeedCell = new Gtk.CellRendererText();
			Gtk.CellRendererText torrentUpSpeedCell = new Gtk.CellRendererText();
			Gtk.CellRendererText torrentRatioCell = new Gtk.CellRendererText();
			Gtk.CellRendererText torrentSizeCell = new Gtk.CellRendererText();
			Gtk.CellRendererText torrentEtaCell = new Gtk.CellRendererText();

			nameColumn.PackStart(torrentNameCell, true);
			statusColumn.PackStart(torrentStatusCell, true);
			doneColumn.PackStart(torrentDoneCell, true);
			seedsColumn.PackStart(torrentSeedsCell, true);
			peersColumn.PackStart(torrentPeersCell, true);
			priorityColumn.PackStart(torrentPriorityCell, true);
			downSpeedColumn.PackStart(torrentDownSpeedCell, true);
			upSpeedColumn.PackStart(torrentUpSpeedCell, true);
			ratioColumn.PackStart(torrentRatioCell, true);
			sizeColumn.PackStart(torrentSizeCell, true);
			etaColumn.PackStart(torrentEtaCell, true);

			nameColumn.AddAttribute (torrentNameCell, "text", 1);
			statusColumn.AddAttribute (torrentStatusCell, "text", 2);
			statusColumn.AddAttribute (torrentStatusCell, "foreground", 11);
			doneColumn.AddAttribute (torrentDoneCell, "value", 3);
			seedsColumn.AddAttribute (torrentSeedsCell, "text", 4);
			peersColumn.AddAttribute (torrentPeersCell, "text", 5);
			priorityColumn.AddAttribute (torrentPriorityCell, "text", 12);
			downSpeedColumn.AddAttribute (torrentDownSpeedCell, "text", 6);
			upSpeedColumn.AddAttribute (torrentUpSpeedCell, "text", 7);
			ratioColumn.AddAttribute (torrentRatioCell, "text", 8);
			sizeColumn.AddAttribute (torrentSizeCell, "text", 9);
			etaColumn.AddAttribute (torrentEtaCell, "text", 10);

			AppendColumn(priorityColumn);
			AppendColumn(downloadColumn);
			AppendColumn(nameColumn);
			AppendColumn(statusColumn);
			AppendColumn(doneColumn);
			AppendColumn(seedsColumn);
			AppendColumn(peersColumn);
			AppendColumn(downSpeedColumn);
			AppendColumn(upSpeedColumn);
			AppendColumn(etaColumn);
			AppendColumn(ratioColumn);
			AppendColumn(sizeColumn);
			
			foreach (TreeViewColumn c in this.Columns) {
				c.Sizing = TreeViewColumnSizing.Fixed;
				c.Reorderable = true;
				c.Resizable = true;
				
				c.Clicked += delegate (object o, EventArgs e) {
					int oldId;
					SortType oldSort;
					TreeViewColumn sender = (TreeViewColumn) o;
					
					Torrents.GetSortColumnId (out oldId, out oldSort);

					// Invert the sort order if we're the same
					if (oldId == sender.SortColumnId)
						sender.SortOrder = sender.SortOrder == SortType.Ascending ? SortType.Descending : SortType.Ascending;
					else
						sender.SortOrder = SortType.Ascending;
					Torrents.SetSortColumnId (sender.SortColumnId, sender.SortOrder);
				};
			}
		}
		
		void UpdateAll ()
		{
			TreeIter iter;
			if (Torrents.GetIterFirst (out iter)) {
				do {
					Update (iter);
				} while (Torrents.IterNext (ref iter));
			}
		}

		void Update (TreeIter row)
		{
			Download d = (Download) Torrents.GetValue (row, 0);
			Console.WriteLine ("Updating: {0}", d.Torrent.Name);
			Torrents.SetValues (row,
			                 d,
			                 d.Torrent.Name,
			                 GetStatusString (d),
			                 (int) (d.Progress * 100.0),
			                 d.Seeds.ToString (),
			                 d.Leechs + " (" + d.Available + ")",
			                 ByteConverter.ConvertSpeed (d.DownloadSpeed),
			                 ByteConverter.ConvertSize (d.UploadSpeed),
			                 ((float)d.TotalUploaded / d.TotalDownloaded).ToString (),
			                 ByteConverter.ConvertSize (d.Torrent.Size),
			                 GetEtaString (d),
			                 GetStatusColour (d),
			                 d.Priority.ToString ()
			                 );

			Console.WriteLine ("Updated: {0}", d.Torrent.Name);
		}

		private string GetStatusColour (Download torrent)
		{
			if (torrent.State == Monsoon.State.Downloading)
				return "darkgreen";
			if (torrent.State == Monsoon.State.Paused)
				return "orange";
			if (torrent.State == Monsoon.State.Hashing)
				return "purple";
			if (torrent.State == Monsoon.State.Seeding)
				return "darkgreen";
			if (torrent.State == Monsoon.State.Stopped && torrent.Complete)
				return "blue";
			if (torrent.State == Monsoon.State.Queued)
				return "black";
			
			return "red";
		}
		
		private string GetEtaString (Download manager)
		{
			TimeSpan eta;
			if (manager.State == Monsoon.State.Downloading && (manager.Torrent.Size - manager.TotalDownloaded) > 0)
			{
				double dSpeed = manager.DownloadSpeed;
				eta = TimeSpan.FromSeconds(dSpeed > 0 ? ((manager.Torrent.Size - manager.TotalDownloaded) / dSpeed) : -1);
			}
			else if (manager.State == Monsoon.State.Seeding && (manager.Torrent.Size - manager.TotalUploaded) > 0)
			{
				double uSpeed = manager.UploadSpeed;
				eta = TimeSpan.FromSeconds(uSpeed > 0 ? ((manager.Torrent.Size - manager.TotalUploaded) / uSpeed) : -1);
			}
			else
				return string.Empty;
			
			if (eta.Seconds <= 0)
				return "∞";
			if (eta.Days > 0)
				return string.Format("{0}d {1}h", eta.Days, eta.Hours);
			if (eta.Hours > 0)
				return string.Format("{0}h {1}m", eta.Hours, eta.Minutes);					
			if (eta.Minutes > 0)
				return string.Format("{0}m {1}s", eta.Minutes, eta.Seconds);
			
			return string.Format("{0}s", eta.Seconds);
		}

		private string GetStatusString (Download manager)
		{
			if (manager == null)
				return "";
			
			if(manager.State == Monsoon.State.Queued)
				return _("Queued");
			
			switch (manager.State)
			{
			case Monsoon.State.Stopped:
				return manager.Complete ? "Finished" : "Stopped";
			case Monsoon.State.Seeding:
				return "Seeding";
			case Monsoon.State.Downloading:
				return "Downloading";
			case Monsoon.State.Hashing:
				return "Hashing";
			case Monsoon.State.Paused:
				return "Paused";
			default:
				return manager.State.ToString ();
			}
		}
		
		private static string _(string s)
		{
			return Mono.Unix.Catalog.GetString(s);
		}
	}
}
