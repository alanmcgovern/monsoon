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

using Nat;

namespace Monsoon
{
	public class ListenPortController
	{
		private UserEngineSettings engineSettings;
		
		private NatController controller;
		private INatDevice device;
		private Mapping map;
		
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		public ListenPortController(UserEngineSettings engineSettings)
		{
			device = null;
			this.engineSettings = engineSettings;
			map = new Mapping(engineSettings.ListenPort, Protocol.Tcp);
			controller = new NatController();

			controller.DeviceFound += OnDeviceFound;
			
		}
		
		public void Start()
		{
			logger.Info("UPnP starting...");
			controller.Start();
		}
		
		public void ChangePort()
		{
			logger.Info("UPnP changing port map");
			RemoveMap();
			map.Port = engineSettings.ListenPort;
			MapPort();
		}
		
		public void Stop()
		{
			if(device == null)
				return;
				
			logger.Info("UPnP shutting down");
			try{
				device.DeletePortMap(map);
				logger.Info("UPnP port map removal successful("+ map.Protocol + "/" + map.Port + ")");
			} catch(MappingException e){
				logger.Error("UPnP failed to remove map(" + map.Protocol + "/" + map.Port + ")" + "Error: " + e.ErrorCode + " " + e.ErrorText);
			}
		}
		
		public void MapPort()
		{
			if(device == null)
				return;
				
			logger.Info("UPnP attempting to map port("+ map.Protocol + "/" + map.Port + ")");
			device.BeginCreatePortMap(map, "Monsoon", EndMapPort, map);
		}
		
		private void EndMapPort(IAsyncResult result)
		{
			if(device == null)
				return;
			try{				
				device.EndCreatePortMap(result);
				logger.Info("UPnP port mapping successful("+ map.Protocol + "/" + map.Port + ")");
			} catch(MappingException e){
				logger.Error("UPnP failed to map port(" + map.Protocol + "/" + map.Port + ")" + "Error: " + e.ErrorCode + " " + e.ErrorText);
			}
		}
		
		public void RemoveMap()
		{
			if(device == null)
				return;
			
			logger.Info("UPnP attempting to remove port map(" + map.Protocol + "/" + map.Port + ")");
			device.BeginDeletePortMap(map, EndRemoveMap, map);
		}
		
		private void EndRemoveMap(IAsyncResult result)
		{
			if(device == null)
				return;
			try{
				device.EndDeletePortMap(result);
				logger.Info("UPnP successfully removed port map(" + map.Protocol + "/" + map.Port + ")");
			} catch(MappingException e){
				logger.Error("UPnP failed to remove map port(" + map.Protocol + "/" + map.Port + ")" + "Error: " + e.ErrorCode + " " + e.ErrorText);
			}
		}
		
		private void OnDeviceFound(object sender, Nat.DeviceEventArgs args)
		{
			logger.Info("UPnP Device found");
			// FIXME: What happens if more then one device is found? Yeeek, bad news
			device = args.Device;
			MapPort();
		}
		
		public bool IsRunning
		{
			get { return controller.IsRunning; }
		}
		
		public int MappedPort
		{
			get { return map.Port; }
		}
	}
}
