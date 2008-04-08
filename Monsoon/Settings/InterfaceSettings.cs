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
		private int windowHeight;
		private int windowWidth;
		private int windowYPos;
		private int windowXPos;
		private int vPaned;
		private int hPaned;
		private bool showDetails;
		private bool showLabels;
		
		// Columns
		private int nameColumnWidth;
		private int statusColumnWidth;
		private int doneColumnWidth;
		private int seedsColumnWidth;
		private int peersColumnWidth;
		private int dlSpeedColumnWidth;
		private int upSpeedColumnWidth;
		private int ratioColumnWidth;
		private int sizeColumnWidth;
		
		private bool nameColumnVisible;
		private bool statusColumnVisible;
		private bool doneColumnVisible;
		private bool seedsColumnVisible;
		private bool peersColumnVisible;
		private bool dlSpeedColumnVisible;
		private bool upSpeedColumnVisible;
		private bool ratioColumnVisible;
		private bool sizeColumnVisible;

		public bool ShowDetails {
			get { return showDetails; }
			set { showDetails = value; }
		}
		
		public bool ShowLabels {
			get { return showLabels; }
			set { showLabels = value; }
		}
		
		public int WindowHeight {
			get { return windowHeight; }
			set { windowHeight = value; }
		}
		
		public int WindowWidth {
			get { return windowWidth; }
			set { windowWidth = value; }
		}
		
		public int VPaned {
			get { return vPaned; }
			set { vPaned = value; }
		}
		
		public int HPaned {
			get { return hPaned; }
			set { hPaned = value; }
		}
		
		public int WindowXPos{
			get { return windowXPos; }
			set { windowXPos = value; }
		}
		
		public int WindowYPos{
			get { return windowYPos; }
			set { windowYPos = value; }
		}
		
		public int NameColumnWidth{
			get { return nameColumnWidth; }
			set { nameColumnWidth = value; }
		}
		
		public bool NameColumnVisible{
			get { return nameColumnVisible; }
			set { nameColumnVisible = value; }
		}
		
		public int StatusColumnWidth{
			get { return statusColumnWidth; }
			set { statusColumnWidth = value; }
		}
		
		public bool StatusColumnVisible{
			get { return statusColumnVisible; }
			set { statusColumnVisible = value; }
		}
		
		public int DoneColumnWidth{
			get { return doneColumnWidth; }
			set { doneColumnWidth = value; }
		}
		
		public bool DoneColumnVisible{
			get { return doneColumnVisible; }
			set { doneColumnVisible = value; }
		}
		
		public int SeedsColumnWidth{
			get { return seedsColumnWidth; }
			set { seedsColumnWidth = value; }
		}
		
		public bool SeedsColumnVisible{
			get { return seedsColumnVisible; }
			set { seedsColumnVisible = value; }
		}
		
		public int PeersColumnWidth{
			get { return peersColumnWidth; }
			set { peersColumnWidth = value; }
		}
		
		public bool PeersColumnVisible{
			get { return peersColumnVisible; }
			set { peersColumnVisible = value; }
		}
		
		public int DlSpeedColumnWidth{
			get { return dlSpeedColumnWidth; }
			set { dlSpeedColumnWidth = value; }
		}
		
		public bool DlSpeedColumnVisible{
			get { return dlSpeedColumnVisible; }
			set { dlSpeedColumnVisible = value; }
		}
		
		public int UpSpeedColumnWidth{
			get { return upSpeedColumnWidth; }
			set { upSpeedColumnWidth = value; }
		}
		
		public bool UpSpeedColumnVisible{
			get { return upSpeedColumnVisible; }
			set { upSpeedColumnVisible = value; }
		}
		
		public int RatioColumnWidth{
			get { return ratioColumnWidth; }
			set { ratioColumnWidth = value; }
		}
		
		public bool RatioColumnVisible{
			get { return ratioColumnVisible; }
			set { ratioColumnVisible = value; }
		}
		
		public int SizeColumnWidth{
			get { return sizeColumnWidth; }
			set { sizeColumnWidth = value; }
		}
		
		public bool SizeColumnVisible{
			get { return sizeColumnVisible; }
			set { sizeColumnVisible = value; }
		}
	}
}
