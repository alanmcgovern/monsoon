
using System;
using Mono.Addins;
using MonoTorrent;
using System.Net;

namespace Monsoon
{
	[TypeExtensionPoint]
	public interface DhtExtension : IDhtEngine
	{
		
	}
}
