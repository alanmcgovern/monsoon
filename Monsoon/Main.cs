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

namespace Monsoon
{
	class MainClass
	{
		// SetProcessName code from http://abock.org/2006/02/09/changing-process-name-in-mono/
		[DllImport("libc")]
		private static extern int prctl(int option, byte [] arg2, ulong arg3, ulong arg4, ulong arg5);

		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger(); 		
		private ListenPortController portController;
		private GconfSettingsStorage settingsStorage;
		private SettingsController<EngineSettings> engineSettings;
		private MainWindow mainWindow;
		private bool isFirstRun;
		
		public static void Main (string[] args)
		{
			new MainClass(args);
		}

		public MainClass(string [] args)
		{
			// required for the MS .NET runtime that doesn't initialize glib automatically
			if (!GLib.Thread.Supported) {
				GLib.Thread.Init();
			}
			
			isFirstRun = false;
			CheckDataFolders();
			
			bool debug = false;
			foreach (string arg in args) {
				Console.WriteLine(arg);
				if (arg == "-d" || arg == "--debug") {
					debug = true;
				}
				else if (File.Exists(arg)) {
					GLib.Timeout.Add (1000, delegate {
						try
						{
							MonoTorrent.Common.Torrent t;
							if (!MonoTorrent.Common.Torrent.TryLoad (arg, out t))
								return false;
							
							mainWindow.TorrentController.addTorrent(t);
						}
						catch (Exception ex)
						{
							logger.Error("Couldn't load torrent: {0}", arg);
						}
						return false;
					});
				}
			}
			
			if (debug) {
				BuildNlogConfig();
			}
			
			logger.Info("Starting Monsoon");
					
			SetProcessName("monsoon");
		
			engineSettings = new GconfEngineSettingsController ();
			try {
				engineSettings.Load();
			}
			catch (Exception ex) {
				logger.Error("Could not load engine settings: {0}", ex.Message);
			}
			settingsStorage = GconfSettingsStorage.Instance;
			portController = new ListenPortController(engineSettings.Settings);

			string localeDir = Path.Combine(Defines.ApplicationDirectory, "locale");
			if (!Directory.Exists(localeDir)) {
				localeDir = Path.Combine(Defines.InstallPrefix, "share");
				localeDir = Path.Combine(localeDir, "locale");
			}
			Mono.Unix.Catalog.Init("monsoon", localeDir);
			logger.Debug("Using locale data from: {0}", localeDir);
			Console.WriteLine(_("Starting Monsoon"));
			
			Application.Init("monsoon", ref args);
			
			mainWindow = new MainWindow (settingsStorage, engineSettings.Settings,
									portController, isFirstRun);
			
			GLib.ExceptionManager.UnhandledException += new GLib.UnhandledExceptionHandler(OnUnhandledException);
			
			Application.Run();

			try {
				engineSettings.Save ();
			}
			catch (Exception ex) {
				logger.Error("Could save engine settings: {0}", ex.Message);
			}
			portController.Stop();
			mainWindow.Stop ();
			mainWindow.Destroy ();
		}

		public static void SetProcessName(string name)
		{
			if (prctl(15 /* PR_SET_NAME */, Encoding.ASCII.GetBytes(name + "\0"), 0, 0, 0) != 0) {
				throw new ApplicationException(_("Error setting process name: " +	Mono.Unix.Native.Stdlib.GetLastError()));
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
				isFirstRun = true;
				//logger.Info("Config folder does not exist, creating now");
				Directory.CreateDirectory(Defines.ConfigDirectory);
			}
			
			if (!Directory.Exists(Defines.TorrentFolder)){
				//logger.Info("Default torrent folder does not exist, creating now");
				Directory.CreateDirectory(Defines.TorrentFolder);
			}
			
		}
		
		private static string _(string s)
		{
			return Mono.Unix.Catalog.GetString(s);
		}
	}
}
