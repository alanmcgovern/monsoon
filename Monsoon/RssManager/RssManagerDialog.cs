//
// RssManagerDialog.cs
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
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;

using Gtk;


namespace Monsoon
{
	
	
	public partial class RssManagerDialog : Gtk.Dialog
	{
		private ListStore feedListStore;
		private ListStore filterListStore;
		private ListStore filterFeedListStore;
		private ListStore itemListStore;
		private ListStore historyListStore;
		private TreeIter allIter;
		private TreeModelFilter feedfilter;
		private FileChooserButton savePathChooserButton;
		
		private BackgroundWorker fetchFeedsWorker;
		
		private RssManagerController controller;
		
		private static NLog.Logger logger = MainClass.DebugEnabled ? NLog.LogManager.GetCurrentClassLogger () : new EmptyLogger ();
		
		
		public RssManagerDialog(RssManagerController controller)
		{
			this.Build();
			
			this.controller = controller;
			
			fetchFeedsWorker = new BackgroundWorker();
			fetchFeedsWorker.DoWork += FetchFeeds;
			fetchFeedsWorker.RunWorkerCompleted += FetchFeedsCompleted;
			
			BuildFeedPage();
			BuildFilterPage();
			BuildTorrentPage();
			BuildHistoryPage();
			
			UpdateFeeds();
		}
		
		
		private void UpdateFeeds()
		{
			if(!fetchFeedsWorker.IsBusy)
				fetchFeedsWorker.RunWorkerAsync();
		}
		
		
		private void FetchFeeds(object sender, DoWorkEventArgs args)
		{
			controller.Items.Clear();
			
			foreach(string feed in controller.Feeds){
				RssReader rssReader = new RssReader(feed);
				
				foreach(RssItem item in rssReader.Items){
					controller.Items.Add(item);
				}
			}
		}
		
		
		private void FetchFeedsCompleted(object sender, RunWorkerCompletedEventArgs args)
		{
			itemListStore.Clear();
			foreach(RssItem item in controller.Items){
				itemListStore.AppendValues(item);
			}
		}
		
		
		private void BuildFeedPage()
		{
			TreeViewColumn urlColumn = new TreeViewColumn("Feed URL", new Gtk.CellRendererText (), "text", 0);
			feedListStore = new ListStore(typeof(string));
			
			
			feedTreeView.AppendColumn(urlColumn);
			
			feedTreeView.Model = feedListStore;
			
			foreach(string feed in controller.Feeds){
				feedListStore.AppendValues(feed);
			}
		}
		
		
		private void BuildFilterPage()
		{
			TreeViewColumn filterColumn = new TreeViewColumn();
			filterListStore = new ListStore(typeof(RssFilter));
			
			CellRendererText textRenderer = new CellRendererText();
			filterFeedCombobox.PackStart(textRenderer, true);
			filterFeedCombobox.AddAttribute(textRenderer, "text", 0);
			
			filterFeedListStore = new ListStore(typeof(string));
			allIter = filterFeedListStore.AppendValues(_("All"));
		   	
			filterFeedCombobox.Model = filterFeedListStore;
			filterFeedCombobox.SetActiveIter(allIter);
			
			filterColumn.Title = _("Filter");
			Gtk.CellRendererText filterCell = new Gtk.CellRendererText ();
			filterColumn.PackStart (filterCell, true);
			filterColumn.SetCellDataFunc (filterCell, new Gtk.TreeCellDataFunc (RenderFilter));
			
			filterTreeView.AppendColumn(filterColumn);
			
			filterTreeView.Model = filterListStore;
			
			savePathChooserButton = new FileChooserButton(_("Select a Save Path"), FileChooserAction.SelectFolder);
			savePathChooserButton.SetCurrentFolder(controller.SavePath);
			savePathChooserButton.ShowAll();
			
			filterTable.Attach(savePathChooserButton, 1, 2, 3, 4, AttachOptions.Fill, AttachOptions.Shrink, 0, 0); 
			
			foreach(RssFilter filter in controller.Filters){
				filterListStore.AppendValues(filter);
			}
			foreach(string feed in controller.Feeds){
				filterFeedListStore.AppendValues(feed);
			}
			
			filterTreeView.Selection.Changed += OnFilterTreeViewSelectionChanged;
			
		}

		
		private void RenderFilter (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			RssFilter filter = (RssFilter) model.GetValue (iter, 0);
			(cell as Gtk.CellRendererText).Text = filter.Name;
		}

		
		private void BuildTorrentPage()
		{
			feedCombobox.Model = filterFeedListStore;
			feedCombobox.SetActiveIter(allIter);
			
			CellRendererText textRenderer = new CellRendererText();
			feedCombobox.PackStart(textRenderer, true);
			feedCombobox.AddAttribute(textRenderer, "text", 0);	
			
			itemListStore = new ListStore(typeof(RssItem));
			
			TreeViewColumn itemColumn = new TreeViewColumn();
			Gtk.CellRendererText itemCell = new Gtk.CellRendererText ();
			itemColumn.Title = _("Item");
			itemColumn.PackStart (itemCell, true);
			itemColumn.SetCellDataFunc (itemCell, new Gtk.TreeCellDataFunc (RenderItem));
			
			
			itemTreeView.AppendColumn(itemColumn);
			
			feedfilter = new TreeModelFilter (itemListStore, null);
			feedfilter.VisibleFunc = new TreeModelFilterVisibleFunc (FilterItemTree);
			itemTreeView.Model = feedfilter;
			
			feedCombobox.Changed += OnFeedComboboxChanged;
			
            		
            		//foreach(RssItem item in rssReader.Items){
            		//	itemListStore.AppendValues(item);
            		//}
            		itemTreeView.ButtonPressEvent += OnItemTreeViewButtonPressEvent;
            		itemTreeView.Selection.Changed += OnItemTreeViewSelectionChanged;
		}
		
		private bool FilterItemTree (TreeModel model, TreeIter iter)
		{
			RssItem item = (RssItem) model.GetValue (iter, 0);
			
			if(item == null)
				return false;
			
			if (item.Feed == feedCombobox.ActiveText || feedCombobox.ActiveText == "All")
				return true;
			else
				return false;
		}
		
		[GLib.ConnectBeforeAttribute]
		private void OnItemTreeViewButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			// Call this first so context menu has a selected torrent
			
			//base.OnButtonPressEvent(e);
			
			if(args.Event.Button == 3){
				Menu menu = new Menu();
				ImageMenuItem downloadItem = new ImageMenuItem (_("Download"));
				downloadItem.Image = new Image (Stock.GoDown, IconSize.Menu);
				downloadItem.Activated += OnDownloadMenuItemActivated;
				menu.Append(downloadItem);
				menu.ShowAll();
				menu.Popup();
			}

		}
		
		
		private void OnDownloadMenuItemActivated(object sender, EventArgs args)
		{
			TreeIter iter;
			RssItem item;
			
			if (!itemTreeView.Selection.GetSelected(out iter))
				return;
			
			item = (RssItem) itemTreeView.Model.GetValue(iter, 0);

			Console.Out.WriteLine("Downloading " + item.Link);
		
			controller.AddTorrent(new TorrentRssWatcherEventArgs (null, item));
		}
		
		
		private void RenderItem (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			RssItem item = (RssItem) model.GetValue (iter, 0);
			if(item != null)
				(cell as Gtk.CellRendererText).Text = item.Title;
		}
		
		
		private void OnItemTreeViewSelectionChanged(object sender, EventArgs args)
		{
		
			TreeIter iter;
			RssItem item;
			
			if (!itemTreeView.Selection.GetSelected(out iter))
				return;
			
			object o =  itemTreeView.Model.GetValue(iter, 0);
			
			if(o.GetType() != typeof(RssItem)){
				return;
			}
			
			item = (RssItem) o;
			
			Console.Out.WriteLine("Item URL: " + item.Link);
			
		}
		
		
		private void BuildHistoryPage()
		{
			historyListStore = controller.HistoryListStore;
			
			TreeViewColumn itemColumn = new TreeViewColumn();
			Gtk.CellRendererText itemCell = new Gtk.CellRendererText ();
			itemColumn.Title = _("Item");
			itemColumn.PackStart (itemCell, true);
			itemColumn.SetCellDataFunc (itemCell, new Gtk.TreeCellDataFunc (RenderItem));
				
			historyTreeView.AppendColumn(itemColumn);
			historyTreeView.Model = historyListStore;
			
			
		}


		protected virtual void OnAddFeedButtonClicked (object sender, System.EventArgs e)
		{
			logger.Debug("Appending " + feedUrlEntry.Text + " to ListStore");
			feedListStore.AppendValues(feedUrlEntry.Text);
			filterFeedListStore.AppendValues(feedUrlEntry.Text);
			controller.AddWatcher(feedUrlEntry.Text);
		}


		protected virtual void OnRemoveFeedButtonClicked (object sender, System.EventArgs e)
		{
			TreeIter iter;
			
			if(!feedTreeView.Selection.GetSelected(out iter))
				return;
			
			string url = (string) feedListStore.GetValue(iter, 0);
			
			feedListStore.Remove(ref iter);
			controller.RemoveWatcher(url);
			
			
			TreeIter feedIter = FeedToIter(url);
			filterFeedListStore.Remove(ref feedIter );
			
			logger.Debug("Removed feed: " + url);
		}


		protected virtual void OnAddFilterButtonClicked (object sender, System.EventArgs e)
		{
			RssFilter filter = new RssFilter();
			filter.SavePath = controller.SavePath;
			filterListStore.AppendValues(filter);
			controller.AddFilter(filter);
		}


		protected virtual void OnRemoveFilterButtonClicked (object sender, System.EventArgs e)
		{
			TreeIter iter;
			
			if(!filterTreeView.Selection.GetSelected(out iter))
				return;
			
			RssFilter filter = (RssFilter) filterTreeView.Model.GetValue(iter, 0);
			
			controller.RemoveFilter(filter);
			filterListStore.Remove(ref iter);
			logger.Debug("Removed row from ListStore");
		}


		protected virtual void OnRefreshFeedButtonClicked (object sender, System.EventArgs e)
		{
			UpdateFeeds();
		}


		protected virtual void OnFilterTreeViewSelectionChanged (object o, EventArgs args)
		{
			TreeIter iter;
			RssFilter filter;
			
			logger.Debug("Retrieving selected RssFilter");
			
			if(!filterTreeView.Selection.GetSelected(out iter))
				return;
			
			logger.Debug("Populating filter settings");
			
			filter = (RssFilter) filterListStore.GetValue(iter, 0);
			
			nameEntry.Changed -= OnRssFilterValueChanged;
			includeEntry.Changed -= OnRssFilterValueChanged;
			excludeEntry.Changed -= OnRssFilterValueChanged;
			filterFeedCombobox.Changed -= OnRssFilterValueChanged;
			savePathChooserButton.CurrentFolderChanged -= OnRssFilterValueChanged;
			
			Console.Out.WriteLine("Trying to get iter for feed: " + filter.Feed);
			filterFeedCombobox.SetActiveIter(FeedToIter(filter.Feed));
			nameEntry.Text = filter.Name;
			includeEntry.Text = filter.Include;
			excludeEntry.Text = filter.Exclude;
			
			if(savePathChooserButton.SetCurrentFolder(filter.SavePath))
				logger.Error("Failed to switch file chooser folder for filter " + filter.Name);
			
			nameEntry.Changed += OnRssFilterValueChanged;
			includeEntry.Changed += OnRssFilterValueChanged;
			excludeEntry.Changed += OnRssFilterValueChanged;
			filterFeedCombobox.Changed += OnRssFilterValueChanged;
			savePathChooserButton.CurrentFolderChanged += OnRssFilterValueChanged;
		}
		
		private TreeIter FeedToIter(string feed)
		{
			TreeIter iter;
			string listfeed;
			
			if(!filterFeedListStore.GetIterFirst(out iter))
				return allIter;
				
			while (filterFeedListStore.IterNext(ref iter)){
				listfeed = (string) filterFeedListStore.GetValue(iter, 0) ;
				Console.Out.WriteLine("FEED COMPARED " + listfeed + " VS " + feed);
				if(feed == listfeed)
					return iter;
			}
			
			return allIter;
		}
		
		private void OnRssFilterValueChanged(object sender, EventArgs args)
		{
			RssFilter filter;
			
			filter = GetSelectedFilter();
			
			if(filter == null)
				return;
			
			filter.Name = nameEntry.Text;
			filter.Include = includeEntry.Text;
			filter.Exclude = excludeEntry.Text;
			filter.Feed = filterFeedCombobox.ActiveText;
			filter.SavePath = savePathChooserButton.CurrentFolder;
		}
		
		
		private RssFilter GetSelectedFilter()
		{
			TreeIter iter;
			
			logger.Debug("Retrieving selected RssFilter");
			
			if(!filterTreeView.Selection.GetSelected(out iter))
				return null;
			
			logger.Debug("Populating filter settings");
			
			return (RssFilter) filterListStore.GetValue(iter, 0);
		}

		protected virtual void OnRemoveHistoryButtonClicked (object sender, System.EventArgs e)
		{
			TreeIter iter;
			RssItem item;
			
			if(!historyTreeView.Selection.GetSelected(out iter))
				return;
			
			item = (RssItem) controller.HistoryListStore.GetValue(iter, 0);
			
			if(item == null)
				return;
				
			if(controller.History.Contains(item))
				controller.History.Remove(item);
				
			controller.HistoryListStore.Remove(ref iter);
		}

		protected virtual void OnClearHistoryButtonClicked (object sender, System.EventArgs e)
		{
			controller.HistoryListStore.Clear();
			controller.History.Clear();
		}

		protected virtual void OnFeedComboboxChanged (object sender, System.EventArgs e)
		{
			feedfilter.Refilter();
		}
		
		private static string _(string s)
		{
			return Mono.Unix.Catalog.GetString(s);
		}
	}
}
