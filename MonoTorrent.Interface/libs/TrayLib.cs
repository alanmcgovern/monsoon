// http://www.mono-project.com/GtkSharpNotificationIcon

using System;
using System.Runtime.InteropServices;
 
using Gtk;
using Gdk;
 
namespace Egg
{
	public class TrayIcon : Plug
	{
		//int stamp;
		//Orientation orientation;
		
		int selection_atom;
		//int manager_atom;
		int system_tray_opcode_atom;
		//int orientation_atom;
		IntPtr manager_window;
		//FilterFunc filter;
		
		public TrayIcon (string name)
		{
			Title = name;
			//stamp = 1;
			//orientation = Orientation.Horizontal;
			AddEvents ((int)EventMask.PropertyChangeMask);
			//filter = new FilterFunc (ManagerFilter);
		}
 
		protected override void OnRealized ()
		{
			base.OnRealized ();
			Display display = Screen.Display;
			IntPtr xdisplay = gdk_x11_display_get_xdisplay (display.Handle);
			selection_atom = XInternAtom (xdisplay, "_NET_SYSTEM_TRAY_S" + Screen.Number.ToString (), false);
			//manager_atom = 
			XInternAtom (xdisplay, "MANAGER", false);
			system_tray_opcode_atom = XInternAtom (xdisplay, "_NET_SYSTEM_TRAY_OPCODE", false);
			//orientation_atom = 
			XInternAtom (xdisplay, "_NET_SYSTEM_TRAY_ORIENTATION", false);
			UpdateManagerWindow ();
			//Screen.RootWindow.AddFilter (filter);
		}
 
		protected override void OnUnrealized ()
		{
			if (manager_window != IntPtr.Zero)
			{
				//Gdk.Window gdkwin = 
				Gdk.Window.LookupForDisplay (Display, (uint)manager_window);
				//gdkwin.RemoveFilter (filter);
			}
			
			//Screen.RootWindow.RemoveFilter (filter);
			base.OnUnrealized ();
		}
 
		private void UpdateManagerWindow ()
		{
			IntPtr xdisplay = gdk_x11_display_get_xdisplay (Display.Handle);
			if (manager_window != IntPtr.Zero)
			{
				//Gdk.Window gdkwin = 
				Gdk.Window.LookupForDisplay (Display, (uint)manager_window);
				//gdkwin.RemoveFilter (filter);
			}
 
			XGrabServer (xdisplay);
 
			manager_window = XGetSelectionOwner (xdisplay, selection_atom);
			if (manager_window != IntPtr.Zero)
				XSelectInput (xdisplay, manager_window, EventMask.StructureNotifyMask | EventMask.PropertyChangeMask);
			XUngrabServer (xdisplay);
			XFlush (xdisplay);
 
			if (manager_window != IntPtr.Zero) {
				//Gdk.Window gdkwin = 
				Gdk.Window.LookupForDisplay (Display, (uint)manager_window);
				//gdkwin.AddFilter (filter);
				SendDockRequest ();
				GetOrientationProperty ();
			}
		}
 
		private void SendDockRequest ()
		{
			SendManagerMessage (SystemTrayMessage.RequestDock, manager_window, Id, 0, 0);
		}
 
		private void SendManagerMessage (SystemTrayMessage message, IntPtr window, uint data1, uint data2, uint data3)
		{
			XClientMessageEvent ev = new XClientMessageEvent ();
			IntPtr display;
 
			ev.type = XEventName.ClientMessage;
			ev.window = window;
			ev.message_type = (IntPtr)system_tray_opcode_atom;
			ev.format = 32;
			ev.ptr1 = gdk_x11_get_server_time (GdkWindow.Handle);
			ev.ptr2 = (IntPtr)message;
			ev.ptr3 = (IntPtr)data1;
			ev.ptr4 = (IntPtr)data2;
			ev.ptr5 = (IntPtr)data3;
 
			display = gdk_x11_display_get_xdisplay (Display.Handle);
			gdk_error_trap_push ();
			XSendEvent (display, manager_window, false, EventMask.NoEventMask, ref ev);
			gdk_error_trap_pop ();
		}
 		
 		/*
		private FilterReturn ManagerFilter (IntPtr xevent, Event evnt)
		{
			//TODO: Implement;
			return FilterReturn.Continue;
		}
 		*/
 		
		private void GetOrientationProperty ()
		{
			// Implement;
		}
 
		[DllImport ("libgdk-x11-2.0.so.0")]
		static extern IntPtr gdk_x11_display_get_xdisplay (IntPtr display);
		[DllImport ("libgdk-x11-2.0.so.0")]
		static extern IntPtr gdk_x11_get_server_time (IntPtr window);
		[DllImport ("libgdk-x11-2.0.so.0")]
		static extern void gdk_error_trap_push ();
		[DllImport ("libgdk-x11-2.0.so.0")]
		static extern void gdk_error_trap_pop ();
		
		[DllImport ("libX11", EntryPoint="XInternAtom")]
		extern static int XInternAtom(IntPtr display, string atom_name, bool only_if_exists);
		[DllImport ("libX11")]
		extern static void XGrabServer (IntPtr display);
		[DllImport ("libX11")]
		extern static void XUngrabServer (IntPtr display);
		[DllImport ("libX11")]
		extern static int XFlush (IntPtr display);
		[DllImport ("libX11")]
		extern static IntPtr XGetSelectionOwner (IntPtr display, int atom);
		[DllImport ("libX11")]
		extern static IntPtr XSelectInput (IntPtr window, IntPtr display, EventMask mask);
		[DllImport ("libX11", EntryPoint="XSendEvent")]
		extern static int XSendEvent(IntPtr display, IntPtr window, bool propagate, EventMask event_mask, ref XClientMessageEvent send_event);
	}
 
	[Flags]
	internal enum EventMask {
		NoEventMask             = 0,
		KeyPressMask            = 1<<0,
		KeyReleaseMask          = 1<<1,
		ButtonPressMask         = 1<<2,
		ButtonReleaseMask       = 1<<3,
		EnterWindowMask         = 1<<4,
		LeaveWindowMask         = 1<<5,
		PointerMotionMask       = 1<<6,
		PointerMotionHintMask   = 1<<7,
		Button1MotionMask       = 1<<8,
		Button2MotionMask       = 1<<9,
		Button3MotionMask       = 1<<10,
		Button4MotionMask       = 1<<11,
		Button5MotionMask       = 1<<12,
		ButtonMotionMask        = 1<<13,
		KeymapStateMask         = 1<<14,
		ExposureMask            = 1<<15,
		VisibilityChangeMask    = 1<<16,
		StructureNotifyMask     = 1<<17,
		ResizeRedirectMask      = 1<<18,
                SubstructureNotifyMask  = 1<<19,
		SubstructureRedirectMask= 1<<20,
		FocusChangeMask         = 1<<21,
		PropertyChangeMask      = 1<<22,
		ColormapChangeMask      = 1<<23,
		OwnerGrabButtonMask     = 1<<24
	}
 
	internal enum SystemTrayMessage
	{
		RequestDock,
		BeginMessage,
		CancelMessage
	}
 
	internal enum SystemTrayOrientation
	{
		Horz,
		Vert
	}
	
	[StructLayout(LayoutKind.Sequential)]
	internal struct XClientMessageEvent {
		internal XEventName     type;
		internal IntPtr         serial;
		internal bool           send_event;
		internal IntPtr         display;
		internal IntPtr         window;
		internal IntPtr         message_type;
		internal int            format;
		internal IntPtr         ptr1;
		internal IntPtr         ptr2;
		internal IntPtr         ptr3;
		internal IntPtr         ptr4;
		internal IntPtr         ptr5;
	}
 
	internal enum XEventName {
		KeyPress                = 2,
		KeyRelease              = 3,
		ButtonPress             = 4,
		ButtonRelease           = 5,
		MotionNotify            = 6,
		EnterNotify             = 7,
		LeaveNotify             = 8,
		FocusIn                 = 9,
		FocusOut                = 10,
		KeymapNotify            = 11,
		Expose                  = 12,
		GraphicsExpose          = 13,
		NoExpose                = 14,
		VisibilityNotify        = 15,
		CreateNotify            = 16,
		DestroyNotify           = 17,
		UnmapNotify             = 18,
		MapNotify               = 19,
		MapRequest              = 20,
		ReparentNotify          = 21,
		ConfigureNotify         = 22,
		ConfigureRequest        = 23,
		GravityNotify           = 24,
		ResizeRequest           = 25,
		CirculateNotify         = 26,
		CirculateRequest        = 27,
		PropertyNotify          = 28,
		SelectionClear          = 29,
		SelectionRequest        = 30,
		SelectionNotify         = 31,
		ColormapNotify          = 32,
		ClientMessage           = 33,
		MappingNotify           = 34,
		TimerNotify             = 100,
		
		LASTEvent
	}
}