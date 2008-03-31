//
// UserTorrentSettings.cs
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
using MonoTorrent.Client;

namespace Monsoon
{
	
	public class UserTorrentSettings : ISettings
	{
	 	static readonly string SETTINGS_PATH = "TorrentSettings/";
	 	
		static readonly string UploadSlotsKey = SETTINGS_PATH + "UploadSlots";
		static readonly string MaxConnectionsKey = SETTINGS_PATH + "MaxConnections";
		static readonly string MaxDownloadSpeedKey = SETTINGS_PATH + "MaxDownloadSpeed";
		static readonly string MaxUploadSpeedKey = SETTINGS_PATH + "MaxUploadSpeed";
		static readonly string FastResumeEnabledKey = SETTINGS_PATH + "FastResumeEnabled";
		
		private TorrentSettings settings;
	 	
	 	
		public UserTorrentSettings()
		{
			settings = new TorrentSettings();
			Restore();
		}
		
		public void Restore()
		{
			GconfSettingsStorage gconf = new GconfSettingsStorage();
			try {
				UploadSlots = (int) gconf.Retrieve(UploadSlotsKey);
			} catch(SettingNotFoundException) {
				
			}
			
			try {
				MaxConnections = (int) gconf.Retrieve(MaxConnectionsKey);
			} catch(SettingNotFoundException) {
				
			}
			
			try{
				MaxDownloadSpeed = (int) gconf.Retrieve(MaxDownloadSpeedKey);
			} catch(SettingNotFoundException){
				
			}
			
			try {
				MaxUploadSpeed = (int) gconf.Retrieve(MaxUploadSpeedKey);
			} catch(SettingNotFoundException){
				
			}
			
			try{
				FastResumeEnabled = (bool) gconf.Retrieve(FastResumeEnabledKey);
			} catch(SettingNotFoundException){
				
			}
			
		}
		
		public void Store()
		{
			GconfSettingsStorage gconf = new GconfSettingsStorage();
			
			gconf.Store(UploadSlotsKey, uploadSlots);
			gconf.Store(MaxConnectionsKey, maxConnections);
			gconf.Store(MaxDownloadSpeedKey, maxDownloadSpeed);
			gconf.Store(MaxUploadSpeedKey, maxUploadSpeed);
			gconf.Store(FastResumeEnabledKey, fastResumeEnabled);
		}
		
		public int UploadSlots
		{
			get{ return settings.UploadSlots; }
			set{ settings.UploadSlots = value;}
		}
		
		public int MaxConnections
		{
			get{ return settings.MaxConnections; }
			set{ settings.MaxConnections = value;}
		}
		
		public int MaxDownloadSpeed
		{
			get{ return settings.MaxDownloadSpeed; }
			set{ settings.MaxDownloadSpeed = value; }
		}
		
		public int MaxUploadSpeed
		{
			get{ return settings.MaxUploadSpeed; }
			set{ settings.MaxUploadSpeed = value; }
		}
		
		public bool FastResumeEnabled
		{
			get{ return settings.FastResumeEnabled; }
			set{ settings.FastResumeEnabled = value; }
		}
	}
}
