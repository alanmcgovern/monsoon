
using System;

namespace Monsoon
{
	public class LabelEventArgs : EventArgs
	{
		public TorrentLabel Label {
			get; private set;
		}
		
		public LabelEventArgs (TorrentLabel label)
		{
			Label = label;
		}
	}
}
