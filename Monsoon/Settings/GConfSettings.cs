
using System;

namespace Monsoon
{
	public abstract class GConfSettings <T> : SettingsController <T>
		where T : new ()
	{
		protected  U Get <U> (string key)
		{
			return Get <U> (key, default (U));
		}
		
		protected U Get <U> (string key, object defaultValue)
		{
			try {
				if (typeof (U).IsEnum)
					return (U) Enum.Parse (typeof (U), GconfSettingsStorage.Instance.Retrieve (key).ToString (), true);
				return (U) GconfSettingsStorage.Instance.Retrieve (key);
			} catch {
				return (U) defaultValue;
			}
		}

		protected U GetAbsolute <U> (string key)
		{
			return GetAbsolute <U> (key, default (U));
		}
		
		protected U GetAbsolute <U> (string key, object defaultValue)
		{
			try {
				if (typeof (U).IsEnum)
					return (U) Enum.Parse (typeof (U), GconfSettingsStorage.Instance.RetrieveAbsolute (key).ToString (), true);
				return (U) GconfSettingsStorage.Instance.RetrieveAbsolute (key);
			} catch {
				return (U) defaultValue;
			}
		}
		
		protected void Set (string key, object value)
		{
			GconfSettingsStorage.Instance.Store (key, value);
		}
	}
}
