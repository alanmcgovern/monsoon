
using System;

namespace Monsoon
{
	public abstract class GConfSettings <T> : SettingsController <T>
		where T : new ()
	{
		protected U Get <U> (string key)
		{
			try {
				return (U) GconfSettingsStorage.Instance.Retrieve (key);
			} catch {
				return default (U);
			}
		}
		
		protected void Set (string key, object value)
		{
			GconfSettingsStorage.Instance.Store (key, value);
		}
	}
}
