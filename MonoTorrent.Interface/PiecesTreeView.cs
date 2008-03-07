// /home/buchan/monotorrent/Monsoon/PiecesTreeView.cs created with MonoDevelop
// User: buchan at 16:34 07/18/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;
using MonoTorrent.Client;

namespace Monsoon
{
	public class PiecesTreeView : TreeView
	{
		private TreeViewColumn numberColumn;
		private TreeViewColumn sizeColumn;
		private TreeViewColumn numBlocksColumn;
		private TreeViewColumn blockColumn;
		//private TreeViewColumn completedColumn;
		
		public PiecesTreeView()
		{
			HeadersVisible = true;
			Selection.Mode = SelectionMode.None;	
			
			numberColumn = new TreeViewColumn();
			sizeColumn = new TreeViewColumn();
			numBlocksColumn = new TreeViewColumn();
			blockColumn = new TreeViewColumn();
			//completedColumn = new TreeViewColumn();
			
			numberColumn.Title = "#";
			sizeColumn.Title = "Size";
			numBlocksColumn.Title = "# of Blocks";
			blockColumn.Title = "Blocks";
			//completedColumn.Title = "Completed";
			
			Gtk.CellRendererText numberCell = new Gtk.CellRendererText();
			Gtk.CellRendererText sizeCell = new Gtk.CellRendererText();
			Gtk.CellRendererText numBlocksCell = new Gtk.CellRendererText();
			CellRendererPiece blockCell = new CellRendererPiece();
			//Gtk.CellRendererText completedCell = new Gtk.CellRendererText();
			
			numberColumn.PackStart(numberCell, true);
			sizeColumn.PackStart(sizeCell, true);
			numBlocksColumn.PackStart(numBlocksCell, true);
			blockColumn.PackStart(blockCell, true);
			//completedColumn.PackStart(completedCell, true);
			
			numberColumn.SetCellDataFunc (numberCell, new Gtk.TreeCellDataFunc (RenderNumber));
			sizeColumn.SetCellDataFunc (sizeCell, new Gtk.TreeCellDataFunc (RenderSize));
			numBlocksColumn.SetCellDataFunc (numBlocksCell, new Gtk.TreeCellDataFunc (RenderNumBlocks));
			blockColumn.SetCellDataFunc (blockCell, new Gtk.TreeCellDataFunc (RenderBlock));
			//completedColumn.SetCellDataFunc (completedCell, new Gtk.TreeCellDataFunc (RenderCompleted));
			
			
			
			AppendColumn(numberColumn);
			AppendColumn(sizeColumn);
			AppendColumn(numBlocksColumn);
			AppendColumn(blockColumn);
			//AppendColumn(completedColumn);
		}
		
		private void RenderNumber (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			BlockEventArgs blockEvent = (BlockEventArgs) model.GetValue (iter, 0);
			(cell as Gtk.CellRendererText).Text = blockEvent.Piece.Index.ToString();
		}
		
		private void RenderSize (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			BlockEventArgs blockEvent = (BlockEventArgs) model.GetValue (iter, 0);
			(cell as Gtk.CellRendererText).Text = ByteConverter.Convert(blockEvent.Block.RequestLength);
		}		
		
		private void RenderNumBlocks (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			BlockEventArgs blockEvent = (BlockEventArgs) model.GetValue (iter, 0);
			(cell as Gtk.CellRendererText).Text = blockEvent.Piece.BlockCount.ToString();
		}
		
		private void RenderBlock (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			BlockEventArgs blockEvent = (BlockEventArgs) model.GetValue (iter, 0);
			(cell as CellRendererPiece).BlockEvent = blockEvent;
			
		}
		/*
		private void RenderCompleted (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			BlockEventArgs blockEvent = (BlockEventArgs) model.GetValue (iter, 0);
			(cell as Gtk.CellRendererText).Text = (blockEvent.Piece.TotalReceived / blockEvent.Block.RequestLength).ToString("0.00");
		}
		*/
	}
}
