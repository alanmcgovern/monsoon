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
	 	static string SETTINGS_PATH = "TorrentSettings/";
	 	
	 	private int uploadSlots;
	 	private int maxConnections;
	 	private int maxDownloadSpeed;
	 	private int maxUploadSpeed;
	 	private bool fastResumeEnabled;
	 	
	 	
		public UserTorrentSettings()
		{
			Restore();
		}
		
		public void Restore()
		{
			GconfSettingsStorage gconf = new GconfSettingsStorage();
			TorrentSettings defaults = new TorrentSettings();
			try {
				uploadSlots = (int) gconf.Retrieve(SETTINGS_PATH + "UploadSlots");
			} catch(SettingNotFoundException) {
				uploadSlots = defaults.UploadSlots;
			}
			
			try {
				maxConnections = (int) gconf.Retrieve(SETTINGS_PATH + "MaxConnections");
			} catch(SettingNotFoundException) {
				maxConnections = defaults.MaxConnections;
			}
			
			try{
				maxDownloadSpeed = (int) gconf.Retrieve(SETTINGS_PATH + "MaxDownloadSpeed");
			} catch(SettingNotFoundException){
				maxDownloadSpeed = defaults.MaxDownloadSpeed;
			}
			
			try {
				maxUploadSpeed = (int) gconf.Retrieve(SETTINGS_PATH + "MaxUploadSpeed");
			} catch(SettingNotFoundException){
				maxUploadSpeed = defaults.MaxUploadSpeed;
			}
			
			try{
				fastResumeEnabled = (bool) gconf.Retrieve(SETTINGS_PATH + "FastResumeEnabled");
			} catch(SettingNotFoundException){
				fastResumeEnabled = defaults.FastResumeEnabled;
			}
			
		}
		
		public void Store()
		{
			GconfSettingsStorage gconf = new GconfSettingsStorage();
			
			gconf.Store(SETTINGS_PATH + "UploadSlots", uploadSlots);
			gconf.Store(SETTINGS_PATH + "MaxConnections", maxConnections);
			gconf.Store(SETTINGS_PATH + "MaxDownloadSpeed", maxDownloadSpeed);
			gconf.Store(SETTINGS_PATH + "MaxUploadSpeed", maxUploadSpeed);
			gconf.Store(SETTINGS_PATH + "FastResumeEnabled", fastResumeEnabled);		
		}
		
		public int UploadSlots
		{
			get{ return this.uploadSlots; }
			set{ this.uploadSlots = value;}
		}
		
		public int MaxConnections
		{
			get{ return this.maxConnections; }
			set{ this.maxConnections = value;}
		}
		
		public int MaxDownloadSpeed
		{
			get{ return this.maxDownloadSpeed; }
			set{ this.maxConnections = value; }
		}
		
		public int MaxUploadSpeed
		{
			get{ return this.maxUploadSpeed; }
			set{ this.maxUploadSpeed = value; }
		}
		
		public bool FastResumeEnabled
		{
			get{ return this.fastResumeEnabled; }
			set{ this.fastResumeEnabled = value; }
		}
	}
}
