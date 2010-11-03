//
// PreferencesDialog.cs
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
using System.Collections.Generic;
using MonoTorrent.Client;
using MonoTorrent.Client.Encryption;
using Gtk;

namespace Monsoon
{
	public partial class PreferencesDialog : Gtk.Dialog
	{
		private static NLog.Logger logger = MainClass.DebugEnabled ? NLog.LogManager.GetCurrentClassLogger () : new EmptyLogger ();

		private EngineSettings engineSettings;
		private PreferencesSettings prefSettings;
		private InterfaceSettings interfaceSettings;
		
		private FileChooserButton downloadLocationButton;
		private FileChooserButton torrentStorageLocationButton;
		private FileChooserButton importLocationButton;

		private LabelController labelController;
		private LabelTreeView labelTreeView;
		
		private Gtk.StatusIcon trayIcon;
		//private IconEntry selectIcon;
		private string selectedIcon;
		private Button selectButton;
		
		public PreferencesDialog(MainWindow mainWindow)
		{
			this.engineSettings = SettingsManager.EngineSettings;
			this.prefSettings = SettingsManager.Preferences; 
			this.labelController = mainWindow.LabelController;
			this.trayIcon = mainWindow.TrayIcon;
			this.interfaceSettings = mainWindow.InterfaceSettings;

			Build();
			buildFoldersPanel();
			buildImportPanel();
			buildLabelPage();
			buildConnectionPage();
			restorePreferences();
			BuildGeneralPage();
			SetNames ();
			upnpCheckBox.Toggled += OnUpnpCheckBoxToggled;
			spinMaxActive.Sensitive = false;
			spinMaxSeed.Sensitive = false;
//			spinMaxActive.ValueChanged += delegate {
//				DownloadQueueService queue = ServiceManager.Get <DownloadQueueService> ();
//				queue.MaxActive = spinMaxActive.ValueAsInt;
//			};
			spinMaxDownload.ValueChanged += delegate {
				DownloadQueueService queue = ServiceManager.Get <DownloadQueueService> ();
				queue.MaxDownloads = spinMaxActive.ValueAsInt;
			};
//			spinMaxSeed.ValueChanged += delegate {
//				DownloadQueueService queue = ServiceManager.Get <DownloadQueueService> ();
//				queue.MaxSeeds = spinMaxActive.ValueAsInt;
//			};
		}

		void SetNames ()
		{
			queuePage.LabelProp = _("Queue");
			labelActive.Text = "Active";
			lblMaxActive.Text = "Max Active";
			lblMaxDownload.Text = "Max Downloading";
			lblMaxSeed.Text = "Max Seeding";
		}
		
		public void SetPageIndex(int index)
		{
			prefNotebook.Page = index;
		}
		
		private void BuildGeneralPage()
		{
			lblToolbarStyle.Text = _("Toolbar Style");
			string [] styles = Enum.GetNames (typeof (Gtk.ToolbarStyle));
			foreach (string s in styles)
				comboToolbarStyle.AppendText (s);
			comboToolbarStyle.AppendText ("System");
			comboToolbarStyle.Changed += delegate {
				if (comboToolbarStyle.ActiveText == "System")
					interfaceSettings.ToolbarStyle = null;
				else
					interfaceSettings.ToolbarStyle = (ToolbarStyle) Enum.Parse (typeof (Gtk.ToolbarStyle), comboToolbarStyle.ActiveText, true);
			};
			if (interfaceSettings.ToolbarStyle.HasValue)
				
				comboToolbarStyle.Active = Array.IndexOf <string> (styles, interfaceSettings.ToolbarStyle.Value.ToString ());
			else
				comboToolbarStyle.Active = styles.Length;
			loadDialogCheckButton.Active = interfaceSettings.ShowLoadDialog;
			minimizeTrayCheckButton.Active = prefSettings.QuitOnClose;
			enableTrayCheckButton.Active = prefSettings.EnableTray;
			enableNotificationsCheckButton.Active = prefSettings.EnableNotifications;
			
			enableNotificationsCheckButton.Toggled += OnEnableNotificationsToggled;
			minimizeTrayCheckButton.Toggled += OnMinimizeTrayToggled;
			enableTrayCheckButton.Toggled += OnEnableTrayToggled;
			loadDialogCheckButton.Toggled +=  OnLoadDialogToggled;
			
			minimizeTrayCheckButton.Sensitive = prefSettings.EnableTray;
		}
		
		private void OnLoadDialogToggled (object sender, EventArgs args)
		{
			interfaceSettings.ShowLoadDialog = loadDialogCheckButton.Active;
		}
		
		private void OnEnableNotificationsToggled (object sender, EventArgs args)
		{
			prefSettings.EnableNotifications = enableNotificationsCheckButton.Active;
		}
		
		private void OnMinimizeTrayToggled (object sender, EventArgs args)
		{
			prefSettings.QuitOnClose = minimizeTrayCheckButton.Active;
		}
		
		private void OnEnableTrayToggled (object sender, EventArgs args)
		{
			prefSettings.EnableTray = enableTrayCheckButton.Active;
			if(!prefSettings.EnableTray){
				trayIcon.Visible = false;
				prefSettings.QuitOnClose = prefSettings.EnableTray;
				minimizeTrayCheckButton.Active = prefSettings.EnableTray;
			} else {
				trayIcon.Visible = true;
			}

			minimizeTrayCheckButton.Sensitive = prefSettings.EnableTray;
		}
        
        private void OnEncryptionChanged (object sender, EventArgs args)
        {
            // FIXME: Don't use the indices like this
            // It's prone to failure if things change later.
            
            // Encryption off
            if (encryptionCombo.Active == 0)
            {
                engineSettings.AllowedEncryption = EncryptionTypes.PlainText;
                engineSettings.PreferEncryption = false;
            }
            // Allow encryption
            else if (encryptionCombo.Active == 1)
            {
                engineSettings.AllowedEncryption = EncryptionTypes.All;
                engineSettings.PreferEncryption = false;
            }
            // Prefer encrypytion, but allow unencrypted
            else if (encryptionCombo.Active == 2)
            {
                engineSettings.AllowedEncryption = EncryptionTypes.All;
                engineSettings.PreferEncryption = true;
            }
            // Force only encrypted connections
            else if (encryptionCombo.Active == 3)
            {
                engineSettings.AllowedEncryption = EncryptionTypes.RC4Full | EncryptionTypes.RC4Header;
                engineSettings.PreferEncryption = true;
            }
        }
		
		private void buildConnectionPage()
		{
			portSpinButton.SetRange(0, 65535);
			maxConnectionsSpinButton.SetRange(0, int.MaxValue);
			maxDownloadSpeedSpinButton.SetRange(0, int.MaxValue);
			maxUploadSpeedSpinButton.SetRange(0, int.MaxValue);
            encryptionCombo.Changed += OnEncryptionChanged;
            
                        // FIXME: Don't use the indices like this
            // It's prone to failure if things change later.
            
            if (engineSettings.AllowedEncryption == EncryptionTypes.PlainText)
            {
                encryptionCombo.Active = 0;
            }
            
            // Allow encryption
            else if (engineSettings.AllowedEncryption == EncryptionTypes.All)
            {
                if (!engineSettings.PreferEncryption)
                    encryptionCombo.Active = 1;
                else
                    encryptionCombo.Active = 2;
            }
            else if (engineSettings.AllowedEncryption == (EncryptionTypes.RC4Full | EncryptionTypes.RC4Header))
            {
                encryptionCombo.Active = 3;
            }
		}
		
		private void buildLabelPage()
		{
			labelTreeView = new LabelTreeView (labelController, false);
			labelTreeView.sizeColumn.Visible = false;
			
			labelTreeView.Selection.Changed += OnLabelSelectionChanged;
			
			labelScrolledWindow.AddWithViewport(labelTreeView);
			labelScrolledWindow.ShowAll();

			//iconButton.Image = new Gtk.Image(Gtk.IconTheme.Default.LoadIcon("gtk-about", 32, 0));
			//iconButton.Sensitive = true;
			
			/*
			selectIcon = new IconEntry("", "Select an Icon");
			iconEntryBox.Add(selectIcon);
			selectIcon.ShowAll();
			*/
			
			selectButton = new Button(_("Browse Icon"));
			selectButton.Clicked += OnIconButtonClicked;
			selectButton.Show();
			iconEntryBox.Add(selectButton);
		}
		
		private void OnLabelSelectionChanged(object o, System.EventArgs args)
		{
			TreeIter iter;
			TreeModel model;

			if (((TreeSelection)o).GetSelected (out model, out iter))
			{
				TorrentLabel label = (TorrentLabel) model.GetValue (iter, 0);
				if(label.Name == "All" || label.Name == "Downloading" || label.Name == "Seeding")
					removeLabelButton.Sensitive = false;
				else
					removeLabelButton.Sensitive = true;
				return;
			}
			removeLabelButton.Sensitive = false;
		}
		
		private void buildFoldersPanel()
		{
			downloadLocationButton = new FileChooserButton(_("Download location"), FileChooserAction.SelectFolder);
			downloadLocationButton.SetCurrentFolder(engineSettings.SavePath);
			
			downloadLocationButton.CurrentFolderChanged += OnDownloadLocationButtonFolderChanged;
			foldersTable.Attach(downloadLocationButton, 1, 2, 0, 1);
			downloadLocationButton.Show();
			
			torrentStorageLocationButton = new FileChooserButton(_("Torrent storage location"), FileChooserAction.SelectFolder);
			
			torrentStorageLocationButton.SetCurrentFolder(prefSettings.TorrentStorageLocation);
			
			torrentStorageLocationButton.CurrentFolderChanged += OnTorrentStorageLocationFolderChanged;
			foldersTable.Attach(torrentStorageLocationButton, 1, 2, 1, 2);
			torrentStorageLocationButton.Show();
		}
		
		private void buildImportPanel()
		{
			importLocationButton = new FileChooserButton(_("Import folder to scan"), FileChooserAction.SelectFolder);
			importLocationButton.SetCurrentFolder(prefSettings.ImportLocation);
			
			importLocationButton.CurrentFolderChanged += OnImportLocationFolderChanged;
			importDirectoryHbox.Add(importLocationButton);
			if(!importTorrentsCheckBox.Active)
				importLocationButton.Sensitive = false;
			importLocationButton.Show();
		}
		
		private void restorePreferences()
		{
			portSpinButton.Value = engineSettings.ListenPort;
			maxConnectionsSpinButton.Value = engineSettings.GlobalMaxConnections;
			maxDownloadSpeedSpinButton.Value = engineSettings.GlobalMaxDownloadSpeed / 1024;
			maxUploadSpeedSpinButton.Value = engineSettings.GlobalMaxUploadSpeed / 1024;
			
			upnpCheckBox.Active = prefSettings.UpnpEnabled;
			startNewTorrentsCheckBox.Active = prefSettings.StartNewTorrents;
			importTorrentsCheckBox.Active = prefSettings.ImportEnabled;
			removeOnImportCheckButton.Active = prefSettings.RemoveOnImport;
		}

		protected virtual void OnPortSpinButtonValueChanged (object sender, System.EventArgs e)
		{
			engineSettings.ListenPort = (int)portSpinButton.Value;
		}

		protected virtual void OnMaxConnectionsSpinButtonValueChanged (object sender, System.EventArgs e)
		{
			engineSettings.GlobalMaxConnections = (int) maxConnectionsSpinButton.Value;
		}
		
		private void OnDownloadLocationButtonFolderChanged (object sender, System.EventArgs e)
		{
			engineSettings.SavePath = downloadLocationButton.CurrentFolder;
		}
		
		private void OnTorrentStorageLocationFolderChanged(object sender, System.EventArgs e)
		{
			prefSettings.TorrentStorageLocation = torrentStorageLocationButton.Filename;
		}
		
		private void OnImportLocationFolderChanged(object sender, System.EventArgs e)
		{
			prefSettings.ImportLocation = importLocationButton.Filename;
		}

		protected virtual void OnUpnpCheckBoxToggled (object sender, System.EventArgs e)
		{
			prefSettings.UpnpEnabled = upnpCheckBox.Active;
		}

		protected virtual void OnMaxDownloadSpeedSpinButtonValueChanged (object sender, System.EventArgs e)
		{
			engineSettings.GlobalMaxDownloadSpeed = (int)maxDownloadSpeedSpinButton.Value * 1024;
		}

		protected virtual void OnMaxUploadSpeedSpinButtonValueChanged (object sender, System.EventArgs e)
		{
			engineSettings.GlobalMaxUploadSpeed = (int)maxUploadSpeedSpinButton.Value * 1024;
		}

		protected virtual void OnImportTorrentsCheckBoxClicked (object sender, System.EventArgs e)
		{
			importLocationButton.Sensitive = importTorrentsCheckBox.Active;
			prefSettings.ImportEnabled = importTorrentsCheckBox.Active;
		}

		protected virtual void OnStartNewTorrentsCheckBoxClicked (object sender, System.EventArgs e)
		{
			prefSettings.StartNewTorrents = startNewTorrentsCheckBox.Active;
		}

		protected virtual void OnIconButtonClicked (object sender, System.EventArgs e)
		{
			/*
			IconSelection iconSelection = new IconSelection();
			
			Dialog dialog = new Dialog("Select Icon", this, DialogFlags.DestroyWithParent);
			dialog.Modal = true;
			
			dialog.Add(iconSelection);
			//dialog.AddButton("Close", ResponseType.Close);
			dialog.Run();
			
			dialog.Destroy();
			*/
			
			Gtk.FileChooserDialog chooser = new FileChooserDialog(
				_("Select an Icon"),
				this, FileChooserAction.Open,
				Gtk.Stock.Cancel, ResponseType.Cancel,
				Gtk.Stock.Open, ResponseType.Ok
			);
			Image previewImage = new Image();
			previewImage.IconSize = 32;
			chooser.PreviewWidget = previewImage;
			chooser.UpdatePreview += Event.Wrap ((EventHandler) delegate {
				try {
					if (chooser.PreviewFilename == null) {
						return;
					}
					
					Gdk.Pixbuf pixbuf = new Gdk.Pixbuf(chooser.PreviewFilename);
					previewImage.Pixbuf = pixbuf;
					chooser.PreviewWidgetActive = true;
				} catch {
					chooser.PreviewWidgetActive = false;
				}
			});
			
			if (chooser.Run() == (int) ResponseType.Ok) {
				logger.Debug("OnIconButtonClicked(): selected icon: " + chooser.Filename);
				Image img = new Gtk.Image(chooser.Filename);
				selectButton.Image = img;
				if (img.StorageType != ImageType.Image) {
					logger.Error("OnIconButtonClicked(): invalid icon: " + chooser.Filename);
					selectedIcon = null;
				} else {
					selectedIcon = chooser.Filename;
				}
			}
			
			chooser.Destroy();
		}

		protected virtual void OnAddLabelButtonClicked (object sender, System.EventArgs e)
		{
			TorrentLabel label;
			if (selectedIcon != null) {
				label = new TorrentLabel(nameEntry.Text, selectedIcon);
			} else {
				label = new TorrentLabel(nameEntry.Text);
			}
			labelController.Add(label);
		}

		protected virtual void OnRemoveLabelButtonClicked (object sender, System.EventArgs e)
		{
			TreeIter iter;
			
			if(labelTreeView.Selection.GetSelected(out iter))
				labelController.Remove ((TorrentLabel) labelTreeView.Model.GetValue (iter, 0));
		}

		protected virtual void OnRemoveOnImportCheckButtonClicked (object sender, System.EventArgs e)
		{
			prefSettings.RemoveOnImport = removeOnImportCheckButton.Active;
		}

		protected virtual void OnNameEntryChanged (object sender, System.EventArgs e)
		{
			if(nameEntry.Text.Length == 0)
				addLabelButton.Sensitive = false;
			else
				addLabelButton.Sensitive = true;
		}

        public override void Destroy()
        {
            labelTreeView.Destroy();
            base.Destroy();
        }
		private static string _(string s)
		{
			return Mono.Unix.Catalog.GetString(s);
		}
	}
}
