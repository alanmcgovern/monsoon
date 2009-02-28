
using System;
using System.Collections.Generic;
using MonoTorrent.Client;

namespace Monsoon
{
	public static class SettingsManager
	{
		static Dictionary <Type, Type> controllers;
		
		static SettingsController <T> Get <T> (T settings)
			where T : new ()
		{
			Type type = controllers [typeof (T)];
			SettingsController <T> s = (SettingsController <T>) Activator.CreateInstance (type);
			s.Settings = settings;
			return s;
		}
		
		public static void Restore <T> (T settings)
			where T : new ()
		{
			Get <T> (settings).Load ();
		}
		
		public static void Store <T> (T settings)
			where T : new ()
		{
			Get <T> (settings).Save ();
		}

		static void Register <T, U> ()
			where U : SettingsController <T>, new ()
			where T : new ()
		{
			controllers.Add (typeof (T), typeof (U));
		}
		
		static SettingsManager ()
		{
			// On unix we use GConf to store all the relevant settings
			// On windows we'll store elsewhere
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				Register <EngineSettings, GconfEngineSettingsController> ();
				Register <InterfaceSettings, GConfInterfaceSettingsController> ();
				Register <PreferencesSettings, GconfPreferencesSettingsController> ();
				Register <TorrentSettings, GconfTorrentSettingsController> ();
			} else {
				
			}
		}
	}
}
