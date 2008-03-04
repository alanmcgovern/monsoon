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
		
		private bool allowLegacyConnections;
		private int globalMaxConnections;
		private int globalMaxDownloadSpeed;
		private int globalMaxHalfOpenConnections;
		private int globalMaxUploadSpeed;
		private int listenPort;
		//private EncryptionType minEncryptionLevel;
		private string savePath;
		
		public UserEngineSettings()
		{
			Restore();				
		}
		
		public void Restore()
		{
			GconfSettingsStorage gconf = new GconfSettingsStorage();
			EngineSettings defaults = new EngineSettings();
			try{
				allowLegacyConnections = (bool)gconf.Retrieve(SETTINGS_PATH + "AllowLegacyConnections");
			} catch(SettingNotFoundException){
				allowLegacyConnections = defaults.AllowLegacyConnections;
			}
			
			try{
				globalMaxConnections = (int)gconf.Retrieve(SETTINGS_PATH + "GlobalMaxConnections");
			} catch(SettingNotFoundException){
				globalMaxConnections = defaults.GlobalMaxConnections;
			}
			
			try{
				globalMaxDownloadSpeed = (int)gconf.Retrieve(SETTINGS_PATH + "GlobalMaxDownloadSpeed");
			} catch(SettingNotFoundException){
				globalMaxDownloadSpeed = defaults.GlobalMaxDownloadSpeed;
			}
			
			try{
				globalMaxHalfOpenConnections = (int)gconf.Retrieve(SETTINGS_PATH + "GlobalMaxHalfOpenConnections");
			} catch(SettingNotFoundException){
				globalMaxHalfOpenConnections = defaults.GlobalMaxHalfOpenConnections;
			}
			
			try{
				globalMaxUploadSpeed = (int)gconf.Retrieve(SETTINGS_PATH + "GlobalMaxUploadSpeed");
			} catch(SettingNotFoundException){
				globalMaxUploadSpeed = defaults.GlobalMaxUploadSpeed;
			}
			
			try{
				listenPort = (int)gconf.Retrieve(SETTINGS_PATH + "ListenPort");
			} catch(SettingNotFoundException){
				listenPort = defaults.ListenPort;
			}
			
			/*
			try{
				minEncryptionLevel = (EncryptionType)gconf.Retrieve(SETTINGS_PATH + "MinEncryptionLevel");
			} catch(SettingNotFoundException){
				minEncryptionLevel = EngineSettings.DefaultSettings().MinEncryptionLevel;
			}
			*/
			
			try{ 
				savePath = (string)gconf.Retrieve(SETTINGS_PATH + "SavePath");
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
			
			gconf.Store(SETTINGS_PATH + "AllowLegacyConnections", allowLegacyConnections);
			gconf.Store(SETTINGS_PATH + "GlobalMaxConnections", globalMaxConnections);
			gconf.Store(SETTINGS_PATH + "GlobalMaxDownloadSpeed", globalMaxDownloadSpeed);
			gconf.Store(SETTINGS_PATH + "GlobalMaxHalfOpenConnections", globalMaxHalfOpenConnections);
			gconf.Store(SETTINGS_PATH + "GlobalMaxUploadSpeed", globalMaxUploadSpeed);
			gconf.Store(SETTINGS_PATH + "ListenPort", listenPort);
			//gconf.Store(SETTINGS_PATH + "MinEncryptionLevel", minEncryptionLevel);
			gconf.Store(SETTINGS_PATH + "SavePath", savePath);
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
		
		/*
		public EncryptionType MinEncryptionLevel
		{
			get { return minEncryptionLevel; }
		}
		*/
		
		public string SavePath
		{
			get { return savePath; }
			set { savePath = value; }
		}
	}
}
