// Defines.cs created with MonoDevelop
// User: buchan at 01:23 03/28/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;

namespace Monsoon
{
	public class Defines
	{
		private static string BaseDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
		private static string AppSettingsPath = Path.Combine(Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "monsoon");
		
		public static string ApplicationName
		{
			get { return "Monsoon"; }
		}
		
		public static string LogFile
		{
			get { return Path.Combine(AppSettingsPath, "monsoon.log"); }
		}
		
		public static string ConfigDirectory
		{
			get { return AppSettingsPath; }
		}
		
		public static string IconPath
		{
			get { return Path.Combine(BaseDirectory, "icons"); }
		}
		
		public static string SerializedTorrentSettings
		{
			get { return Path.Combine (AppSettingsPath, "torrents.xml"); }
		}
		
		public static string SerializedLabels
		{
			get { return Path.Combine(AppSettingsPath, "labels.xml"); }
		}
		
		public static string SerializedRssFeeds
		{
			get { return Path.Combine (AppSettingsPath, "rssfeeds.xml"); }
		}
		
		public static string SerializedRssHistroy
		{
			get { return Path.Combine (AppSettingsPath, "rsshistory.xml"); }
		}
		
		public static string SerializedRssFilters
		{
			get { return Path.Combine (AppSettingsPath, "rssfilters.xml"); }
		}
		
		public static string TorrentFolder
		{
			get { return Path.Combine(AppSettingsPath, "torrents"); }
		}
	}
}
