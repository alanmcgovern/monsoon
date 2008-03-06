// SpeedLimitMenu.cs created with MonoDevelop
// User: alan at 21:20 06/03/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;
using Gtk;

namespace Monsoon
{
	public class SpeedLimitMenu : Gtk.Menu
	{
		public event EventHandler ClickedItem;
		
		private List<MenuItem> labels;
		private bool isUpload;
		
		public bool IsUpload
		{
			get { return isUpload; }
			set { isUpload = value; }
		}
		
		public SpeedLimitMenu()
		{
			
			// This should always be an uneven number
			labels = new List<MenuItem>(13);
			MenuItem l = new MenuItem ("Unlimited");
			l.Activated += delegate (object sender, EventArgs e) {
				if (ClickedItem != null)
					ClickedItem (sender, e);
			};
			Append (l);
			
			for (int i = 0; i < labels.Capacity; i++)
			{
				l = new MenuItem ("");
				l.Activated += delegate (object sender, EventArgs e) {
					if (ClickedItem != null)
						ClickedItem (sender, e);
				};
				labels.Add(l);
				Append(l);
			}
		}
		
		private void SetLabel (MenuItem item, int speed)
		{
			speed = Math.Max (0, speed);
			((Label) item.Child).Text = ByteConverter.Convert (speed);
		}
		
		public void CalculateSpeeds(int currentSpeed)
		{
			int centre = labels.Count / 2;
			
			if (currentSpeed == 0)
				currentSpeed = 50;
			
			labels[centre].Name = ByteConverter.Convert (currentSpeed);
			for (int i = 0; i <= centre; i++)
			{
				int speed = currentSpeed;
				// Steps of 1
				if (i < 3)
				{
					SetLabel (labels [centre + i], speed - i);
					SetLabel (labels [centre - i], speed + i);
				}
				
				// Steps of 5
				else if (i >= 3 && i < 5)
				{
					speed = (speed / 5) * 5;
					while (speed >= (currentSpeed - 2))
						speed -= 5;
					SetLabel (labels [centre + i], speed - 5 * (i - 3));
					
					speed = (currentSpeed / 5 + 1) * 5;
					while (speed <= (currentSpeed + 2))
						speed += 5;
					SetLabel (labels [centre - i], speed + 5 * (i - 3));
				}
				
				// Steps of 10
				else
				{
					speed = (speed / 10) * 10;
					while (speed >= (currentSpeed - 12))
						speed -= 10;
					SetLabel (labels [centre + i], speed - 10 * (i - 5));
					
					speed = (currentSpeed / 10 + 1) * 10;
					while (speed <= (currentSpeed + 12))
						speed += 10;
					SetLabel (labels [centre - i], speed + 10 * (i - 5));
				}
			}
		}
	}
}
