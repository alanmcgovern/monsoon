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
	public class GconfPreferencesSettingsController : GConfSettings <PreferencesSettings>
	{
		static string SETTINGS_PATH = "PreferencesSettings/";
		
		public override void Load ()
		{
		 	try { Settings.EnableNotifications = Get <bool> (SETTINGS_PATH + "enableNotifications"); }
			catch (SettingNotFoundException e) {}
			
			try { Settings.QuitOnClose = Get <bool> (SETTINGS_PATH + "quitOnClose"); }
			catch (SettingNotFoundException e) {}
			
			try { Settings.EnableTray = Get <bool> (SETTINGS_PATH + "enableTray"); }
			catch (SettingNotFoundException e) {}
			
			try { Settings.TorrentStorageLocation = Get <string> (SETTINGS_PATH + "torrentStorageLocation"); }
			catch (SettingNotFoundException e) {}
			
			try { Settings.ImportLocation = Get <string> (SETTINGS_PATH + "importLocation"); }
			catch (SettingNotFoundException e) {}
			
			try { Settings.UpnpEnabled = Get <bool> (SETTINGS_PATH + "upnpEnabled"); }
			catch (SettingNotFoundException e) {}
			
			try { Settings.StartNewTorrents = Get <bool> (SETTINGS_PATH + "startNewTorrents"); }
			catch (SettingNotFoundException e) {}
			
			try { Settings.ImportEnabled = Get <bool> (SETTINGS_PATH + "importEnabled"); }
			catch (SettingNotFoundException e) {}
			
			try { Settings.RemoveOnImport = Get <bool> (SETTINGS_PATH + "removeOnImport"); }
			catch (SettingNotFoundException e) {}
		}
		
		public override void Save ()
		{
			GconfSettingsStorage gconf = GconfSettingsStorage.Instance;
			
			Set (SETTINGS_PATH + "enableNotifications", Settings.EnableNotifications);
			Set (SETTINGS_PATH + "quitOnClose", Settings.QuitOnClose);
			Set (SETTINGS_PATH + "enableTray", Settings.EnableTray);
			Set (SETTINGS_PATH + "torrentStorageLocation", Settings.TorrentStorageLocation);
			Set (SETTINGS_PATH + "startNewTorrents", Settings.StartNewTorrents);
			Set (SETTINGS_PATH + "importLocation", Settings.ImportLocation);
			Set (SETTINGS_PATH + "upnpEnabled", Settings.UpnpEnabled);
			Set (SETTINGS_PATH + "importEnabled", Settings.ImportEnabled);
			Set (SETTINGS_PATH + "removeOnImport", Settings.RemoveOnImport);
		}
	}
}
