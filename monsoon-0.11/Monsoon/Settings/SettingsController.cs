// InterfaceSettingsController.cs created with MonoDevelop
// User: alan at 23:06 03/04/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Monsoon
{
	public abstract class SettingsController<T>
		where T : new()
	{
		private T settings;
		
		public T Settings
		{
			get { return settings; }
		}
		
		public SettingsController()
		{
			settings = new T();
		}
		
		public abstract void Save ();
		
		public abstract void Load ();
	}
}
