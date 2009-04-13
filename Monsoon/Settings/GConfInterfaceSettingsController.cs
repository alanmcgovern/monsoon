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
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Monsoon
{
	public class GConfInterfaceSettingsController : GConfSettings <InterfaceSettings>
	{
		public static readonly string ToolbarStyleSystemKey = "/desktop/gnome/interface/toolbar_style";
		private static string SETTINGS_PATH = "InterfaceSettings/";
		static string ToolbarStyleKey = SETTINGS_PATH + "toolbarStyle";

		public override void Load ()
		{
			Settings.ShowDetails = Get <bool> (SETTINGS_PATH + "showDetails", Settings.ShowDetails);
			Settings.ShowLabels = Get <bool> (SETTINGS_PATH + "showLabels", Settings.ShowLabels);
			
			Settings.WindowHeight = Get <int> (SETTINGS_PATH + "windowHeight", Settings.WindowHeight);
			Settings.WindowWidth = Get <int> (SETTINGS_PATH + "windowWidth", Settings.WindowWidth);
			Settings.VPaned = Get <int> (SETTINGS_PATH + "vPaned", Settings.VPaned);
			Settings.HPaned = Get <int> (SETTINGS_PATH + "hPaned", Settings.HPaned);
			Settings.WindowYPos = Get <int> (SETTINGS_PATH + "windowYPos", Settings.WindowYPos);
			Settings.WindowXPos = Get <int> (SETTINGS_PATH + "windowXPos", Settings.WindowXPos);
			Settings.ShowLoadDialog = Get <bool>  (SETTINGS_PATH + "ShowLoadDialog", Settings.ShowLoadDialog);
			object o = null;
			try {
				o = Get <object> (ToolbarStyleKey, Settings.ToolbarStyle);
			} catch { }
			if (o == null || "null".Equals (o))
				Settings.ToolbarStyle = null;
			else
				Settings.ToolbarStyle = (Gtk.ToolbarStyle) Enum.Parse (typeof (Gtk.ToolbarStyle), o.ToString (), true);
			
			try {
				Settings.ToolbarStyleSystem = GetAbsolute<Gtk.ToolbarStyle> (ToolbarStyleSystemKey);
			} catch { }
			foreach (string column in new List <string> (Settings.ColumnWidth.Keys)) {
				Settings.ColumnWidth [column] = Get <int> (SETTINGS_PATH + string.Format ("Columns/{0}/Width", column), 75);
				Settings.ColumnVisibility [column] = Get <bool> (SETTINGS_PATH + string.Format ("Columns/{0}/Visible", column), true);
			}
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
			Set (SETTINGS_PATH + "ShowLoadDialog", Settings.ShowLoadDialog);
			if (Settings.ToolbarStyle.HasValue)
				Set (ToolbarStyleKey, Settings.ToolbarStyle.Value.ToString ());
			else
				Set (ToolbarStyleKey, "null");
			
			// Columns
			foreach (string column in Settings.ColumnVisibility.Keys) {
				Set (SETTINGS_PATH + string.Format ("Columns/{0}/Width", column), Settings.ColumnWidth [column]);
				Set (SETTINGS_PATH + string.Format ("Columns/{0}/Visible", column), Settings.ColumnVisibility [column]);
			}
		}
	}
}
