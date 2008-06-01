//
// ListenPortController.cs
//
// Author:
//   Jared Hendry (buchan@gmail.com)
//
// Copyright (C) 2007 Jared Hendry
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Threading;
using System.Collections.Generic;
using System.Net;

using Mono.Nat;
using Mono.Nat.Pmp;
using Mono.Nat.Upnp;
using MonoTorrent.Client;

namespace Monsoon
{
	public class ListenPortController
	{
		private EngineSettings settings;
		
		private List<INatDevice> devices;
		private Mapping tcpMapping;
		private Mapping udpMapping;
		
		private static NLog.Logger logger = MainClass.DebugEnabled ? NLog.LogManager.GetCurrentClassLogger () : new EmptyLogger ();
		
		public ListenPortController(EngineSettings engineSettings)
		{
			settings = engineSettings;
			
			devices = new List<INatDevice> ();
			tcpMapping = new Mapping (Protocol.Tcp, engineSettings.ListenPort, engineSettings.ListenPort);
			tcpMapping.Description = Defines.ApplicationName;
			
			udpMapping = new Mapping (Protocol.Udp, engineSettings.ListenPort, engineSettings.ListenPort);
			udpMapping.Description = Defines.ApplicationName;
			
			IPAddress[] addresses = null;
			try 
			{
				addresses = NatUtility.GetLocalAddresses (false);
			}
			catch (Exception)
			{
				logger.Warn ("Could not resolve hostname, port forwarding may not work");
				addresses = new IPAddress[] { IPAddress.Loopback };
			}
			
			NatUtility.AddController (new UpnpNatController (addresses));
			NatUtility.AddController (new PmpNatController (addresses));
			
			NatUtility.DeviceFound += OnDeviceFound;
		}
		
		public void Start()
		{
			logger.Info("UPnP starting...");
			running = true;
			NatUtility.StartDiscovery ();
		}
		
		public void ChangePort()
		{
			logger.Info("UPnP changing port map");
			RemoveMap();
			
			tcpMapping = new Mapping (Protocol.Tcp, settings.ListenPort, settings.ListenPort);
			udpMapping = new Mapping (Protocol.Udp, settings.ListenPort, settings.ListenPort);
			tcpMapping.Description = Defines.ApplicationName;
			udpMapping.Description = Defines.ApplicationName;

			MapPort();
		}
		
		public void Stop()
		{
			if (!running)
				return;
			
			running = false;
			logger.Info("UPnP shutting down");
			NatUtility.StopDiscovery ();
			
			lock (devices)
			foreach (INatDevice device in devices)
			{
				try
				{
					device.DeletePortMap(tcpMapping);
					logger.Info("UPnP port map removal successful {0}", tcpMapping);
					device.DeletePortMap(udpMapping);
					logger.Info("UPnP port map removal successful {0}", udpMapping);
				} 
				catch(MappingException e)
				{
					logger.Error ("UPnP failed to remove map. Error: {0}",e);
				}
			}
		}
		
		public void MapPort()
		{
			logger.Info("UPnP attempting to map port {0}", MappedPort);
			lock (devices)
			foreach (INatDevice device in devices)
			{
				try
				{
					device.BeginCreatePortMap (tcpMapping, EndMapTcpPort, device);
				}
				catch (Exception)
				{
					logger.Info("Failed to map port {0} on {1}", tcpMapping, device);
				}
			}
		}
		
		private void EndMapTcpPort(IAsyncResult result)
		{
			try
			{
				INatDevice device = (INatDevice)result.AsyncState;
				device.EndCreatePortMap(result);
				logger.Info("UPnP port mapping successful {0}", tcpMapping);
				device.BeginCreatePortMap (udpMapping, EndMapUdpPort, device);
			} 
			catch(MappingException e)
			{
				logger.Error("UPnP failed to map port {0}. Error {1}", tcpMapping, e);
			}
		}
		
		private void EndMapUdpPort(IAsyncResult result)
		{		
			try
			{
				INatDevice device = (INatDevice)result.AsyncState;
				device.EndCreatePortMap(result);
				logger.Info("UPnP port mapping successful {0}", udpMapping);
			} 
			catch(MappingException e)
			{
				logger.Error("UPnP failed to map port {0}. Error {1}", udpMapping, e);
			}
			
		}
		
		public void RemoveMap()
		{
			logger.Info("UPnP attempting to remove port map {0}", tcpMapping);
			lock (devices)
			foreach (INatDevice device in devices)
			{
				try
				{
					device.BeginDeletePortMap(tcpMapping, EndRemoveTcpMap, device);
				}
				catch (MappingException e)
				{
					logger.Error("UPnP failed to map port {0}. Error: {1}", tcpMapping, e);
				}
			}
		}
		
		private void EndRemoveTcpMap(IAsyncResult result)
		{
			try
			{
				INatDevice device = (INatDevice)result.AsyncState;
				device.EndDeletePortMap(result);
				logger.Info("UPnP successfully removed port map {0}", tcpMapping);
				device.BeginDeletePortMap(udpMapping, EndRemoveUdpMap, device);
			}
			catch(MappingException e)
			{
				logger.Error("UPnP failed to remove map port {0}. Error: {1}", tcpMapping, e);
			}
		}
		
		private void EndRemoveUdpMap(IAsyncResult result)
		{
			try
			{
				INatDevice device = (INatDevice)result.AsyncState;
				device.EndDeletePortMap(result);
				logger.Info("UPnP successfully removed port map {0}", udpMapping);
			}
			catch(MappingException e)
			{
				logger.Error("UPnP failed to remove map port {0}. Error: {1}", udpMapping, e);
			}
		}
		
		private void OnDeviceFound(object sender, DeviceEventArgs args)
		{
			logger.Info("UPnP Device found");
			lock (devices)
			if (!devices.Contains(args.Device))
				devices.Add(args.Device);
			MapPort();
		}
		
		private bool running;
		public bool IsRunning
		{
			get { return running; }
		}
		
		public int MappedPort
		{
			get { return tcpMapping.PublicPort; }
		}
	}
}
