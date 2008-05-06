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

namespace Monsoon
{
	
	
	public partial class EditColumnsDialog : Gtk.Dialog
	{
		TorrentTreeView torrentTreeView;
		public EditColumnsDialog(TorrentTreeView torrentTreeView)
		{
			this.Build();
			
			this.torrentTreeView = torrentTreeView;
			
			Title = _("Edit columns");
			Modal = true;
			
			nameVisibleCheckButton.Active = torrentTreeView.nameColumn.Visible;
			statusVisibleCheckButton.Active = torrentTreeView.statusColumn.Visible;
			doneVisibleCheckButton.Active = torrentTreeView.doneColumn.Visible;
			seedsVisibleCheckButton.Active = torrentTreeView.seedsColumn.Visible;
			peersVisibleCheckButton.Active = torrentTreeView.peersColumn.Visible;
			downSpeedVisibleCheckButton.Active = torrentTreeView.downSpeedColumn.Visible;
			upSpeedVisibleCheckButton.Active = torrentTreeView.upSpeedColumn.Visible;
			ratioVisibleCheckButton.Active = torrentTreeView.ratioColumn.Visible;
			sizeVisibleCheckButton.Active = torrentTreeView.sizeColumn.Visible;
			etaVisibleCheckButton.Active = torrentTreeView.etaColumn.Visible;
			
			nameVisibleCheckButton.Toggled += OnNameVisibleToggled;
			statusVisibleCheckButton.Toggled += OnStatusVisibleToggled;
			doneVisibleCheckButton.Toggled += OnDoneVisibleToggled;
			seedsVisibleCheckButton.Toggled += OnSeedsVisibleToggled;
			peersVisibleCheckButton.Toggled += OnPeersVisibleToggled;
			downSpeedVisibleCheckButton.Toggled += OnDownSpeedVisibleToggled;
			upSpeedVisibleCheckButton.Toggled += OnUpSpeedVisibleToggled;
			ratioVisibleCheckButton.Toggled += OnRatioVisibleToggled;
			sizeVisibleCheckButton.Toggled += OnSizeVisibleToggled;
			etaVisibleCheckButton.Toggled += OnEtaVisibleToggled;
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
		
		private void OnEtaVisibleToggled(object sender, EventArgs args)
		{
			torrentTreeView.etaColumn.Visible = etaVisibleCheckButton.Active;
		}
		
		private static string _(string s)
		{
			return Mono.Unix.Catalog.GetString(s);
		}
	}
}
