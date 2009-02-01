//
// CellRendererPiece.cs (Gtk port of BlockProgressBar.cs)
//
// Author:
//   Alan McGovern (alan.mcgovern@gmail.com)
//   Jared Hendry (buchan@gmail.com)
//
// Copyright (C) 2007 Alan McGovern
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
using Gdk;
using MonoTorrent.Client;


namespace Monsoon
{
	public class CellRendererPiece : Gtk.CellRenderer
	{
		
		private Piece blockEvent;
		
		public CellRendererPiece() : base()
		{
		
			blockEvent = null;
		}
		
		protected override void Render(Gdk.Drawable window, Widget widget, Gdk.Rectangle background_area, Gdk.Rectangle cell_area, Gdk.Rectangle expose_area, CellRendererState flags)
		{
			Gdk.GC gc = new Gdk.GC(window);
			
			int width = cell_area.Width / blockEvent.BlockCount;
			
			for (int i = 0; i < blockEvent.BlockCount; i++) {
				if(blockEvent.AllBlocksReceived)
					gc.RgbFgColor = new Gdk.Color(179, 139, 83); // Brown
				else if (blockEvent[i].Written)
					gc.RgbFgColor = new Gdk.Color(145, 246, 145); // Green
				else if (blockEvent[i].Received)
					gc.RgbFgColor = new Gdk.Color(232, 176, 6); // Gold
				else if (blockEvent[i].Requested)
					gc.RgbFgColor = new Gdk.Color(112, 180, 224); // Blue
				else
					gc.RgbFgColor = new Gdk.Color(248, 227, 212); // Pink
					
				Gdk.Rectangle rect = new Gdk.Rectangle(background_area.X + (width * i), background_area.Y + 1, width, background_area.Height - 2 * 1);
				window.DrawRectangle(gc,true,rect);
			}
		}
		
		public override void GetSize (Widget widget, ref Gdk.Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			height = 0;
			width = 0 ;
			y_offset = 0;
			x_offset = 0;
		}
		
		public Piece Piece {
			set { blockEvent = value; }
		}
	}
}
