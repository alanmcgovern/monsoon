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
			if(devices.Count == 0)
				return;
				
			logger.Info("UPnP shutting down");
			running = false;
			foreach (INatDevice device in devices)
			{
				try
				{
					device.DeletePortMap(map);
					logger.Info("UPnP port map removal successful("+ map.Protocol + "/" + MappedPort + ")");
				} 
				catch(MappingException e)
				{
					logger.Error("UPnP failed to remove map(" + map.Protocol + "/" +  MappedPort + ")" + "Error: " + e.ErrorCode + " " + e.ErrorText);
				}
			}
		}
		
		public void MapPort()
		{
			if(devices.Count == 0)
				return;
			
			logger.Info("UPnP attempting to map port("+ map.Protocol + "/" + MappedPort + ")");
			foreach (INatDevice device in devices)
			{
				try
				{
					device.BeginCreatePortMap (map, EndMapPort, map);
				}
				catch (Exception ex)
				{
					logger.Info("Failed to map port {0} on {1}", map, device);
				}
			}
			
		}
		
		private void EndMapPort(IAsyncResult result)
		{
			if(devices.Count == 0)
				return;
			foreach (INatDevice device in devices)
			{
				try
				{
					device.EndCreatePortMap(result);
					logger.Info("UPnP port mapping successful("+ map.Protocol + "/" + MappedPort + ")");
				} 
				catch(MappingException e)
				{
					logger.Error("UPnP failed to map port(" + map.Protocol + "/" + MappedPort + ")" + "Error: " + e.ErrorCode + " " + e.ErrorText);
				}
			}
		}
		
		public void RemoveMap()
		{
			if(devices.Count == 0)
				return;
			
			logger.Info("UPnP attempting to remove port map(" + map.Protocol + "/" + MappedPort + ")");
			foreach (INatDevice device in devices)
			{
				try
				{
					device.BeginDeletePortMap(map, EndRemoveMap, map);
				}
				catch (MappingException e)
				{
					logger.Error("UPnP failed to map port(" + map.Protocol + "/" + MappedPort + ")" + "Error: " + e.ErrorCode + " " + e.ErrorText);
				}
			}
		}
		
		private void EndRemoveMap(IAsyncResult result)
		{
			foreach (INatDevice device in devices)
			{
				try
				{
					device.EndDeletePortMap(result);
					logger.Info("UPnP successfully removed port map(" + map.Protocol + "/" + MappedPort + ")");
				}
				catch(MappingException e)
				{
					logger.Error("UPnP failed to remove map port(" + map.Protocol + "/" + MappedPort + ")" + "Error: " + e.ErrorCode + " " + e.ErrorText);
				}
			}
		}
		
		private void OnDeviceFound(object sender, DeviceEventArgs args)
		{
			logger.Info("UPnP Device found");
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
