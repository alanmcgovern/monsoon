// /home/buchan/monotorrent/MonoTorrent.Interface/CreateTorrentProgressDialog.cs created with MonoDevelop
// User: buchan at 23:02 07/26/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace MonoTorrent.Interface
{
	public partial class TorProgressDlg : Gtk.Dialog
	{
		
		public TorProgressDlg()
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
