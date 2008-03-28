// Defines.cs created with MonoDevelop
// User: buchan at 01:23 03/28/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Monsoon
{
	public class Defines
	{
		public static string ApplicationName
		{
			get { return "Monsoon"; }
		}
		
		public static string LogFile
		{
			get { return System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "monsoon"), "monsoon.log"); }
		}
		
		public static string ConfigDirectory
		{
			get { return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"monsoon"); }
		}
		
		public static string SerializedTorrentSettings
		{
			get { return System.IO.Path.Combine (System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "monsoon"), "torrents.xml"); }
		}
		
		public static string SerializedLabels
		{
			get { return System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "monsoon"), "labels.xml"); }
		}
		
		public static string SerializedRssFeeds
		{
			get { return System.IO.Path.Combine (System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "monsoon"), "rssfeeds.xml"); }
		}
		
		public static string SerializedRssHistroy
		{
			get { return System.IO.Path.Combine (System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "monsoon"), "rsshistory.xml"); }
		}
		
		public static string SerializedRssFilters
		{
			get { return System.IO.Path.Combine (System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "monsoon"), "rssfilters.xml"); }
		}
		
		public static string TorrentFolder
		{
			get { return System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "monsoon"), "torrents"); }
		}
	}
}
