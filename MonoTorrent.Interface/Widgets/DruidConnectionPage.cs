//
// DruidConnectionPage.cs
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
	
	
	public partial class DruidConnectionPage : Gtk.Bin
	{
		
		public DruidConnectionPage()
		{
			this.Build();
			// TODO: Add a "Test button" and script on esoteriq.org that connects to the users open port and verifies it is open
			downSpinButton.SetRange(0, int.MaxValue);
			upSpinButton.SetRange(0, int.MaxValue);
		}
		
		public int ListenPort {
			get { return (int) portSpinButton.Value; }
		}
		
		public bool UpnpEnabled {
			get { return upnpCheckButton.Active; }
		}
		
		public int GlobalMaxDownloadSpeed {
			get { return (int) downSpinButton.Value; }
		}
		
		public int GlobalMaxUploadSpeed {
			get { return (int) upSpinButton.Value; }
		}
	}
}
