//
// CreateTorrentDialog.cs
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
using System.Collections.Generic;
using Gtk;
using MonoTorrent.Common;
using MonoTorrent.BEncoding;

namespace Monsoon
{
	
	
	public partial class CreateTorrentDialog : Gtk.Dialog
	{
		private TorrentController torrentController;
		
		private FileChooserButton newTorrentLocationButton;
		private ListStore trackerListStore;
		
		private TorrentCreator creator;
		private CreateTorrentProgressDialog progressDialog;
		private FileChooserDialog savePathChooser;
		
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		public CreateTorrentDialog(TorrentController torrentController)
		{
			this.Build();
			this.torrentController = torrentController;
			
			newTorrentLocationButton = new FileChooserButton("Select file", FileChooserAction.Open);
			
			selectFileHbox.Add(newTorrentLocationButton);
			newTorrentLocationButton.Show();
			
			BuildTrackerWidgets();
			
			
		}
		
		public string SavePath
		{
			get { 
				if(newTorrentLocationButton.Action == FileChooserAction.SelectFolder)
					return newTorrentLocationButton.CurrentFolder;
				else
					return newTorrentLocationButton.Filename;
			}
		}
		
		public string Comment
		{
			get { return commentEntry.Text; }	
		}
		
		public bool StartSeeding
		{
			get { return startSeedingCheckBox.Active; } 
		}
		
		private void BuildTrackerWidgets()
		{
			trackerListStore = new Gtk.ListStore (typeof (string));
			trackerTreeView.Model = trackerListStore;
			
			TreeViewColumn trackerColumn = new TreeViewColumn ();
			trackerColumn.Title = _("Trackers");
			
			CellRendererText trackerTextCell = new CellRendererText ();
			trackerColumn.PackStart(trackerTextCell, true);
			
			trackerTreeView.AppendColumn(trackerColumn);
			trackerColumn.AddAttribute(trackerTextCell, "text", 0);
			
			trackerTreeView.Selection.Changed += OnTrackerTreeSelectionChanged;
			trackerListStore.RowDeleted += OnTrackerListRowDeleted;
		}

		protected virtual void OnTrackerEntryChanged (object sender, System.EventArgs e)
		{ 
			Uri uri = null;
			
			try{
				uri = new Uri(trackerEntry.Text);
			} catch(UriFormatException){
				addTrackerButton.Sensitive = false;
			}
			
			if(uri != null)
				addTrackerButton.Sensitive = true;
		}

		protected virtual void OnAddTrackerButtonClicked (object sender, System.EventArgs e)
		{
			trackerListStore.AppendValues(trackerEntry.Text);
			updateButtonOk();
		}

		protected virtual void OnRemoveTrackerButtonClicked (object sender, System.EventArgs e)
		{
			TreeIter iter;
			
			trackerTreeView.Selection.GetSelected(out iter);
			trackerListStore.Remove(ref iter);
		}
		
		private void OnTrackerListRowDeleted(object sender, System.EventArgs e)
		{
			updateButtonOk();
		}
			
		private void OnTrackerTreeSelectionChanged(object sender, System.EventArgs e)
		{
			TreeIter iter;
			
			if(trackerTreeView.Selection.GetSelected(out iter))
				removeTrackerButton.Sensitive = true;
			else
				removeTrackerButton.Sensitive = false;				
		}
		
		private void updateButtonOk()
		{
			if(trackerListStore.IterNChildren() > 0)
				buttonOk.Sensitive = true;
			else			
				buttonOk.Sensitive = false;
		}
		
		public string [] GetTrackers()
		{
			TreeIter iter;
			List<string> trackers = new List<string> ();
			
			if(!trackerListStore.GetIterFirst(out iter))
				return null;
			
			while(true){
				trackers.Add((string)trackerListStore.GetValue(iter, 0));
				if(!trackerListStore.IterNext(ref iter))
					break;
			}
			
			return trackers.ToArray();			
		}

		protected virtual void OnCreateButtonClicked (object sender, System.EventArgs e)
		{
			savePathChooser = new FileChooserDialog("Save Torrent As...", this, FileChooserAction.Save, Gtk.Stock.Cancel, ResponseType.Cancel, Gtk.Stock.Save, ResponseType.Accept);
			
			ResponseType result = (ResponseType) savePathChooser.Run();
			if(result == ResponseType.Accept){
				savePathChooser.HideAll();
				createTorrent();
			}
			
			if(result == ResponseType.Cancel || result == ResponseType.DeleteEvent){
				savePathChooser.Destroy();
				//createTorrentDialog.Destroy();
				return;
			}
						
		}
		
		private void createTorrent()
		{
			creator = new TorrentCreator();
			
			progressDialog = new CreateTorrentProgressDialog();
			
			// TODO: Read the multi-tracker spec -- learn the proper way to add multiple trackers
			creator.Announces.Add(new List<string>());
            		foreach(string s in GetTrackers())
            			creator.Announces[0].Add(s);
            		
			creator.Comment = Comment;
			creator.CreatedBy = Defines.ApplicationName;
			
			creator.Path = SavePath;

			creator.Hashed += OnHashed;
			
			TorrentCreatorAsyncResult creatorResult = creator.BeginCreate(null, BeginCreateCb);
			
			ResponseType cancelResult = (ResponseType) progressDialog.Run();
			if(cancelResult == ResponseType.Cancel){
				creatorResult.Abort();
				try{
					creator.EndCreate(creatorResult);
					progressDialog.Destroy();
				} catch (Exception e) {
					logger.ErrorException("Unable to end creation" + e.Message, e);
				}
			}
		}
		
		private void OnHashed(object sender, TorrentCreatorEventArgs args)
		{
			Gtk.Application.Invoke (delegate {
				progressDialog.Progress = args.OverallCompletion;
			});
		}

		private void BeginCreateCb(IAsyncResult result)
		{	
			logger.Debug("Torrent creation finished");
			progressDialog.Destroy();
			try{
				BEncodedDictionary dict = creator.EndCreate(result);
				System.IO.File.WriteAllBytes(savePathChooser.Filename, dict.Encode());
				if(startSeedingCheckBox.Active)
				{
					Torrent t = Torrent.Load(savePathChooser.Filename);
					BitField bf = new BitField(t.Pieces.Count);
					bf.Not();
					MonoTorrent.Client.FastResume fresume = new MonoTorrent.Client.FastResume (t.InfoHash, bf, new List<MonoTorrent.Client.Peer>());
					torrentController.FastResume.Add(fresume);
					torrentController.addTorrent(t, startSeedingCheckBox.Active);
				}
				logger.Debug("Torrent file created");
			}catch(Exception e){
				logger.Error("Failed to create torrent - " + e.Message);
			}
		}

		protected virtual void OnFolderRadioButtonToggled (object sender, System.EventArgs e)
		{
			if(newTorrentLocationButton.Action == FileChooserAction.Open)
				newTorrentLocationButton.Action = FileChooserAction.SelectFolder;
			else
				newTorrentLocationButton.Action = FileChooserAction.Open;
		}

		protected virtual void OnFileRadioButtonToggled (object sender, System.EventArgs e)
		{
		}
		
		private static string _(string s)
		{
			return Mono.Unix.Catalog.GetString(s);
		}
	}
}
