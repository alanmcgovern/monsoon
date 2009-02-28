//
// GconfTorrentSettingsController.cs
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
using MonoTorrent.Client;

namespace Monsoon
{
	public class GconfTorrentSettingsController : GConfSettings<TorrentSettings>
	{
		static readonly string SETTINGS_PATH = "TorrentSettings/";
	 	
		static readonly string InitialSeedingKey = SETTINGS_PATH + "InitialSeeding";
		static readonly string UploadSlotsKey = SETTINGS_PATH + "UploadSlots";
		static readonly string MaxConnectionsKey = SETTINGS_PATH + "MaxConnections";
		static readonly string MaxDownloadSpeedKey = SETTINGS_PATH + "MaxDownloadSpeed";
		static readonly string MaxUploadSpeedKey = SETTINGS_PATH + "MaxUploadSpeed";
		static readonly string FastResumeEnabledKey = SETTINGS_PATH + "FastResumeEnabled";

		public override void Load ()
		{
			Settings.InitialSeedingEnabled = Get <bool> (InitialSeedingKey);
			Settings.UploadSlots = Get <int> (UploadSlotsKey);
			Settings.MaxConnections = Get <int> (MaxConnectionsKey);
			Settings.MaxDownloadSpeed = Get <int> (MaxDownloadSpeedKey);
			Settings.MaxUploadSpeed = Get <int> (MaxUploadSpeedKey);
		}
		
		public override void Save ()
		{
			GconfSettingsStorage gconf = GconfSettingsStorage.Instance;
			
			gconf.Store(InitialSeedingKey, Settings.InitialSeedingEnabled);
			gconf.Store(UploadSlotsKey, Settings.UploadSlots);
			gconf.Store(MaxConnectionsKey, Settings.MaxConnections);
			gconf.Store(MaxDownloadSpeedKey, Settings.MaxDownloadSpeed);
			gconf.Store(MaxUploadSpeedKey, Settings.MaxUploadSpeed);
		}
	}
}
