//
// XmlTorrentStorageController.cs
//
// Author:
//   Jared Hendry (buchan@gmail.com)
//
// Copyright (C) 2008 Jared Hendry
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
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;

namespace Monsoon
{
	
	
	public class XmlTorrentStorageController : SettingsController<List<TorrentStorage>>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		public override void Load ()
		{
			TorrentStorage[] storedTorrents;

			if(!File.Exists(Defines.SerializedTorrentSettings))
			   return;
			
			try {
				using (FileStream fs = File.Open(Defines.SerializedTorrentSettings, FileMode.Open)) {
					XmlSerializer xs = new XmlSerializer(typeof(TorrentStorage[]));				
					storedTorrents = (TorrentStorage[]) xs.Deserialize(fs);
				}
			} catch (XmlException) {
				logger.Error("Error loading stored torrents");
				return;
			}
			
			Settings.Clear();
			foreach(TorrentStorage torrentStorage in storedTorrents)
				Settings.Add(torrentStorage);
		}
		
		public override void Save ()
		{
			using (Stream fs = new FileStream (Defines.SerializedTorrentSettings, FileMode.Create)) {
				XmlWriter writer = new XmlTextWriter (fs, Encoding.UTF8);				
				XmlSerializer s = new XmlSerializer (typeof(TorrentStorage[]));
				s.Serialize (writer, Settings.ToArray()); 	
			}
		}

	}
}
