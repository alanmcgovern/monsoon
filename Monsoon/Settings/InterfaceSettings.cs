//
// GconfTorrentSettingsController.cs
//
// Author:
//   Jared Hendry (buchan@gmail.com)
//
// Copyright (C) 2007 Jared Hendry
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

namespace Monsoon
{
	public class InterfaceSettings
	{
		public bool ShowLoadDialog {
			get; set;
		}
		
		public bool ShowDetails {
			get; set;
		}
		
		public bool ShowLabels {
			get; set;
		}
		
		public int WindowHeight {
			get; set;
		}
		
		public int WindowWidth {
			get; set;
		}
		
		public int VPaned {
			get; set;
		}
		
		public int HPaned {
			get; set;
		}
		
		public int WindowXPos{
			get; set;
		}
		
		public int WindowYPos{
			get; set;
		}
		
		public Dictionary <string, bool> ColumnVisibility {
			get; private set;
		}

		public Dictionary <string, int> ColumnWidth {
			get; private set;
		}
		
		public InterfaceSettings ()
		{
			ShowDetails = true;
			ShowLabels = true;
			WindowHeight = 480;
			WindowWidth = 805;
			VPaned = 185; 
			HPaned = 140;
			WindowYPos = 0;
			WindowXPos = 0;
			ShowLoadDialog = true;

			ColumnWidth = new Dictionary<string, int> ();

			ColumnWidth.Add ("name", 220);
			ColumnWidth.Add ("status", 65);
			ColumnWidth.Add ("done", 75);
			ColumnWidth.Add ("seeds", 52);
			ColumnWidth.Add ("peers", 48);
			ColumnWidth.Add ("priority", 57);
			ColumnWidth.Add ("downspeed", 75);
			ColumnWidth.Add ("upspeed", 75);
			ColumnWidth.Add ("ratio", 75);
			ColumnWidth.Add ("size", 75);
			ColumnWidth.Add ("eta", 50);

			ColumnVisibility = new Dictionary <string, bool> ();
			foreach (string s in ColumnWidth.Keys)
				ColumnVisibility.Add (s, true);
		}
	}
}
