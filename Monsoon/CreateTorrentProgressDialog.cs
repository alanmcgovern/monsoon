// /home/buchan/monotorrent/Monsoon/CreateTorrentProgressDialog.cs created with MonoDevelop
// User: buchan at 23:02 07/26/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Monsoon
{
	
	
	public partial class CreateTorrentProgressDialog : Gtk.Dialog
	{
		
		public CreateTorrentProgressDialog()
		{
			this.Build();
		}
		
		public double Progress
		{
			set{
				createProgressBar.Fraction = value / 100;
				createProgressBar.Text = (value / 100).ToString("0%");	
			}
		}
		
		public string File
		{
			set { fileLabel.Text = value; }		
		}
	}
}
