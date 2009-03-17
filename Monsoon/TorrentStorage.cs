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
	[Serializable]
	[XmlRoot("Torrent")]
	public class TorrentStorage
	{
		private string torrentPath;
		private string savePath;
		private TorrentSettings settings;
		private State state;
		private long uploadedData;
		private long downloadedData;
		private string infoHash;
		List<TorrentFileSettingsModel> files;

		public int Priority {
			get; set;
		}
		
		public TorrentStorage()
		{
			files = new List<TorrentFileSettingsModel>();
		}

		public string TorrentPath
		{
			get { return torrentPath; }
			set { torrentPath = value; }
		}

		public string SavePath
		{
			get { return savePath; }
			set { savePath = value; }
		}

		public TorrentSettings Settings
		{
			get { return settings; }
			set { settings = value; }
		}

		public State State
		{
			get { return state; }
			set { state = value; }
		}

		public long UploadedData
		{
			get { return uploadedData; }
			set { uploadedData = value; }
		}
		
		public long DownloadedData
		{
			get { return downloadedData; }
			set { downloadedData = value; }
		}
		
		public string InfoHash {
			get { return infoHash; }
			set { infoHash = value; }
		}

		public List<TorrentFileSettingsModel> Files {
			get { return files; }
			set { files = value; }
		}
		
	}
}
