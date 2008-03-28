// project created on 05/21/2007 at 02:42
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

using Gtk;

using NLog;
using NLog.Targets; 
using NLog.Config; 

namespace Monsoon
{
	class MainClass
	{
		public static string BaseDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
		
		// SetProcessName code from http://abock.org/2006/02/09/changing-process-name-in-mono/
		[DllImport("libc")]
		private static extern int prctl(int option, byte [] arg2, ulong arg3, ulong arg4, ulong arg5);

		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger(); 		
		private ListenPortController portController;
		private UserEngineSettings userEngineSettings;
		private MainWindow mainWindow;
		private bool isFirstRun;
		
		public static void Main (string[] args)
		{
			new MainClass(args);
		}

		public MainClass(string [] args)
		{
			isFirstRun = false;
			CheckDataFolders();
			BuildNlogConfig();
			logger.Info("Starting Monsoon");
					
			SetProcessName("monsoon");
		
			userEngineSettings = new UserEngineSettings();
			portController = new ListenPortController(userEngineSettings);
			
			Application.Init ();
			mainWindow = new MainWindow (userEngineSettings, portController, isFirstRun);
			
			// This is so we can use IconEntry button
			// Use Gnome.Program instead of Gtk.Application?
			//Gnome.Program program = 
			new Gnome.Program("monsoon", "0.1", Gnome.Modules.UI, args);
			
			try{
				Application.Run();
			} catch(Exception e){
				Console.Out.WriteLine(e.ToString());
				Application.Init();
				UnhandledExceptionDialog exDialog = new UnhandledExceptionDialog(e);
				exDialog.Run();
				mainWindow.Stop();
				exDialog.Destroy();
			}
			portController.Stop();
			mainWindow.Stop ();
			mainWindow.Destroy ();
		}


		public static void SetProcessName(string name)
		{
			if(prctl(15 /* PR_SET_NAME */, Encoding.ASCII.GetBytes(name + "\0"), 0, 0, 0) != 0) {
				throw new ApplicationException("Error setting process name: " +	Mono.Unix.Native.Stdlib.GetLastError());
			}
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
	}
}