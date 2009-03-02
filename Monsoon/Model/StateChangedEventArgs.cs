
using System;

namespace Monsoon
{
	public class StateChangedEventArgs : EventArgs
	{
		public Download Download {
			get; private set;
		}
		
		public State NewState {
			get; private set;
		}
		
		public State OldState {
			get; private set;
		}
		
		public StateChangedEventArgs (Download download, State newState, State oldState)
		{
			Download = download;
			OldState = oldState;
			NewState = newState;
		}
	}
}
