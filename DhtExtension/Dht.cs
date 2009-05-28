
using System;
using System.Net;
using MonoTorrent;
using MonoTorrent.Common;
using MonoTorrent.Dht;
using MonoTorrent.Dht.Listeners;
using Mono.Addins;
using Monsoon;
using Gtk;

namespace DhtExtension
{
	[Extension ("/monsoon/dht")]
	public class Dht : Monsoon.IDhtExtension
	{
		Label label = new Label ();
		public event EventHandler<PeersFoundEventArgs> PeersFound {
			add { engine.PeersFound += value; }
			remove { engine.PeersFound -= value; }
		}
		public event EventHandler StateChanged {
			add { engine.StateChanged += value; }
			remove { engine.StateChanged -= value; }
		}

		DhtEngine engine;
		DhtListener listener;
		
		public Dht ()
		{
			listener = new DhtListener (new IPEndPoint (IPAddress.Any, 35800));
			engine = new DhtEngine (listener);
			engine.StateChanged += delegate { UpdateText (); };
			GLib.Timeout.Add (5000, delegate {
				UpdateText ();
				return true;
			});
		}

		void UpdateText ()
		{
			string status;
			switch (engine.State) {
				case DhtState.Initialising:
					status = "Initialising";
					break;
				case DhtState.NotReady:
					status = "NotReady";
					break;
				case DhtState.Ready:
					status = "Ready";
					break;
				default:
					status = "";
					break;
			}
			int nodes = (int) engine.SaveNodes ().Length / 20;
			// FIXME: Hardcoded zero here for the moment, need to get a node count
			label.Text = string.Format ("Dht Status: {0}. Nodes: {1}", status, nodes);
		}

		public byte[] SaveNodes()
		{
			return engine.SaveNodes ();
		}

		public void Announce(InfoHash infohash, int port)
		{
			engine.Announce (infohash, port);
		}

		public void Dispose ()
		{
			engine.Dispose ();
		}
		
		public void GetPeers(InfoHash infohash)
		{
			engine.GetPeers (infohash);
		}

		public ToolItem GetWidget ()
		{
			UpdateText ();
			ToolItem item = new ToolItem ();
			HBox box = new HBox ();
			box.Add (label);
			item.Child = box;
			return item;
		}
		
		public void Start()
		{
			TorrentController c = ServiceManager.Get <TorrentController> ();
			listener.ChangeEndpoint (new IPEndPoint (IPAddress.Any, c.Engine.Settings.ListenPort));
			engine.Start ();
		}
		
		public void Start(byte[] initialNodes)
		{
			TorrentController c = ServiceManager.Get <TorrentController> ();
			listener.ChangeEndpoint (new IPEndPoint (IPAddress.Any, c.Engine.Settings.ListenPort));
			engine.Start (initialNodes);
		}

		public void Stop()
		{
			engine.Stop ();
		}

		public bool Disposed { 
			get { return engine.Disposed; }
		}

		public DhtState State {
			get { return engine.State; }
		}
	}
}
