
using System;
using Mono.Addins;
using MonoTorrent;
using System.Net;

namespace Monsoon
{
	[TypeExtensionPoint ("/monotorrent/dht")]
	public interface IDhtExtension : MonoTorrent.IDhtEngine
	{

	}
}
