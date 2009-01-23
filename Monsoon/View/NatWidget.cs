// NatWidget.cs created with MonoDevelop
// User: alan at 13:06 09/07/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gdk;
using Gtk;

namespace Monsoon
{
    [System.ComponentModel.Category("Monsoon")]
    [System.ComponentModel.ToolboxItemAttribute ("NatWidget")]
    public class NatWidget : Gtk.DrawingArea
    {
        private DateTime lastIncoming;
        private bool portForwarded;
        private bool routerFound;
        
        public bool HasIncoming
        {
            get { return (DateTime.Now - lastIncoming) < TimeSpan.FromMinutes(5); }
            set
            {
                bool changed = HasIncoming != value;
                lastIncoming = value ? DateTime.Now : DateTime.MinValue; 
                if (changed)
                    Redraw ();
            }
        }
        
        public bool RouterFound
        {
            get { return routerFound; }
            set
            {
                bool changed = RouterFound != value;
                routerFound = value;
                if (changed)
                    Redraw ();
            }
        }
        
        public bool PortForwarded
        {
            get { return portForwarded; }
            set
            {
                bool changed = portForwarded != value;
                portForwarded = value;
                if (changed)
                    Redraw ();
            }
        }
        
        public NatWidget()
        {
            this.ExposeEvent += Render;
        }
        
        private void Redraw ()
        {
            int width, height;
            
            this.GdkWindow.GetSize (out width, out height);
            this.GdkWindow.InvalidateRect (new Rectangle(0, 0, width, height), false);
        }
        
        private void Render(object o, Gtk.ExposeEventArgs e)
        {
            TooltipText = GetToolTip();
            
            using (Gdk.GC gc = new Gdk.GC (e.Event.Window))
            {
                e.Event.Window.Clear ();
                
                // Calculate center and radius of nat indicator
                int r = (int)(Math.Min (e.Event.Area.Width, e.Event.Area.Height) * 0.5);
                int x = (e.Event.Area.Width - r) / 2 ;
                int y = (e.Event.Area.Height - r) / 2;
                
                // Paint a black ring
                gc.RgbFgColor = new Color (0, 0, 0);
                e.Event.Window.DrawArc (gc, true, x, y, r, r, 0, 360 * 64);
                
                // Paint the inner colour
                r -= 2;
                x += 1;
                y += 1;
                gc.RgbFgColor = GetColour ();
                e.Event.Window.DrawArc (gc, true, x, y, r, r, 0, 360 * 64);
            }
        }
        
        private Color GetColour ()
        {
            Color c = new Color();
            Color.Parse ("yellow", ref c);
            if (HasIncoming)
                return new Color (0, 255, 0);
            if (RouterFound && PortForwarded)
                return c;
            else
                return new Color (255, 0, 0);
        }
        
        private string GetToolTip ()
        {
            if (HasIncoming)
                return _("OK");
            
            if (RouterFound && PortForwarded)
                return _("No incoming connections received");
            
            if (RouterFound && !PortForwarded)
                return _("Problem forwarding port");
            
            else
                return _("No upnp/nat-pmp enabled router detected");
        }
        
        private string _(string s)
        {
            return s;
        }
    }
}
