using Gtk;
using System;

namespace Monsoon
{
	
	
	public class AboutDialog : Gtk.AboutDialog
	{
		
		public AboutDialog() : base()
		{
			Name = "Monsoon";
			Authors = new String[]{"Alan McGovern (Library)",
						"Jared Hendry (Interface)"};
			
		}
	}
}
