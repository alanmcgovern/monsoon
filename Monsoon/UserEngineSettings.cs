//
// UserEngineSettings.cs
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
using MonoTorrent.Common;

namespace Monsoon
{
	
	public class UserEngineSettings : EngineSettings, ISettings
	{
		static string SETTINGS_PATH = "EngineSettings/";
		
		static readonly string AllowLegacyConnectionsKey = SETTINGS_PATH + "AllowLegacyConnections";
		static readonly string GlobalMaxConnectionsKey = SETTINGS_PATH + "GlobalMaxConnections";
		static readonly string GlobalMaxDownloadSpeedKey = SETTINGS_PATH + "GlobalMaxDownloadSpeed";
		static readonly string GlobalMaxHalfOpenConnectionsKey = SETTINGS_PATH + "GlobalMaxHalfOpenConnections";
		static readonly string GlobalMaxUploadSpeedKey = SETTINGS_PATH + "GlobalMaxUploadSpeed";
		static readonly string ListenPortKey = SETTINGS_PATH + "ListenPort";
		static readonly string SavePathKey = SETTINGS_PATH + "SavePath";
		static readonly string MaxReadRateKey = SETTINGS_PATH + "MaxReadRate";
		static readonly string MaxWriteRateKey = SETTINGS_PATH + "MaxWriteRate";

		public UserEngineSettings()
		{
			Restore();				
		}
		
		public void Restore()
		{
			GconfSettingsStorage gconf = GconfSettingsStorage.Instance;
			try{
				AllowLegacyConnections = (bool)gconf.Retrieve(AllowLegacyConnectionsKey);
			} catch(SettingNotFoundException){
				
			}
			
			try{
				GlobalMaxConnections = (int)gconf.Retrieve(GlobalMaxConnectionsKey);
			} catch(SettingNotFoundException){
				
			}
			
			try{
				GlobalMaxDownloadSpeed = (int)gconf.Retrieve(GlobalMaxDownloadSpeedKey);
			} catch(SettingNotFoundException){
				
			}
			
			try{
				GlobalMaxHalfOpenConnections = (int)gconf.Retrieve(GlobalMaxHalfOpenConnectionsKey);
			} catch(SettingNotFoundException){
				
			}
			
			try{
				GlobalMaxUploadSpeed = (int)gconf.Retrieve(GlobalMaxUploadSpeedKey);
			} catch(SettingNotFoundException){
				
			}
			
			try{
				ListenPort = (int)gconf.Retrieve(ListenPortKey);
			} catch(SettingNotFoundException){
				
			}
			
			try{
				MaxReadRate = (int)gconf.Retrieve(MaxReadRateKey);
			} catch(SettingNotFoundException){
				
			}
			
			try{
				MaxWriteRate = (int)gconf.Retrieve(MaxWriteRateKey);
			} catch(SettingNotFoundException){
				
			}
			
			try{ 
				SavePath = (string)gconf.Retrieve(SavePathKey);
			} catch(SettingNotFoundException){
				
			} finally{
				// If savePath has not been set, fallback to user's home directory
				if (String.IsNullOrEmpty(SavePath)) {
					SavePath = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
				}
			}
			
		}
		
		public void Store()
		{	
			GconfSettingsStorage gconf = GconfSettingsStorage.Instance;
			
			gconf.Store(AllowLegacyConnectionsKey, AllowLegacyConnections);
			gconf.Store(GlobalMaxConnectionsKey, GlobalMaxConnections);
			gconf.Store(GlobalMaxDownloadSpeedKey, GlobalMaxDownloadSpeed);
			gconf.Store(GlobalMaxHalfOpenConnectionsKey, GlobalMaxHalfOpenConnections);
			gconf.Store(GlobalMaxUploadSpeedKey, GlobalMaxUploadSpeed);
			gconf.Store(ListenPortKey, ListenPort);
			gconf.Store(SavePathKey, SavePath);
			gconf.Store(MaxReadRateKey, MaxReadRate);
			gconf.Store(MaxWriteRateKey, MaxWriteRate);
		}
	}
}
