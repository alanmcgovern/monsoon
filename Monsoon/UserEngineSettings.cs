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
	
	public class UserEngineSettings : ISettings
	{
		static string SETTINGS_PATH = "EngineSettings/";
		
		static string AllowLegacyConnectionsKey = SETTINGS_PATH + "AllowLegacyConnections";
		static string GlobalMaxConnectionsKey = SETTINGS_PATH + "GlobalMaxConnections";
		static string GlobalMaxDownloadSpeedKey = SETTINGS_PATH + "GlobalMaxDownloadSpeed";
		static string GlobalMaxHalfOpenConnectionsKey = SETTINGS_PATH + "GlobalMaxHalfOpenConnections";
		static string GlobalMaxUploadSpeedKey = SETTINGS_PATH + "GlobalMaxUploadSpeed";
		static string ListenPortKey = SETTINGS_PATH + "ListenPort";
		static string SavePathKey = SETTINGS_PATH + "SavePath";
		static string MaxReadRateKey = SETTINGS_PATH + "MaxReadRate";
		static string MaxWriteRateKey = SETTINGS_PATH + "MaxWriteRate";
		
		private bool allowLegacyConnections;
		private int globalMaxConnections;
		private int globalMaxDownloadSpeed;
		private int globalMaxHalfOpenConnections;
		private int globalMaxUploadSpeed;
		private int listenPort;
		//private EncryptionType minEncryptionLevel;
		private string savePath;
		private int maxReadRate;
		private int maxWriteRate;
		
		public UserEngineSettings()
		{
			Restore();				
		}
		
		public void Restore()
		{
			GconfSettingsStorage gconf = new GconfSettingsStorage();
			EngineSettings defaults = new EngineSettings();
			try{
				allowLegacyConnections = (bool)gconf.Retrieve(AllowLegacyConnectionsKey);
			} catch(SettingNotFoundException){
				allowLegacyConnections = defaults.AllowLegacyConnections;
			}
			
			try{
				globalMaxConnections = (int)gconf.Retrieve(GlobalMaxConnectionsKey);
			} catch(SettingNotFoundException){
				globalMaxConnections = defaults.GlobalMaxConnections;
			}
			
			try{
				globalMaxDownloadSpeed = (int)gconf.Retrieve(GlobalMaxDownloadSpeedKey);
			} catch(SettingNotFoundException){
				globalMaxDownloadSpeed = defaults.GlobalMaxDownloadSpeed;
			}
			
			try{
				globalMaxHalfOpenConnections = (int)gconf.Retrieve(GlobalMaxHalfOpenConnectionsKey);
			} catch(SettingNotFoundException){
				globalMaxHalfOpenConnections = defaults.GlobalMaxHalfOpenConnections;
			}
			
			try{
				globalMaxUploadSpeed = (int)gconf.Retrieve(GlobalMaxUploadSpeedKey);
			} catch(SettingNotFoundException){
				globalMaxUploadSpeed = defaults.GlobalMaxUploadSpeed;
			}
			
			try{
				listenPort = (int)gconf.Retrieve(ListenPortKey);
			} catch(SettingNotFoundException){
				listenPort = defaults.ListenPort;
			}
			
			try{
				maxReadRate = (int)gconf.Retrieve(MaxReadRateKey);
			} catch(SettingNotFoundException){
				maxReadRate = defaults.MaxReadRate;
			}
			
			try{
				maxWriteRate = (int)gconf.Retrieve(MaxWriteRateKey);
			} catch(SettingNotFoundException){
				maxWriteRate = defaults.MaxWriteRate;
			}
			
			try{ 
				savePath = (string)gconf.Retrieve(SavePathKey);
			} catch(SettingNotFoundException){
				savePath = defaults.SavePath;
			} finally{
				// If savePath has not been set, fallback to user's home directory
				if (String.IsNullOrEmpty(savePath)) {
					savePath = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
				}
			}
			
		}
		
		public void Store()
		{	
			GconfSettingsStorage gconf = new GconfSettingsStorage();
			
			gconf.Store(AllowLegacyConnectionsKey, allowLegacyConnections);
			gconf.Store(GlobalMaxConnectionsKey, globalMaxConnections);
			gconf.Store(GlobalMaxDownloadSpeedKey, globalMaxDownloadSpeed);
			gconf.Store(GlobalMaxHalfOpenConnectionsKey, globalMaxHalfOpenConnections);
			gconf.Store(GlobalMaxUploadSpeedKey, globalMaxUploadSpeed);
			gconf.Store(ListenPortKey, listenPort);
			gconf.Store(SavePathKey, savePath);
			gconf.Store(MaxReadRateKey, maxReadRate);
			gconf.Store(MaxWriteRateKey, maxWriteRate);
		}
		
		public bool AllowLegacyConnections
		{
			get { return allowLegacyConnections; }
			set { allowLegacyConnections = value; }
		}
		
		public int GlobalMaxConnections
		{
			get { return globalMaxConnections; }
			set { globalMaxConnections = value; }
		}
		
		public int GlobalMaxDownloadSpeed
		{
			get { return globalMaxDownloadSpeed; }
			set { globalMaxDownloadSpeed = value; }
		}
		
		public int GlobalMaxHalfOpenConnections
		{
			get { return globalMaxHalfOpenConnections; }
			set { globalMaxHalfOpenConnections = value; }
		}
		
		public int GlobalMaxUploadSpeed
		{
			get { return globalMaxUploadSpeed; }
			set { globalMaxUploadSpeed = value; }
		}
		
		public int ListenPort
		{
			get { return listenPort; }
			set { listenPort = value; }
		}
		
		public string SavePath
		{
			get { return savePath; }
			set { savePath = value; }
		}
		
		public int MaxReadRate
		{
			get { return maxReadRate; }
			set { maxReadRate = value; }
		}
		
		public int MaxWriteRate
		{
			get { return maxWriteRate; }
			set { maxWriteRate = value; }
		}
	}
}
