
using System;

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
	}
}
