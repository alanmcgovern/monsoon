//
// TorrentStorage.cs
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
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

using MonoTorrent.Client;
using MonoTorrent.Common;

namespace Monsoon
{

	[XmlRoot("Torrent")]
	public class TorrentStorage
	{
		private string torrentPath;
		private string savePath;
		private UserTorrentSettings settings;
		private TorrentState state;
		private long uploadedData;
		private long downloadedData;
		
		public TorrentStorage()
		{
		}
		
		public TorrentStorage(string torrentPath, string savePath, UserTorrentSettings settings, TorrentState state, long uploadedData, long downloadedData)
		{
			this.torrentPath = torrentPath;
			this.savePath = savePath;
			this.settings = settings;
			this.state = state;
			this.uploadedData = uploadedData;
			this.downloadedData = downloadedData;
		}
		
		[XmlAttribute("TorrentPath")]
		public string TorrentPath
		{
			get { return torrentPath; }
			set { torrentPath = value; }
		}
		
		[XmlElement("SavePath")]
		public string SavePath
		{
			get { return savePath; }
			set { savePath = value; }
		}
		
		[XmlElement("Settings")]
		public UserTorrentSettings Settings
		{
			get { return settings; }
			set { settings = value; }
		}
		
		[XmlElement("State")]
		public TorrentState State
		{
			get { return state; }
			set { state = value; }
		}
		
		[XmlElement("UploadedData")]
		public long UploadedData
		{
			get { return uploadedData; }
			set { uploadedData = value; }
		}
		
		[XmlElement("DownloadedData")]
		public long DownloadedData
		{
			get { return downloadedData; }
			set { downloadedData = value; }
		}
	}
}
