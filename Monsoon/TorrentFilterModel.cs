// /home/buchan/monotorrent/Monsoon/TorrentFilterModel.cs created with MonoDevelop
// User: buchan at 03:43 07/15/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;

namespace Monsoon
{
	
	
	public class TorrentFilterModel : TreeModelFilter, TreeDragDest
	{
		public TorrentFilterModel(TreeModel model, TreePath path) : base(model, path)
		{
		}
		
		public bool RowDropPossible(TreePath path, SelectionData selectionData)
		{
			return true;
		}
		
		public bool DragDataReceived(TreePath path, SelectionData selectionData)
		{
			return true;
		}
	}
}
