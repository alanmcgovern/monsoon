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
    public class NatWidget : HBox
    {
        const int Radius = 14;
        const int Diameter = Radius * 2;
        
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

        Pixmap map;
        Gtk.Image image;
        public NatWidget()
        {
            map = new Pixmap (null, Diameter, Diameter, 24);
            image = new Gtk.Image (map, null);
            map.Colormap = Gdk.Colormap.System;
            PackStart (image);
            DoubleBuffered = true;
            Redraw ();
        }
        
        private void Redraw ()
        {
            TooltipText = GetToolTip();
            using (Gdk.GC gc = new Gdk.GC (map))
            {
                gc.Colormap = Gdk.Colormap.System;
                
                gc.RgbFgColor = new Color (255, 255, 255);
                map.DrawRectangle (gc, true, 0, 0, Diameter, Diameter);
                
                // Paint a black ring
                gc.RgbFgColor = new Color (0, 0, 0);
                map.DrawArc (gc, true, Radius / 2, Radius / 2, Radius, Radius, 0, 360 * 64);
                
                // Paint the inner colour
                gc.RgbFgColor = GetColour ();
                map.DrawArc (gc, true,  Radius / 2 + 1, Radius / 2 + 1, Radius - 2, Radius - 2, 0, 360 * 64);
            }
            image.QueueDraw ();
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
