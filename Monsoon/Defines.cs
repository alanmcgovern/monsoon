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
		private static string applicationDirectory;
		private static string applicationDataDirectory;
		private static string version;
		
		public static string InstallPrefix {
			get {
				return "/usr/local";
			}
		}

		public static string AddinPath
		{
			get { return Path.Combine(ApplicationDataDirectory, "Addins"); }
		}
		
		public static string ApplicationName
		{
			get { return "Monsoon"; }
		}
		
		public static string LogFile
		{
			get { return Path.Combine(ApplicationDataDirectory, "monsoon.log"); }
		}
		
		public static string ConfigDirectory
		{
			get { return ApplicationDataDirectory; }
		}
		
		public static string IconPath
		{
			get { return Path.Combine(ApplicationDirectory, "icons"); }
		}
		
		public static string SerializedTorrentSettings
		{
			get { return Path.Combine (ApplicationDataDirectory, "torrents.xml"); }
		}
		
		public static string SerializedFastResume
		{
			get { return Path.Combine (ApplicationDataDirectory, "fastresume.benc"); }
		}
		
		public static string SerializedLabels
		{
			get { return Path.Combine(ApplicationDataDirectory, "labels.xml"); }
		}
		
		public static string SerializedRssFeeds
		{
			get { return Path.Combine (ApplicationDataDirectory, "rssfeeds.xml"); }
		}
		
		public static string SerializedRssHistroy
		{
			get { return Path.Combine (ApplicationDataDirectory, "rsshistory.xml"); }
		}
		
		public static string SerializedRssFilters
		{
			get { return Path.Combine (ApplicationDataDirectory, "rssfilters.xml"); }
		}
		
		public static string TorrentFolder
		{
			get { return Path.Combine(ApplicationDataDirectory, "torrents"); }
		}

		public static string ApplicationDirectory {
			get {
				return applicationDirectory;
			}
		}

		public static string ApplicationDataDirectory {
			get {
				return applicationDataDirectory;
			}
		}

		public static string Version {
			get {
				return version;
			}
		}
		
		static Defines()
		{
			applicationDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
			applicationDataDirectory = Path.Combine(Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "monsoon");
			
			Version ver = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
			version = String.Format("{0}.{1}", ver.Major, ver.Minor);
		}
	}
}
