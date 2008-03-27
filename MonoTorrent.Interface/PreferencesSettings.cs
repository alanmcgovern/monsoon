//
// PreferencesSettings.cs
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
using System.IO;

namespace Monsoon
{
	
	
	public class PreferencesSettings: ISettings
	{
		static string SETTINGS_PATH = "PreferencesSettings/";
	 	
	 	private string torrentStorageLocation;
	 	private string importLocation;
	 	private bool startNewTorrents;
	 	private bool upnpEnabled;
	 	private bool importEnabled;
	 	private bool removeOnImport;
	 	
		private bool enableNotifications;
		private bool quitOnClose;
		private bool enableTray;
	 	
		public PreferencesSettings()
		{
			Restore();
		}
		
		public void Restore()
		{
			GconfSettingsStorage gconf = new GconfSettingsStorage();
			
			try {
				enableNotifications = (bool) gconf.Retrieve(SETTINGS_PATH + "enableNotifications");
			} catch (SettingNotFoundException) {
				enableNotifications = true;
			}
			
			try {
				quitOnClose = (bool) gconf.Retrieve(SETTINGS_PATH + "quitOnClose");
			} catch (SettingNotFoundException) {
				quitOnClose = false;
			}
			
			try {
				enableTray = (bool) gconf.Retrieve(SETTINGS_PATH + "enableTray");
			} catch (SettingNotFoundException) {
				enableTray = true;
			}
			
			try {
				torrentStorageLocation = (string) gconf.Retrieve(SETTINGS_PATH + "torrentStorageLocation");
			} catch(SettingNotFoundException) {
				torrentStorageLocation = Path.Combine(Path.Combine(Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData),"monsoon"), "torrents");
			}
			
			try {
				importLocation = (string) gconf.Retrieve(SETTINGS_PATH + "importLocation");
			} catch (SettingNotFoundException) {
			
				importLocation = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			}
			
			try {	
				upnpEnabled = (bool) gconf.Retrieve(SETTINGS_PATH + "upnpEnabled");
			} catch (SettingNotFoundException) {
				upnpEnabled = false;
			}
			
			try {
				startNewTorrents = (bool) gconf.Retrieve(SETTINGS_PATH + "startNewTorrents");
			} catch (SettingNotFoundException) {
				startNewTorrents = true;
			}
			
			try {
				importEnabled = (bool) gconf.Retrieve(SETTINGS_PATH + "importEnabled");
			} catch (SettingNotFoundException) {
				importEnabled = false;
			}
			
			try {
				removeOnImport = (bool) gconf.Retrieve(SETTINGS_PATH + "removeOnImport");
			} catch (SettingNotFoundException) {
				removeOnImport = false;
			}
				
		}
		
		public void Store()
		{
			GconfSettingsStorage gconf = new GconfSettingsStorage();
			
			gconf.Store(SETTINGS_PATH + "enableNotifications", enableNotifications);
			gconf.Store(SETTINGS_PATH + "quitOnClose", quitOnClose);
			gconf.Store(SETTINGS_PATH + "enableTray", enableTray);
			gconf.Store(SETTINGS_PATH + "torrentStorageLocation", torrentStorageLocation);
			gconf.Store(SETTINGS_PATH + "startNewTorrents", startNewTorrents);
			gconf.Store(SETTINGS_PATH + "importLocation", importLocation);
			gconf.Store(SETTINGS_PATH + "upnpEnabled", upnpEnabled);
			gconf.Store(SETTINGS_PATH + "importEnabled", importEnabled);
			gconf.Store(SETTINGS_PATH + "removeOnImport", removeOnImport);
		}
		public bool EnableNotifications {
			get { return enableNotifications; }
			set { enableNotifications = value; }
		}
		
		public bool EnableTray {
			get { return enableTray; }
			set { enableTray = value; }
		}
		
		public bool QuitOnClose {
			get { return quitOnClose; }
			set { quitOnClose = value; }
		}
		
		public string TorrentStorageLocation {
			get { return torrentStorageLocation; }
			set { torrentStorageLocation = value; }
			
		}
	 	
	 	public string ImportLocation{
	 		get { return importLocation; }
	 		set { importLocation = value; }
	 	}
	 	
	 	public bool StartNewTorrents{
	 		get { return startNewTorrents; }
	 		set { startNewTorrents = value; }
	 	}
	 	
	 	public bool UpnpEnabled{
	 		get { return upnpEnabled; }
	 		set { upnpEnabled = value; }
	 	}
	 	
	 	public bool ImportEnabled{
	 		get { return importEnabled; }
	 		set { importEnabled = value; }
	 	}
	 	
	 	public bool RemoveOnImport{
	 		get { return removeOnImport; }
	 		set { removeOnImport = value; }
	 	}
	}
}
