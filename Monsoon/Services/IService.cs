
using System;

namespace Monsoon
{
	public interface IService
	{
		bool Initialised { get; }
		void Initialise ();
	}
}
