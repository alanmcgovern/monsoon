
using System;
using Mono.Addins;
using MonoTorrent;
using System.Net;

namespace Monsoon
{
	[TypeExtensionPoint ("/monsoon/dht")]
	public interface IDhtExtension : MonoTorrent.IDhtEngine
	{

	}
}
