//
// GconfInterfaceSettingsController.cs
//
// Author:
//   Alan McGovern (alan.mcgovern@gmail.com)
//
// Copyright (C) 2008 Alan McGovern
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
using System.Xml.Serialization;

namespace Monsoon
{
	public class GConfInterfaceSettingsController : GConfSettings <InterfaceSettings>
	{
		private static string SETTINGS_PATH = "InterfaceSettings/";

		public override void Load ()
		{
				Settings.ShowDetails = Get <bool> (SETTINGS_PATH + "showDetails");
				Settings.ShowLabels = Get <bool> (SETTINGS_PATH + "showLabels");
			
				Settings.WindowHeight = Get <int> (SETTINGS_PATH + "windowHeight");
				Settings.WindowWidth = Get <int> (SETTINGS_PATH + "windowWidth");
				Settings.VPaned = Get <int> (SETTINGS_PATH + "vPaned");
				Settings.HPaned = Get <int> (SETTINGS_PATH + "hPaned");
				Settings.WindowYPos = Get <int> (SETTINGS_PATH + "windowYPos");
				Settings.WindowXPos = Get <int> (SETTINGS_PATH + "windowXPos");
			
				Settings.NameColumnWidth = Get <int> (SETTINGS_PATH + "Columns/Name/Width");
				Settings.NameColumnVisible = Get <bool> (SETTINGS_PATH + "Columns/Name/Visible");
				Settings.StatusColumnWidth = Get <int> (SETTINGS_PATH + "Columns/Status/Width");
				Settings.StatusColumnVisible = Get <bool> (SETTINGS_PATH + "Columns/Status/Visible");
				Settings.DoneColumnWidth = Get <int> (SETTINGS_PATH + "Columns/Done/Width");
				Settings.DoneColumnVisible = Get <bool> (SETTINGS_PATH + "Columns/Done/Visible");
				Settings.SeedsColumnWidth = Get <int> (SETTINGS_PATH + "Columns/Seeds/Width");
				Settings.SeedsColumnVisible = Get <bool> (SETTINGS_PATH + "Columns/Seeds/Visible");
				Settings.PeersColumnWidth = Get <int> (SETTINGS_PATH + "Columns/Peers/Width");
				Settings.PeersColumnVisible = Get <bool> (SETTINGS_PATH + "Columns/Peers/Visible");
				Settings.DlSpeedColumnWidth = Get <int> (SETTINGS_PATH + "Columns/DLSpeed/Width");
				Settings.DlSpeedColumnVisible = Get <bool> (SETTINGS_PATH + "Columns/DLSpeed/Visible");
				Settings.UpSpeedColumnWidth = Get <int> (SETTINGS_PATH + "Columns/UPSpeed/Width");
				Settings.UpSpeedColumnVisible = Get <bool> (SETTINGS_PATH + "Columns/UPSpeed/Visible");
				Settings.RatioColumnWidth = Get <int> (SETTINGS_PATH + "Columns/Ratio/Width");
				Settings.RatioColumnVisible = Get <bool> (SETTINGS_PATH + "Columns/Ratio/Visible");
				Settings.SizeColumnWidth = Get <int> (SETTINGS_PATH + "Columns/Size/Width");
				Settings.SizeColumnVisible = Get <bool> (SETTINGS_PATH + "Columns/Size/Visible");
				Settings.ShowLoadDialog = Get <bool>  (SETTINGS_PATH + "ShowLoadDialog");
				Settings.EtaColumnWidth = Get <int> (SETTINGS_PATH + "Columns/Eta/Width");
				Settings.EtaColumnVisible = Get <bool> (SETTINGS_PATH + "Columns/Eta/Visible");
		}

		public override void Save ()
		{
			Set (SETTINGS_PATH + "showDetails", Settings.ShowDetails);
			Set (SETTINGS_PATH + "showLabels", Settings.ShowLabels);
			
			Set (SETTINGS_PATH + "windowHeight", Settings.WindowHeight);
			Set (SETTINGS_PATH + "windowWidth", Settings.WindowWidth);
			Set (SETTINGS_PATH + "vPaned", Settings.VPaned);
			Set (SETTINGS_PATH + "hPaned", Settings.HPaned);
			Set (SETTINGS_PATH + "windowXPos", Settings.WindowXPos);
			Set (SETTINGS_PATH + "windowYPos", Settings.WindowYPos);
			
			// Columns
			Set (SETTINGS_PATH + "Columns/Name/Width", Settings.NameColumnWidth);
			Set (SETTINGS_PATH + "Columns/Name/Visible", Settings.NameColumnVisible);
			Set (SETTINGS_PATH + "Columns/Status/Width", Settings.StatusColumnWidth);
			Set (SETTINGS_PATH + "Columns/Status/Visible", Settings.StatusColumnVisible);
			Set (SETTINGS_PATH + "Columns/Done/Width", Settings.DoneColumnWidth);
			Set (SETTINGS_PATH + "Columns/Done/Visible", Settings.DoneColumnVisible);
			Set (SETTINGS_PATH + "Columns/Seeds/Width", Settings.SeedsColumnWidth);
			Set (SETTINGS_PATH + "Columns/Seeds/Visible", Settings.SeedsColumnVisible);
			Set (SETTINGS_PATH + "Columns/Peers/Width", Settings.PeersColumnWidth);
			Set (SETTINGS_PATH + "Columns/Peers/Visible", Settings.PeersColumnVisible);
			Set (SETTINGS_PATH + "Columns/DLSpeed/Width", Settings.DlSpeedColumnWidth);
			Set (SETTINGS_PATH + "Columns/DLSpeed/Visible", Settings.DlSpeedColumnVisible);
			Set (SETTINGS_PATH + "Columns/UPSpeed/Width", Settings.UpSpeedColumnWidth);
			Set (SETTINGS_PATH + "Columns/UPSpeed/Visible", Settings.UpSpeedColumnVisible);
			Set (SETTINGS_PATH + "Columns/Ratio/Width", Settings.RatioColumnWidth);
			Set (SETTINGS_PATH + "Columns/Ratio/Visible", Settings.RatioColumnVisible);
			Set (SETTINGS_PATH + "Columns/Size/Width", Settings.SizeColumnWidth);
			Set (SETTINGS_PATH + "Columns/Size/Visible", Settings.SizeColumnVisible);
			Set (SETTINGS_PATH + "Columns/Eta/Width", Settings.EtaColumnWidth);
			Set (SETTINGS_PATH + "Columns/Eta/Visible", Settings.EtaColumnVisible);
			
			Set (SETTINGS_PATH + "ShowLoadDialog", Settings.ShowLoadDialog);
		}
	}
}
