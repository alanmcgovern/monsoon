//
// PiecesTreeView.cs
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
			
			// I18N: number of / amount
			numberColumn.Title = _("#");
			sizeColumn.Title = _("Size");
			numBlocksColumn.Title = _("# of Blocks");
			blockColumn.Title = _("Blocks");
			//completedColumn.Title = _("Completed");
			
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
			(cell as Gtk.CellRendererText).Text = ByteConverter.ConvertSize(blockEvent.Block.RequestLength * blockEvent.Piece.BlockCount);
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
		private static string _(string s)
		{
			return Mono.Unix.Catalog.GetString(s);
		}
	}
}
