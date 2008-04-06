// XmlTorrentStorageController.cs created with MonoDevelop
// User: buchan at 03:20 04/04/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
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
