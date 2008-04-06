//
// XmlRssFiltersController.cs
//
// Author:
//   Jared Hendry (hendry.jared@gmail.com)
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
	
	
	public class XmlRssFiltersController : SettingsController<List<RssFilter>>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		public override void Load ()
		{
			RssFilter [] filtersToRestore = null;
			XmlSerializer xs = new XmlSerializer (typeof(RssFilter[]));
			
			logger.Info ("Restoring RSS feeds");
			
			if (System.IO.File.Exists(Defines.SerializedRssFilters)) {
				FileStream fs = null;
				
				try {
					fs = System.IO.File.Open(Defines.SerializedRssFilters, System.IO.FileMode.Open);
				} catch {
					logger.Error("Error opening rssfilters.xml");
				}
				
				try {				
					filtersToRestore = (RssFilter[]) xs.Deserialize(fs);
				} catch {
					logger.Error("Failed to restore RSS filters");
					return;
				} finally {				
					fs.Close();
				}
				
				foreach(RssFilter filter in filtersToRestore){
					Console.Out.WriteLine("Filter:" + filter.Name);
					Settings.Add(filter);
				}
			}
			
			
		}
		
		public override void Save ()
		{
			logger.Info ("Storing filters");

			using (Stream fs = new FileStream (Defines.SerializedRssFilters, FileMode.Create))
			{
				XmlWriter writer = new XmlTextWriter (fs, Encoding.UTF8);

				XmlSerializer s = new XmlSerializer (typeof(RssFilter[]));
				s.Serialize(writer, Settings.ToArray ());
			}
		}


	}
}
