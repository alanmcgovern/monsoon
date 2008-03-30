//
// TorrentSettingsController.cs
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
	public class TorrentSettingsController
	{
		private GconfSettingsStorage storage;
		
		public TorrentSettingsController(GconfSettingsStorage storage)
		{
			if (storage == null) {
				throw new ArgumentNullException("storage");
			}
			
			this.storage = storage;
		}
		
		public TorrentSettingsModel GetTorrentSettings(byte[] infoHash)
		{
			if (infoHash == null) {
				throw new ArgumentNullException("infoHash");
			}
			
			string hashHex = GetInfoHashAsHex(infoHash);
			string key = String.Format("TorrentSettings/{0}/", hashHex);
			string lastState;
			
			try {
				lastState = (string) storage.Retrieve(key + "LastState");
			} catch (SettingNotFoundException) {
				lastState = TorrentState.Stopped.ToString();
			}
			
			TorrentSettingsModel settings = new TorrentSettingsModel();
			settings.InfoHash = hashHex;
			settings.LastState = (TorrentState) Enum.Parse(typeof(TorrentState), lastState);
			return settings;
		}
		
		public void SetTorrentSettings(TorrentSettingsModel settings)
		{
			if (settings == null) {
				throw new ArgumentNullException("settings");
			}
			
			string key = String.Format("TorrentSettings/{0}/", settings.InfoHash);
			storage.Store(key + "LastState", settings.LastState.ToString());
		}
		
		// TODO: implement and use me!
		public void RemoveTorrentSettings(TorrentSettingsModel settings)
		{
			if (settings == null) {
				throw new ArgumentNullException("settings");
			}
			
			throw new NotImplementedException();
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
	}
}
