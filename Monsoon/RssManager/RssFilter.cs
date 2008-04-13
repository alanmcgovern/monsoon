//
// RssFilter.cs
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
	
	
	public class RssFilter
	{
		private string name;
		private string include;
		private string exclude;
		private string savePath;
		private string label;
		private string feed;
		
		public RssFilter() : this(_("New Filter"))
		{
		}
		
		
		public RssFilter(string name)
		{
			this.name = name;
			include = string.Empty;
			exclude = string.Empty;
			savePath = string.Empty;
			label = string.Empty;
			feed = _("All");
		}
		
		
		public string Name {
			get { return name; }
			set { name = value; }
		}
		
		
		public string Include {
			get { return include; }
			set { include = value; }
		}
		
		// If set to null Exclude is assigned string.Empty
		public string Exclude {
			get { return exclude; }
			set { 
				if (value == null)
					exclude = string.Empty;
				else 
					exclude = value;
			}
		}
		
		
		public string Label {
			get { return label; }
			set { label = value; }
		}
		
		public string Feed {
			get { return feed; }
			set { feed = value; }
		}
		
		public string SavePath {
			get { return savePath; }
			set { savePath = value; }
		}
		
		private static string _(string s)
		{
			return Mono.Unix.Catalog.GetString(s);
		}
	}
}
