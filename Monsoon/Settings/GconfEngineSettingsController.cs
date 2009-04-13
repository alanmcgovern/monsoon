//
// GconfEngineSettingsController.cs
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
using MonoTorrent.Client;
using MonoTorrent.Common;
using MonoTorrent.Client.Encryption;

namespace Monsoon
{
	
	public class GconfEngineSettingsController : GConfSettings <EngineSettings>
	{
		static string SETTINGS_PATH = "EngineSettings/";
		
		static readonly string AllowedEncryptionKey = SETTINGS_PATH + "AllowedEncryption";
		static readonly string GlobalMaxConnectionsKey = SETTINGS_PATH + "GlobalMaxConnections";
		static readonly string GlobalMaxDownloadSpeedKey = SETTINGS_PATH + "GlobalMaxDownloadSpeed";
		static readonly string GlobalMaxHalfOpenConnectionsKey = SETTINGS_PATH + "GlobalMaxHalfOpenConnections";
		static readonly string GlobalMaxUploadSpeedKey = SETTINGS_PATH + "GlobalMaxUploadSpeed";
		static readonly string HaveSuppressionKey = SETTINGS_PATH + "HaveSuppression";
		static readonly string ListenPortKey = SETTINGS_PATH + "ListenPort";
		static readonly string MaxOpenFilesKey = SETTINGS_PATH + "MaxOpenFiles";
		static readonly string SavePathKey = SETTINGS_PATH + "SavePath";
		static readonly string MaxReadRateKey = SETTINGS_PATH + "MaxReadRate";
		static readonly string MaxWriteRateKey = SETTINGS_PATH + "MaxWriteRate";
		static readonly string PreferEncryptionKey = SETTINGS_PATH + "PreferEncryption";

		public override void Load ()
		{
			Settings.AllowedEncryption = (EncryptionTypes) Get <int> (AllowedEncryptionKey, (int)Settings.AllowedEncryption);
			Settings.GlobalMaxConnections = Get <int> (GlobalMaxConnectionsKey, Settings.GlobalMaxConnections);
			Settings.GlobalMaxDownloadSpeed = Get <int> (GlobalMaxDownloadSpeedKey, Settings.GlobalMaxDownloadSpeed);
			Settings.GlobalMaxHalfOpenConnections = Get <int> (GlobalMaxHalfOpenConnectionsKey, Settings.GlobalMaxHalfOpenConnections);
			Settings.MaxOpenFiles = Get <int> (MaxOpenFilesKey, Settings.MaxOpenFiles);
			Settings.GlobalMaxUploadSpeed = Get <int> (GlobalMaxUploadSpeedKey, Settings.GlobalMaxUploadSpeed);
			Settings.HaveSupressionEnabled = Get <bool> (HaveSuppressionKey, Settings.HaveSupressionEnabled);
			Settings.ListenPort = Get <int> (ListenPortKey, Settings.ListenPort);
			Settings.MaxReadRate = Get <int> (MaxReadRateKey, Settings.MaxReadRate);
			Settings.MaxWriteRate = Get <int> (MaxWriteRateKey, Settings.MaxWriteRate);
			Settings.PreferEncryption = Get <bool> (PreferEncryptionKey, Settings.PreferEncryption);
			Settings.SavePath = Get <string> (SavePathKey, Settings.SavePath);
			
			// Try to get XDG_DOWNLOAD_DIR path, if unavailible fallback to
			// users home directory
			if (String.IsNullOrEmpty(Settings.SavePath) || !Directory.Exists(Settings.SavePath))
				Settings.SavePath = GetDownloadDirectory();
		}
		
		public override void Save ()
		{
			GconfSettingsStorage gconf = GconfSettingsStorage.Instance;
			
			Set (AllowedEncryptionKey, (int) Settings.AllowedEncryption);
			Set (GlobalMaxConnectionsKey, Settings.GlobalMaxConnections);
			Set (GlobalMaxDownloadSpeedKey, Settings.GlobalMaxDownloadSpeed);
			Set (GlobalMaxHalfOpenConnectionsKey, Settings.GlobalMaxHalfOpenConnections);
			Set (GlobalMaxUploadSpeedKey, Settings.GlobalMaxUploadSpeed);
			Set (HaveSuppressionKey, Settings.HaveSupressionEnabled);
			Set (ListenPortKey, Settings.ListenPort);
			Set (SavePathKey, Settings.SavePath);
			Set (MaxOpenFilesKey, Settings.MaxOpenFiles);
			Set (MaxReadRateKey, Settings.MaxReadRate);
			Set (MaxWriteRateKey, Settings.MaxWriteRate);
			Set (PreferEncryptionKey, Settings.PreferEncryption);
		}
		
		private string GetDownloadDirectory() {
			string dirspath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "user-dirs.dirs");

			if (!File.Exists(dirspath))
				return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			
			string xdgline = string.Empty;
			using (StreamReader sr = new StreamReader(dirspath))
				while((xdgline = sr.ReadLine()) != null && !xdgline.StartsWith("XDG_DOWNLOAD_DIR="));
			
			if(string.IsNullOrEmpty(xdgline))
				return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			
			string dlpath = xdgline.Substring(xdgline.IndexOf("\""));
			dlpath = dlpath.Replace("\"", string.Empty);
			if (dlpath.StartsWith("$HOME"))
				return dlpath.Replace("$HOME", Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			
			return Environment.GetFolderPath (Environment.SpecialFolder.Personal);
		}	
	}
}
