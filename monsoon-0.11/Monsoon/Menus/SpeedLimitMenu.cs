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
		private List<int> speeds;
		private List<SpeedMenuItem> labels;
		private bool isUpload;
		
		public bool IsUpload
		{
			get { return isUpload; }
			set { isUpload = value; }
		}
		
		public SpeedLimitMenu()
		{
			
			// This should always be an uneven number
			labels = new List<SpeedMenuItem>(13);
			speeds = new List<int> (labels.Capacity);
			
			SpeedMenuItem l = new SpeedMenuItem ("Unlimited");
			l.Activated += delegate (object sender, EventArgs e) {
				if (ClickedItem != null)
					ClickedItem (sender, e);
			};
			Append (l);
			
			for (int i = 0; i < labels.Capacity; i++)
			{
				l = new SpeedMenuItem ("");
				l.Activated += delegate (object sender, EventArgs e) {
					if (ClickedItem != null)
						ClickedItem (sender, e);
				};
				labels.Add(l);
				Append(l);
			}
		}
		
		public void CalculateSpeeds(int currentSpeed)
		{
			int centre = labels.Count / 2;
			currentSpeed = currentSpeed / 1024; // Convert to kB/sec for this method

			// If unlimited, show speeds around the 50kB/sec mark
			if (currentSpeed == 0)
				currentSpeed = 50;
			
			// If it's really slow, don't go below 8kB/sec
			if (currentSpeed < 8)
				currentSpeed = 8;
			
			speeds.Clear();
			
			int smallIncrement = Math.Max(1, currentSpeed / 100);
			// Place steps if 1kB/sec beside the current speed
			for (int i= - 2; i <= 2; i++)
				speeds.Add(currentSpeed + smallIncrement * i);
			
			// Now do some percentage based steps. We multiply current speed
			// by (i+2) / centre. If we have 11 elements, centre == 5, which means
			// we get 2/5, 3/5 and 4/5 of current speed.
			for (int i = 0; i < (labels.Count - 5) / 2; i++)
			{
				int s =  (int)(currentSpeed * ( (i+2.0)/centre));
				if (!speeds.Contains(s))
					speeds.Add(s);
				
				if(!speeds.Contains(currentSpeed + s))
					speeds.Add(currentSpeed + s);
			}
			
			// Make sure we have exactly the right number of speeds
			// in our menu. If we don't have enough, then we keep calculating higher
			// speeds
			double increment = 1.1;
			while(speeds.Count < labels.Count)
			{
				int s = (int)(speeds[speeds.Count - 1] * 1.1);
				if (speeds.Contains(s))
					increment += 0.1;
				else
					speeds.Add(s);
			}
			
			speeds.Sort();
			speeds.Reverse();
			
			for (int i=0; i < speeds.Count; i++)
				labels[i].Speed = Math.Max (0, speeds[i]) * 1024;
		}
	}
}
