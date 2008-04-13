//
// SpeedLimitMenu.cs
//
// Author:
//   Alan McGovern (alan.mcgovern@gmail.com)
//
// Copyright (C) 2008 Alan McGovern
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
			
			SpeedMenuItem l = new SpeedMenuItem (_("Unlimited"));
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
		
		private static string _(string s)
		{
			return Mono.Unix.Catalog.GetString(s);
		}
	}
}
