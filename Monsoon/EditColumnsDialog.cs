//
// EditColumnsDialog.cs
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
using Gtk;

namespace Monsoon
{
	
	
	public partial class EditColumnsDialog : Gtk.Dialog
	{
		public EditColumnsDialog(Gtk.TreeViewColumn[] columns)
		{
			this.Build();
			
			Title = _("Edit columns");
			Modal = true;
			table.Homogeneous = true;

			Array.Sort <TreeViewColumn> (columns, delegate (TreeViewColumn left, TreeViewColumn right) {
				return left.Title.CompareTo (right.Title);
			});
			for (uint i = 0 ; i < columns.Length; i++) {
				TorrentTreeView.Column c = (TorrentTreeView.Column) columns [i];
				if (c.Ignore)
					continue;
				
				CheckButton check = new CheckButton { Label = c.Title, Active = c.Visible  };
				check.Clicked += delegate {
					Console.WriteLine ("Setting {0} to {1} with width {2}/", c.Title, check.Active, c.Width, c.FixedWidth);
					c.Visible = check.Active;
					c.FixedWidth = Math.Max (c.FixedWidth, 10);
				};
				Console.WriteLine ("Appending one");
				this.table.Attach (check, i % 2, i % 2 + 1, i / 2, i / 2 + 1);
			}
			ShowAll ();
		}

		private static string _(string s)
		{
			return Mono.Unix.Catalog.GetString(s);
		}
	}
}
