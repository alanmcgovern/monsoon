// /home/buchan/monotorrent/Monsoon/UnhandledExceptionDialog.cs created with MonoDevelop
// User: buchan at 14:48 07/23/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;

namespace Monsoon
{
	
	
	public partial class UnhandledExceptionDialog : Gtk.Dialog
	{
		
		public UnhandledExceptionDialog(Exception e)
		{
			this.Build();
			Modal = true;
			
			exceptionImage.IconName = Stock.DialogError;
			exceptionImage.IconSize = 6;
			
			exceptionLabel.Text = "An unhandled exception has been encountered:\n" + e.Message;
			
			TextBuffer buffer;
			buffer = exceptionTextView.Buffer;
			buffer.Text = e.ToString() + e.StackTrace;
		}
	}
}
