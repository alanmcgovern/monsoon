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

namespace Monsoon
{
	public class ListenPortController
	{
		private UserEngineSettings settings;
		
		private List<INatDevice> devices;
		private Mapping map;
		
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		public ListenPortController(UserEngineSettings engineSettings)
		{
			settings = engineSettings;
			
			devices = new List<INatDevice> ();
			map = new Mapping (Protocol.Tcp, engineSettings.ListenPort, engineSettings.ListenPort);
			map.Description = "Monsoon";
			
			IPAddress[] addresses = NatUtility.GetLocalAddresses (false);
			
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
			map = new Mapping (map.Protocol, settings.ListenPort, settings.ListenPort);
			map.Description = "Monsoon";
			MapPort();
		}
		
		public void Stop()
		{
			running = false;
			logger.Info("UPnP shutting down");
			NatUtility.StopDiscovery ();
			
			lock (devices)
			foreach (INatDevice device in devices)
			{
				try
				{
					device.DeletePortMap(map);
					logger.Info("UPnP port map removal successful {0}", map);
				} 
				catch(MappingException e)
				{
					logger.Error ("UPnP failed to remove map {0}. Error: {1}", map, e);
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
					device.BeginCreatePortMap (map, EndMapPort, device);
				}
				catch (Exception ex)
				{
					logger.Info("Failed to map port {0} on {1}", map, device);
				}
			}
		}
		
		private void EndMapPort(IAsyncResult result)
		{
			try
			{
				((INatDevice)result.AsyncState).EndCreatePortMap(result);
				logger.Info("UPnP port mapping successful {0}", map);
			} 
			catch(MappingException e)
			{
				logger.Error("UPnP failed to map port {0}. Error {1}", map, e);
			}
		}
		
		public void RemoveMap()
		{
			logger.Info("UPnP attempting to remove port map {0}", map);
			lock (devices)
			foreach (INatDevice device in devices)
			{
				try
				{
					device.BeginDeletePortMap(map, EndRemoveMap, device);
				}
				catch (MappingException e)
				{
					logger.Error("UPnP failed to map port {0}. Error: {1}", map, e);
				}
			}
		}
		
		private void EndRemoveMap(IAsyncResult result)
		{
			try
			{
				((INatDevice)result.AsyncState).EndDeletePortMap(result);
				logger.Info("UPnP successfully removed port map {0}", map);
			}
			catch(MappingException e)
			{
				logger.Error("UPnP failed to remove map port {0}. Error: {1}", map, e);
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
			get { return map.PublicPort; }
		}
	}
}
