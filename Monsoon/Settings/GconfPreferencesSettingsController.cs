//
// GconfPreferencesSettingsController.cs
//
// Author:
//   Alan McGovern (alan.mcgovern@gmail.com)
//
// Copyright (C) 2008 Alan McGovern
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

namespace Monsoon
{
	public class GconfPreferencesSettingsController : SettingsController<PreferencesSettings>
	{
		static string SETTINGS_PATH = "PreferencesSettings/";
		
		public override void Load ()
		{
			GconfSettingsStorage gconf = GconfSettingsStorage.Instance;
			
			try {
				Settings.EnableNotifications = (bool) gconf.Retrieve(SETTINGS_PATH + "enableNotifications");
			} catch (SettingNotFoundException) {
				Settings.EnableNotifications = true;
			}
			
			try {
				Settings.QuitOnClose = (bool) gconf.Retrieve(SETTINGS_PATH + "quitOnClose");
			} catch (SettingNotFoundException) {
				Settings.QuitOnClose = false;
			}
			
			try {
				Settings.EnableTray = (bool) gconf.Retrieve(SETTINGS_PATH + "enableTray");
			} catch (SettingNotFoundException) {
				Settings.EnableTray = true;
			}
			
			try {
				Settings.TorrentStorageLocation = (string) gconf.Retrieve(SETTINGS_PATH + "torrentStorageLocation");
			} catch(SettingNotFoundException) {
				Settings.TorrentStorageLocation = Defines.SerializedTorrentSettings;
			}
			
			try {
				Settings.ImportLocation = (string) gconf.Retrieve(SETTINGS_PATH + "importLocation");
			} catch (SettingNotFoundException) {
			
				Settings.ImportLocation = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			}
			
			try {	
				Settings.UpnpEnabled = (bool) gconf.Retrieve(SETTINGS_PATH + "upnpEnabled");
			} catch (SettingNotFoundException) {
				Settings.UpnpEnabled = true;
			}
			
			try {
				Settings.StartNewTorrents = (bool) gconf.Retrieve(SETTINGS_PATH + "startNewTorrents");
			} catch (SettingNotFoundException) {
				Settings.StartNewTorrents = true;
			}
			
			try {
				Settings.ImportEnabled = (bool) gconf.Retrieve(SETTINGS_PATH + "importEnabled");
			} catch (SettingNotFoundException) {
				Settings.ImportEnabled = false;
			}
			
			try {
				Settings.RemoveOnImport = (bool) gconf.Retrieve(SETTINGS_PATH + "removeOnImport");
			} catch (SettingNotFoundException) {
				Settings.RemoveOnImport = false;
			}
		}
		
		public override void Save ()
		{
			GconfSettingsStorage gconf = GconfSettingsStorage.Instance;
			
			gconf.Store(SETTINGS_PATH + "enableNotifications", Settings.EnableNotifications);
			gconf.Store(SETTINGS_PATH + "quitOnClose", Settings.QuitOnClose);
			gconf.Store(SETTINGS_PATH + "enableTray", Settings.EnableTray);
			gconf.Store(SETTINGS_PATH + "torrentStorageLocation", Settings.TorrentStorageLocation);
			gconf.Store(SETTINGS_PATH + "startNewTorrents", Settings.StartNewTorrents);
			gconf.Store(SETTINGS_PATH + "importLocation", Settings.ImportLocation);
			gconf.Store(SETTINGS_PATH + "upnpEnabled", Settings.UpnpEnabled);
			gconf.Store(SETTINGS_PATH + "importEnabled", Settings.ImportEnabled);
			gconf.Store(SETTINGS_PATH + "removeOnImport", Settings.RemoveOnImport);
		}
	}
}
