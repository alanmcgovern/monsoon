// TorrentSettingsController.cs created with MonoDevelop
// User: alan at 00:02 04/04/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using MonoTorrent.Client;

namespace Monsoon
{
	public class GconfTorrentSettingsController : SettingsController<TorrentSettings>
	{
		static readonly string SETTINGS_PATH = "TorrentSettings/";
	 	
		static readonly string UploadSlotsKey = SETTINGS_PATH + "UploadSlots";
		static readonly string MaxConnectionsKey = SETTINGS_PATH + "MaxConnections";
		static readonly string MaxDownloadSpeedKey = SETTINGS_PATH + "MaxDownloadSpeed";
		static readonly string MaxUploadSpeedKey = SETTINGS_PATH + "MaxUploadSpeed";
		static readonly string FastResumeEnabledKey = SETTINGS_PATH + "FastResumeEnabled";

		public override void Load ()
		{
			GconfSettingsStorage gconf = GconfSettingsStorage.Instance;
			try {
				Settings.UploadSlots = (int) gconf.Retrieve(UploadSlotsKey);
			} catch(SettingNotFoundException) {
				
			}
			
			try {
				Settings.MaxConnections = (int) gconf.Retrieve(MaxConnectionsKey);
			} catch(SettingNotFoundException) {
				
			}
			
			try{
				Settings.MaxDownloadSpeed = (int) gconf.Retrieve(MaxDownloadSpeedKey);
			} catch(SettingNotFoundException){
				
			}
			
			try {
				Settings.MaxUploadSpeed = (int) gconf.Retrieve(MaxUploadSpeedKey);
			} catch(SettingNotFoundException){
				
			}
			
			try{
				Settings.FastResumeEnabled = (bool) gconf.Retrieve(FastResumeEnabledKey);
			} catch(SettingNotFoundException){
				
			}
		}
		
		public override void Save ()
		{
			GconfSettingsStorage gconf = GconfSettingsStorage.Instance;
			
			gconf.Store(UploadSlotsKey, Settings.UploadSlots);
			gconf.Store(MaxConnectionsKey, Settings.MaxConnections);
			gconf.Store(MaxDownloadSpeedKey, Settings.MaxDownloadSpeed);
			gconf.Store(MaxUploadSpeedKey, Settings.MaxUploadSpeed);
			gconf.Store(FastResumeEnabledKey, Settings.FastResumeEnabled);
		}
	}
}
