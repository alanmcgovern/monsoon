
using System;
using System.Collections.Generic;

namespace Monsoon
{
	public static class ServiceManager
	{
		static Dictionary <Type, IService> services = new Dictionary<Type, IService> ();
		
		public static T Get <T> ()
			where T : class, IService, new ()
		{
			if (!services.ContainsKey (typeof (T))) {
				T t = new T ();
				services.Add (typeof (T), t);
				return t;
			}
			return (T) services [typeof (T)];
		}
	}
}
