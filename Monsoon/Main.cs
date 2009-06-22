//
// Main.cs
//
// Author:
//   Jared Hendry (buchan@gmail.com)
//   Mirco Bauer  (meebey@meebey.net)
//
// Copyright (C) 2007 Jared Hendry
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
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

using Gtk;

using NLog;
using NLog.Targets; 
using NLog.Config; 
using MonoTorrent.Client;
using Mono.Addins;

namespace Monsoon
{
	public class EmptyLogger : NLog.Logger
	{
		public EmptyLogger ()
		{
			
		}
	}
	
	class MainClass
	{
		public static bool DebugEnabled;
		
		// SetProcessName code from http://abock.org/2006/02/09/changing-process-name-in-mono/
		[DllImport("libc")]
		private static extern int prctl(int option, byte [] arg2, ulong arg3, ulong arg4, ulong arg5);

		private static NLog.Logger logger = null;
		private MainWindow mainWindow;
		
		public static void Main (string[] args)
		{
			new MainClass(args);
		}

		public MainClass(string [] args)
		{
			Ticker.Tick();
			
			// required for the MS .NET runtime that doesn't initialize glib automatically
			if (!GLib.Thread.Supported)
				GLib.Thread.Init();
			
			// Connect to dbus
			DBusInstance DBusInstance = ServiceManager.Get <DBusInstance> ();
			DBusInstance.Initialise ();

			if (DBusInstance.AlreadyRunning)
			{
				Console.WriteLine("Already running");
				DBusInstance.CommandParser.ParseCommands (args);
				return;
			}
			
			DBusInstance.CommandParser.RunCommand += HandleCommand;
			
			Ticker.Tick();
			CheckDataFolders();
			Ticker.Tock ("Checking folders");

			foreach (string arg in args)
				HandleCommand (arg);
			
			Ticker.Tick ();
			if (DebugEnabled) {
				BuildNlogConfig();
			}
			logger = DebugEnabled ? NLog.LogManager.GetCurrentClassLogger () : new EmptyLogger ();
			Ticker.Tock("NLog");
			
			logger.Info("Starting Monsoon");
			
			Ticker.Tick ();
			SetProcessName("monsoon");
			Ticker.Tock("Setting process name");

			string localeDir = Path.Combine(Defines.ApplicationDirectory, "locale");
			if (!Directory.Exists(localeDir)) {
				localeDir = Path.Combine(Defines.InstallPrefix, "share");
				localeDir = Path.Combine(localeDir, "locale");
			}
			
			Ticker.Tick ();
			Mono.Unix.Catalog.Init("monsoon", localeDir);
			logger.Debug("Using locale data from: {0}", localeDir);

			Application.Init("monsoon", ref args);
			Ticker.Tock("Locale");
			
			try {
				SettingsManager.Restore <EngineSettings> (SettingsManager.EngineSettings);
				SettingsManager.Restore <PreferencesSettings> (SettingsManager.Preferences);
				SettingsManager.Restore <TorrentSettings> (SettingsManager.DefaultTorrentSettings);
			}
			catch (Exception ex) {
				logger.Error("Couldn't restore old settings: {0}", ex.Message);
			}
			
			try
			{
				Ticker.Tick();
				mainWindow = new MainWindow ();
				Ticker.Tock ("Instantiating window");
				
			}
			catch(Exception e)
			{
				logger.ErrorException(e.Message, e);
				Environment.Exit (0);
			}

			LoadAddins ();
			
			GLib.ExceptionManager.UnhandledException += new GLib.UnhandledExceptionHandler(OnUnhandledException);
			
			Ticker.Tock ("Total time:");
			Application.Run();

			try {
				SettingsManager.Store <EngineSettings> (SettingsManager.EngineSettings);
				SettingsManager.Store <PreferencesSettings> (SettingsManager.Preferences);
				SettingsManager.Store <TorrentSettings> (SettingsManager.DefaultTorrentSettings);
			}
			catch (Exception ex) {
				logger.Error("Could save engine settings: {0}", ex.Message);
			}
			ServiceManager.Get <ListenPortController> ().Stop ();
			mainWindow.Destroy ();
		}
		
		private void HandleCommand (string command)
		{
			if (command == "-d" || command == "--debug")
			{
				DebugEnabled = true;
			}
			else if (File.Exists(command))
			{
				GLib.Timeout.Add (250, delegate {
					mainWindow.LoadTorrent (command);
					return false;
				});
			}
		}

		public static void SetProcessName(string name)
		{
			try {
				if (prctl(15 /* PR_SET_NAME */, Encoding.ASCII.GetBytes(name + "\0"), 0, 0, 0) != 0) {
					throw new ApplicationException(_("Error setting process name: " +	Mono.Unix.Native.Stdlib.GetLastError()));
				}
			} catch (Exception ex) {
				logger.ErrorException("Couldn't set process name, ignoring.", ex);
			}
		}
		
		private void OnUnhandledException(GLib.UnhandledExceptionArgs args)
		{
			UnhandledExceptionDialog exDialog = new UnhandledExceptionDialog((Exception)args.ExceptionObject);
			exDialog.Run();
			args.ExitApplication = true;
		}
		
		private void BuildNlogConfig()
		{
			LoggingConfiguration config = new LoggingConfiguration(); 
		 
			// Create targets 
		 	ConsoleTarget consoleTarget = new ConsoleTarget(); 
			config.AddTarget("console", consoleTarget); 
		 
			FileTarget fileTarget = new FileTarget(); 
			config.AddTarget("file", fileTarget); 
			
			//memoryTarget = new MemoryTarget();
			//config.AddTarget("memory", memoryTarget);
			
			// define layout
			consoleTarget.Layout = "${date:format=HH\\:MM\\:ss} ${level} ${logger} ${message}"; 
			fileTarget.FileName = Defines.LogFile; 
			fileTarget.Layout = "${level} ${stacktrace} ${message}"; 
			//memoryTarget.Layout = "${date:format=HH\\:MM\\:ss} ${level} ${logger} ${message}";
			
			// define rules 
			LoggingRule rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget); 
			config.LoggingRules.Add(rule1); 
			LoggingRule rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget); 
			config.LoggingRules.Add(rule2); 
			//LoggingRule rule3 = new LoggingRule("*", LogLevel.Debug, fileTarget);
			//config.LoggingRules.Add(rule3);
			LogManager.Configuration = config; 
		}
		
		private void CheckDataFolders()
		{
			//logger.Info("Check for directory... " + System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"monotorrent"));
			if (!Directory.Exists(Defines.ConfigDirectory)){
				//logger.Info("Config folder does not exist, creating now");
				Directory.CreateDirectory(Defines.ConfigDirectory);
			}
			
			if (!Directory.Exists(Defines.TorrentFolder)){
				//logger.Info("Default torrent folder does not exist, creating now");
				Directory.CreateDirectory(Defines.TorrentFolder);
			}
			
		}
		
		void DhtChanged (object sender, ExtensionNodeEventArgs e)
		{
			try {
				TorrentController controller = ServiceManager.Get <TorrentController> ();
				if (e.Change == ExtensionChange.Add) {
					TypeExtensionNode node = (TypeExtensionNode) e.ExtensionNode;
					IDhtExtension dht = (IDhtExtension) node.GetInstance ();
					if (dht.State == MonoTorrent.DhtState.NotReady)
						dht.Start ();
					
					controller.Engine.RegisterDht (dht);
					ToolItem w = dht.GetWidget ();
					if (w != null) {
						mainWindow.StatusToolbar.Insert (new SeparatorToolItem {
							Draw = false,
							WidthRequest = 10
						}, 0);
						mainWindow.StatusToolbar.Insert (w, 0);
						
						mainWindow.StatusToolbar.ShowAll ();
					}
					logger.Info ("DHT has been enabled");
				} else {
					logger.Warn ("DHT cannot be disabled on the fly");
				}
			} catch (Exception ex) {
				logger.Error ("Failed to enable DHT: {0}", ex.Message);
			}
		}
		
		void LoadAddins ()
		{
			try {
				Ticker.Tick ();
				
				// Initialise the addin manager and listen for DHT nodes to be attached
				AddinManager.Initialize (Defines.AddinPath);
				AddinManager.AddExtensionNodeHandler ("/monsoon/dht", DhtChanged);
				AddinManager.Registry.Update (null);
			} catch (Exception ex) {
				logger.Error ("Could not load extensions: {0}", ex.Message);
			} finally {
				Ticker.Tock ("Mono.Addins Initialised");
			}
		}

		private static string _(string s)
		{
			return Mono.Unix.Catalog.GetString(s);
		}
	}
}
