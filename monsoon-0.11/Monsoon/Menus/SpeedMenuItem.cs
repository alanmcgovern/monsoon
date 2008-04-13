// SpeedMenuItem.cs created with MonoDevelop
// User: alan at 21:18 31/03/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;

namespace Monsoon
{
	public class SpeedMenuItem : MenuItem
	{
		private int speed;
		public int Speed
		{
			get { return speed; }
			set
			{
				speed = value;
				((Label) Child).Text = ByteConverter.ConvertSpeed (speed);
			}
		}
		
		public SpeedMenuItem()
		{
		}
		
		public SpeedMenuItem(string label)
			:base (label)
		{
		}
	}
}
