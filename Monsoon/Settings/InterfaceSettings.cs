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
		
		public int NameColumnWidth{
			get; set;
		}
		
		public bool NameColumnVisible{
			get; set;
		}
		
		public int StatusColumnWidth{
			get; set;
		}
		
		public bool StatusColumnVisible{
			get; set;
		}
		
		public int DoneColumnWidth{
			get; set;
		}
		
		public bool DoneColumnVisible{
			get; set;
		}

		public bool PriorityColumnVisible {
			get; set;
		}
		
		public int SeedsColumnWidth{
			get; set;
		}
		
		public bool SeedsColumnVisible{
			get; set;
		}
		
		public int PeersColumnWidth{
			get; set;
		}
		
		public bool PeersColumnVisible{
			get; set;
		}
		
		public int DlSpeedColumnWidth{
			get; set;
		}
		
		public bool DlSpeedColumnVisible{
			get; set;
		}
		
		public int UpSpeedColumnWidth{
			get; set;
		}
		
		public bool UpSpeedColumnVisible{
			get; set;
		}

		public int PriorityColumnWidth {
			get; set;
		}
		
		public int RatioColumnWidth{
			get; set;
		}
		
		public bool RatioColumnVisible{
			get; set;
		}
		
		public int SizeColumnWidth{
			get; set;
		}
		
		public bool SizeColumnVisible{
			get; set;
		}
		
		public int EtaColumnWidth{
			get; set;
		}
		
		public bool EtaColumnVisible{
			get; set;
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
			NameColumnWidth = 220;
			NameColumnVisible = true;
			StatusColumnWidth = 65;
			StatusColumnVisible = true;
			DoneColumnWidth = 75;
			DoneColumnVisible = true;
			SeedsColumnWidth = 52;
			SeedsColumnVisible = true;
			PeersColumnWidth = 48;
			PeersColumnVisible = true;
			DlSpeedColumnWidth = 75;
			DlSpeedColumnVisible = true;
			UpSpeedColumnWidth = 75;
			UpSpeedColumnVisible = true;
			RatioColumnWidth = 75;
			RatioColumnVisible = true;
			SizeColumnWidth = 75;
			SizeColumnVisible = true;
			ShowLoadDialog = true;
			EtaColumnWidth = 50;
			EtaColumnVisible = true;
			PriorityColumnVisible = true;
			PriorityColumnWidth = 57;
		}
	}
}
