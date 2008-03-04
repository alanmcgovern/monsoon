//
// FirstRunDruid.cs
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
using Gnome;
namespace Monsoon
{
	
	
	public class FirstRunDruid : Gnome.Druid
	{
		private DruidPageEdge startPage;
		private DruidPageEdge endPage;
		private DruidPageStandard connectionPage;
		private DruidPageStandard storagePage;
		
		private DruidConnectionPage connectionPageWidget;
		private DruidStoragePage storagePageWidget;
		
		public FirstRunDruid (string title, bool close_on_cancel) : base (title, close_on_cancel)
		{
			connectionPageWidget = new DruidConnectionPage();
			storagePageWidget = new DruidStoragePage();

			BuildPages ();
		}
		
		private void BuildPages ()
		{
			BuildStartPage();
			BuildEndPage();
			BuildConnectionPage();
			BuildStoragePage();
			
			AppendPage(startPage);
			AppendPage(connectionPage);
			AppendPage(storagePage);
			AppendPage(endPage);
		}
		
		private void BuildStartPage()
		{
			startPage = new DruidPageEdge(EdgePosition.Start, true,
			"Welcome to MonoTorrent",
			"This is your first time running MonoTorrent! You will be guided through a few basics steps to help configure MonoTorrent.", null, null, null);	
		}
		
		private void BuildEndPage()
		{
			endPage = new DruidPageEdge(EdgePosition.Finish, true,
			"Configuration Complete",
			"Congratulations, you have successfully configured MonoTorrent!", null, null, null);
		}
		
		private void BuildConnectionPage()
		{
			connectionPage = new DruidPageStandard("Connection Settings", null, null);
			connectionPage.VBox.Add(connectionPageWidget);
		}
		
		private void BuildStoragePage()
		{
			storagePage = new DruidPageStandard("Storage Settings", null, null);
			storagePage.VBox.Add(storagePageWidget);
		}
		
		public int ListenPort {
			get { return connectionPageWidget.ListenPort; }
		}
		
		public bool UpnpEnabled {
			get { return connectionPageWidget.UpnpEnabled; }
		}
		
		public int GlobalMaxDownloadSpeed {
			get { return connectionPageWidget.GlobalMaxDownloadSpeed; }
		}
		
		public int GlobalMaxUploadSpeed { 
			get { return connectionPageWidget.GlobalMaxUploadSpeed; }
		}
		
		public string SavePath {
			get { return storagePageWidget.SavePath; }
		}
		
		public string TorrentStorageLocation {
			get { return storagePageWidget.TorrentStorageLocation; }
		}
	}
}
