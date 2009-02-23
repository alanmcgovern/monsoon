
using System;
using Gtk;

namespace Monsoon
{
	public static class Event
	{
		public static void Raise (EventHandler h, object o, EventArgs e)
		{
			if (h != null)
				h (o, e);
		}
		
		public static void Raise<T> (EventHandler<T> h, object o, T e)
			where T : EventArgs
		{
			if (h != null)
				h (o, e);
		}
		
		internal static EventHandler Wrap (EventHandler h)
		{
			return delegate (object o, EventArgs e) {
				h (o, e);
			};
		}
		internal static EnterNotifyEventHandler Wrap (EnterNotifyEventHandler h)
		{
			return delegate (object o, EnterNotifyEventArgs e) {
				h (o, e);
			};
		}
		internal static DragBeginHandler Wrap (DragBeginHandler h)
		{
			return delegate (object o, DragBeginArgs e) {
				h (o, e);
			};
		}
		internal static DragEndHandler Wrap (DragEndHandler h)
		{
			return delegate (object o, DragEndArgs e) {
				h (o, e);
			};
		}
		internal static GLib.IdleHandler Wrap (GLib.IdleHandler h)
		{
			return delegate {
				return h ();
			};
		}
		internal static EditedHandler Wrap (EditedHandler h)
		{
			return delegate (object o, EditedArgs e) {
				h (o, e);
			};
		}
	}
}
