// DBusInstance.cs
//
// Copyright (c) 2008 Alan McGovern (alan.mcgovern@gmail.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using NDesk.DBus;
using org.freedesktop.DBus;

namespace Monsoon
{
	public static class DBusInstance
	{
		private const string CommandParserPath = "/org/monsoon/commandparser";
		private const string BusName = "org.monsoon.monsoon";
		
		private static ICommandParser commandParser;
		private static bool alreadyRunning;
		
		
		public static bool AlreadyRunning
		{
			get { return alreadyRunning; }
		}
		
		public static ICommandParser CommandParser
		{
			get { return commandParser; }
		}

		
		static DBusInstance ()
		{
			commandParser = new CommandParser ();
		}
		
		public static void Connect ()
		{
			try
			{
				Ticker.Tick ();
				BusG.Init ();
				alreadyRunning = Bus.Session.RequestName (BusName) != RequestNameReply.PrimaryOwner;
				
				if (alreadyRunning)
					commandParser = Bus.Session.GetObject<ICommandParser>(BusName, new ObjectPath (CommandParserPath));
				else
					Bus.Session.Register(new ObjectPath(CommandParserPath), commandParser);
			}
			catch (Exception)
			{
				Console.WriteLine ("**************************************");
				Console.WriteLine ("* DBus support could not be started. *");
				Console.WriteLine ("* Some functionality will be missing *");
				Console.WriteLine ("**************************************");
			}
			finally
			{
				Ticker.Tock ("DBus");
			}
		}
	}
}
