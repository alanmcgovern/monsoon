//
// TorrentFileSettingsController.cs
//
// Author:
//   Mirco Bauer (meebey@meebey.net)
//
// Copyright (C) 2008 Mirco Bauer
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
using System.Text;
using MonoTorrent.Common;

namespace Monsoon
{
	public class TorrentFileSettingsController
	{
		private GconfSettingsStorage storage;
		
		public TorrentFileSettingsController(GconfSettingsStorage storage)
		{
			if (storage == null) {
				throw new ArgumentNullException("storage");
			}
			
			this.storage = storage;
		}
		
		public TorrentFileSettingsModel GetFileSettings(byte[] infoHash, string path)
		{
			if (infoHash == null) {
				throw new ArgumentNullException("infoHash");
			}
			if (path == null) {
				throw new ArgumentNullException("path");
			}
			
			string hashHex = GetInfoHashAsHex(infoHash);
			string key = String.Format("TorrentSettings/{0}/{1}/", hashHex,
			                           EncodeKey(path));
			string priority;
			
			try {
				priority = (string) storage.Retrieve(key + "Priority");
			} catch (SettingNotFoundException) {
				priority = Priority.Normal.ToString();
			}
			
			TorrentFileSettingsModel settings = new TorrentFileSettingsModel();
			settings.TorrentInfoHash = hashHex;
			settings.Path = path;
			settings.Priority = (Priority) Enum.Parse(typeof(Priority), priority);
			return settings;
		}
		
		public void SetFileSettings(TorrentFileSettingsModel settings)
		{
			if (settings == null) {
				throw new ArgumentNullException("settings");
			}
			
			string key = String.Format("TorrentSettings/{0}/{1}/",
			                           settings.TorrentInfoHash,
			                           EncodeKey(settings.Path));
			storage.Store(key + "Path",     settings.Path);
			storage.Store(key + "Priority", settings.Priority.ToString());
		}
		
		private string GetInfoHashAsHex(byte[] infoHash)
		{
			if (infoHash == null) {
				throw new ArgumentNullException("infoHash");
			}
			
			StringBuilder hex = new StringBuilder(infoHash.Length);
			foreach (byte b in infoHash) {
				hex.Append(b.ToString("X"));
			}
			return hex.ToString();
		}
		
		private string EncodeKey(string key)
		{
			if (key == null) {
				throw new ArgumentNullException("key");
			}
			
			key = key.Replace(' ', '_');
			key = key.Replace('/', '_');
			key = key.Replace('(', '_');
			key = key.Replace(')', '_');
			key = key.Replace('\'', '_');
			key = key.Replace('.', '_');
			return key;
		}
	}
}
