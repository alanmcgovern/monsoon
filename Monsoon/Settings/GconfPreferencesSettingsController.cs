// GconfPreferencesSettingsController.cs created with MonoDevelop
// User: alan at 00:57 04/04/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
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
