// /home/buchan/monotorrent/Monsoon/EditColumnsDialog.cs created with MonoDevelop
// User: buchan at 02:36 07/18/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Monsoon
{
	
	
	public partial class EditColumnsDialog : Gtk.Dialog
	{
		TorrentTreeView torrentTreeView;
		public EditColumnsDialog(TorrentTreeView torrentTreeView)
		{
			this.Build();
			
			this.torrentTreeView = torrentTreeView;
			
			Title = "Edit columns";
			Modal = true;
			
			nameVisibleCheckButton.Active = torrentTreeView.nameColumn.Visible;
			statusVisibleCheckButton.Active = torrentTreeView.statusColumn.Visible;
			doneVisibleCheckButton.Active = torrentTreeView.doneColumn.Visible;
			seedsVisibleCheckButton.Active = torrentTreeView.seedsColumn.Visible;
			peersVisibleCheckButton.Active = torrentTreeView.peersColumn.Visible;
			downSpeedVisibleCheckButton.Active = torrentTreeView.downSpeedColumn.Visible;
			upSpeedVisibleCheckButton.Active = torrentTreeView.upSpeedColumn.Visible;
			ratioVisibleCheckButton.Active = torrentTreeView.ratioColumn.Visible;
			sizeVisibleCheckButton.Active = torrentTreeView.ratioColumn.Visible;
			
			nameVisibleCheckButton.Toggled += OnNameVisibleToggled;
			statusVisibleCheckButton.Toggled += OnStatusVisibleToggled;
			doneVisibleCheckButton.Toggled += OnDoneVisibleToggled;
			seedsVisibleCheckButton.Toggled += OnSeedsVisibleToggled;
			peersVisibleCheckButton.Toggled += OnPeersVisibleToggled;
			downSpeedVisibleCheckButton.Toggled += OnDownSpeedVisibleToggled;
			upSpeedVisibleCheckButton.Toggled += OnUpSpeedVisibleToggled;
			ratioVisibleCheckButton.Toggled += OnRatioVisibleToggled;
			sizeVisibleCheckButton.Toggled += OnSizeVisibleToggled;
		}
		
		private void OnNameVisibleToggled(object sender, EventArgs args)
		{
			torrentTreeView.nameColumn.Visible = nameVisibleCheckButton.Active;
		}
		
		private void OnStatusVisibleToggled(object sender, EventArgs args)
		{
			torrentTreeView.statusColumn.Visible = statusVisibleCheckButton.Active;
		}
		
		private void OnDoneVisibleToggled(object sender, EventArgs args)
		{
			torrentTreeView.doneColumn.Visible = statusVisibleCheckButton.Active;
		}
		
		private void OnSeedsVisibleToggled(object sender, EventArgs args)
		{
			torrentTreeView.seedsColumn.Visible = seedsVisibleCheckButton.Active;
		}
		
		private void OnPeersVisibleToggled(object sender, EventArgs args)
		{
			torrentTreeView.peersColumn.Visible = peersVisibleCheckButton.Active;
		}
		
		private void OnDownSpeedVisibleToggled(object sender, EventArgs args)
		{
			torrentTreeView.downSpeedColumn.Visible = downSpeedVisibleCheckButton.Active;
		}
		
		private void OnUpSpeedVisibleToggled(object sender, EventArgs args)
		{
			torrentTreeView.upSpeedColumn.Visible = upSpeedVisibleCheckButton.Active;
		}
		
		private void OnRatioVisibleToggled(object sender, EventArgs args)
		{
			torrentTreeView.ratioColumn.Visible = ratioVisibleCheckButton.Active;
		}
		
		private void OnSizeVisibleToggled(object sender, EventArgs args)
		{
			torrentTreeView.sizeColumn.Visible = sizeVisibleCheckButton.Active;
		}
		
	}
}
