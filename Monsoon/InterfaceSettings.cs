// /home/buchan/monotorrent/Monsoon/InterfaceSettings.cs created with MonoDevelop
// User: buchan at 16:03 07/12/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Monsoon
{
	public class InterfaceSettings : ISettings
	{
		private static string SETTINGS_PATH = "InterfaceSettings/";
		
		private int windowHeight;
		private int windowWidth;
		private int windowYPos;
		private int windowXPos;
		private int vPaned;
		private int hPaned;
		private bool showDetails;
		private bool showLabels;
		
		// Columns
		private int nameColumnWidth;
		private int statusColumnWidth;
		private int doneColumnWidth;
		private int seedsColumnWidth;
		private int peersColumnWidth;
		private int dlSpeedColumnWidth;
		private int upSpeedColumnWidth;
		private int ratioColumnWidth;
		private int sizeColumnWidth;
		
		private bool nameColumnVisible;
		private bool statusColumnVisible;
		private bool doneColumnVisible;
		private bool seedsColumnVisible;
		private bool peersColumnVisible;
		private bool dlSpeedColumnVisible;
		private bool upSpeedColumnVisible;
		private bool ratioColumnVisible;
		private bool sizeColumnVisible;

		
		public InterfaceSettings()
		{
			Restore();
		}
		
		public void Restore()
		{
			GconfSettingsStorage gconf = new GconfSettingsStorage();
			
			try {
				showDetails = (bool) gconf.Retrieve(SETTINGS_PATH + "showDetails");
			} catch(SettingNotFoundException) {
				showDetails = true;
			}
			
			try {
				showLabels = (bool) gconf.Retrieve(SETTINGS_PATH + "showLabels");
			} catch(SettingNotFoundException) {
				showLabels = true;
			}
			
			try {
				windowHeight = (int) gconf.Retrieve(SETTINGS_PATH + "windowHeight");
			} catch(SettingNotFoundException) {
				windowHeight = 480;
			}
			
			try {
				windowWidth = (int) gconf.Retrieve(SETTINGS_PATH + "windowWidth");
			} catch(SettingNotFoundException) {
				windowWidth = 640;
			}
			
			try { 
				vPaned = (int) gconf.Retrieve(SETTINGS_PATH + "vPaned");
			} catch(SettingNotFoundException) {
				vPaned = 145; 
			}
			
			try {
				hPaned = (int) gconf.Retrieve(SETTINGS_PATH + "hPaned");
			} catch(SettingNotFoundException) {
				hPaned = 160;
			}
			
			try {
				windowYPos = (int) gconf.Retrieve(SETTINGS_PATH + "windowYPos");
			} catch(SettingNotFoundException) {
				windowYPos = 0;
			}
			
			try{
				windowXPos = (int) gconf.Retrieve(SETTINGS_PATH + "windowXPos");
			} catch(SettingNotFoundException) {
				windowXPos = 0;
			}
			
			// Restore column order/width/visibility
			try{
				nameColumnWidth = (int) gconf.Retrieve(SETTINGS_PATH + "Columns/Name/Width");
			} catch(SettingNotFoundException){
				nameColumnWidth = 75;
			}
			
			try{
				nameColumnVisible = (bool) gconf.Retrieve(SETTINGS_PATH + "Columns/Name/Visible");
			} catch(SettingNotFoundException){
				nameColumnVisible = true;
			}
			
			try{
				statusColumnWidth = (int) gconf.Retrieve(SETTINGS_PATH + "Columns/Status/Width");
			} catch(SettingNotFoundException){
				statusColumnWidth = 75;
			}
			
			try{
				statusColumnVisible = (bool) gconf.Retrieve(SETTINGS_PATH + "Columns/Status/Visible");
			} catch(SettingNotFoundException){
				statusColumnVisible = true;
			}
			
			try{
				doneColumnWidth = (int) gconf.Retrieve(SETTINGS_PATH + "Columns/Done/Width");
			} catch(SettingNotFoundException){
				doneColumnWidth = 75;
			}
			
			try{
				doneColumnVisible = (bool) gconf.Retrieve(SETTINGS_PATH + "Columns/Done/Visible");
			} catch(SettingNotFoundException){
				doneColumnVisible = true;
			}
			
			try{
				seedsColumnWidth = (int) gconf.Retrieve(SETTINGS_PATH + "Columns/Seeds/Width");
			} catch(SettingNotFoundException){
				seedsColumnWidth = 75;
			}
			
			try{
				seedsColumnVisible = (bool) gconf.Retrieve(SETTINGS_PATH + "Columns/Seeds/Visible");
			} catch(SettingNotFoundException){
				seedsColumnVisible = true;
			}
			
			try{
				peersColumnWidth = (int) gconf.Retrieve(SETTINGS_PATH + "Columns/Peers/Width");
			} catch(SettingNotFoundException){
				peersColumnWidth = 75;
			}
			
			try{
				peersColumnVisible = (bool) gconf.Retrieve(SETTINGS_PATH + "Columns/Peers/Visible");
			} catch(SettingNotFoundException){
				peersColumnVisible = true;
			}
			
			try{
				dlSpeedColumnWidth = (int) gconf.Retrieve(SETTINGS_PATH + "Columns/DLSpeed/Width");
			} catch(SettingNotFoundException){
				dlSpeedColumnWidth = 75;
			}
			
			try{
				dlSpeedColumnVisible = (bool) gconf.Retrieve(SETTINGS_PATH + "Columns/DLSpeed/Visible");
			} catch(SettingNotFoundException){
				dlSpeedColumnVisible = true;
			}
			
			try{
				upSpeedColumnWidth = (int) gconf.Retrieve(SETTINGS_PATH + "Columns/UPSpeed/Width");
			} catch(SettingNotFoundException){
				upSpeedColumnWidth = 75;
			}
			
			try{
				upSpeedColumnVisible = (bool) gconf.Retrieve(SETTINGS_PATH + "Columns/UPSpeed/Visible");
			} catch(SettingNotFoundException){
				upSpeedColumnVisible = true;
			}
			
			try{
				ratioColumnWidth = (int) gconf.Retrieve(SETTINGS_PATH + "Columns/Ratio/Width");
			} catch(SettingNotFoundException){
				ratioColumnWidth = 75;
			}
			
			try{
				ratioColumnVisible = (bool) gconf.Retrieve(SETTINGS_PATH + "Columns/Ratio/Visible");
			} catch(SettingNotFoundException){
				ratioColumnVisible = true;
			}
			
			try{
				sizeColumnWidth = (int) gconf.Retrieve(SETTINGS_PATH + "Columns/Size/Width");
			} catch(SettingNotFoundException){
				sizeColumnWidth = 75;
			}
			
			try{
				sizeColumnVisible = (bool) gconf.Retrieve(SETTINGS_PATH + "Columns/Size/Visible");
			} catch(SettingNotFoundException){
				sizeColumnVisible = true;
			}
		}
		
		public void Store()
		{
			GconfSettingsStorage gconf = new GconfSettingsStorage();
			
			gconf.Store(SETTINGS_PATH + "showDetails", showDetails);
			gconf.Store(SETTINGS_PATH + "showLabels", showLabels);
			
			gconf.Store(SETTINGS_PATH + "windowHeight", windowHeight);
			gconf.Store(SETTINGS_PATH + "windowWidth", windowWidth);
			gconf.Store(SETTINGS_PATH + "vPaned", vPaned);
			gconf.Store(SETTINGS_PATH + "hPaned", hPaned);
			gconf.Store(SETTINGS_PATH + "windowXPos", windowXPos);
			gconf.Store(SETTINGS_PATH + "windowYPos", windowYPos);
			
			// Columns
			gconf.Store(SETTINGS_PATH + "Columns/Name/Width", nameColumnWidth);
			gconf.Store(SETTINGS_PATH + "Columns/Name/Visible", nameColumnVisible);
			gconf.Store(SETTINGS_PATH + "Columns/Status/Width", statusColumnWidth);
			gconf.Store(SETTINGS_PATH + "Columns/Status/Visible", statusColumnVisible);
			gconf.Store(SETTINGS_PATH + "Columns/Done/Width", doneColumnWidth);
			gconf.Store(SETTINGS_PATH + "Columns/Done/Visible", doneColumnVisible);
			gconf.Store(SETTINGS_PATH + "Columns/Seeds/Width", seedsColumnWidth);
			gconf.Store(SETTINGS_PATH + "Columns/Seeds/Visible", seedsColumnVisible);
			gconf.Store(SETTINGS_PATH + "Columns/Peers/Width", seedsColumnWidth);
			gconf.Store(SETTINGS_PATH + "Columns/Peers/Visible", peersColumnVisible);
			gconf.Store(SETTINGS_PATH + "Columns/DLSpeed/Width", dlSpeedColumnWidth);
			gconf.Store(SETTINGS_PATH + "Columns/DLSpeed/Visible", dlSpeedColumnVisible);
			gconf.Store(SETTINGS_PATH + "Columns/UPSpeed/Width", upSpeedColumnWidth);
			gconf.Store(SETTINGS_PATH + "Columns/UPSpeed/Visible", upSpeedColumnVisible);
			gconf.Store(SETTINGS_PATH + "Columns/Ratio/Width", ratioColumnWidth);
			gconf.Store(SETTINGS_PATH + "Columns/Ratio/Visible", ratioColumnVisible);
			gconf.Store(SETTINGS_PATH + "Columns/Size/Width", sizeColumnWidth);
			gconf.Store(SETTINGS_PATH + "Columns/Size/Visible", sizeColumnVisible);
		}
		
		public bool ShowDetails {
			get { return showDetails; }
			set { showDetails = value; }
		}
		
		public bool ShowLabels {
			get { return showLabels; }
			set { showLabels = value; }
		}
		
		public int WindowHeight {
			get { return windowHeight; }
			set { windowHeight = value; }
		}
		
		public int WindowWidth {
			get { return windowWidth; }
			set { windowWidth = value; }
		}
		
		public int VPaned {
			get { return vPaned; }
			set { vPaned = value; }
		}
		
		public int HPaned {
			get { return hPaned; }
			set { hPaned = value; }
		}
		
		public int WindowXPos{
			get { return windowXPos; }
			set { windowXPos = value; }
		}
		
		public int WindowYPos{
			get { return windowYPos; }
			set { windowYPos = value; }
		}
		
		public int NameColumnWidth{
			get { return nameColumnWidth; }
			set { nameColumnWidth = value; }
		}
		
		public bool NameColumnVisible{
			get { return nameColumnVisible; }
			set { nameColumnVisible = value; }
		}
		
		public int StatusColumnWidth{
			get { return statusColumnWidth; }
			set { statusColumnWidth = value; }
		}
		
		public bool StatusColumnVisible{
			get { return statusColumnVisible; }
			set { statusColumnVisible = value; }
		}
		
		public int DoneColumnWidth{
			get { return doneColumnWidth; }
			set { doneColumnWidth = value; }
		}
		
		public bool DoneColumnVisible{
			get { return doneColumnVisible; }
			set { doneColumnVisible = value; }
		}
		
		public int SeedsColumnWidth{
			get { return seedsColumnWidth; }
			set { seedsColumnWidth = value; }
		}
		
		public bool SeedsColumnVisible{
			get { return seedsColumnVisible; }
			set { seedsColumnVisible = value; }
		}
		
		public int PeersColumnWidth{
			get { return peersColumnWidth; }
			set { peersColumnWidth = value; }
		}
		
		public bool PeersColumnVisible{
			get { return peersColumnVisible; }
			set { peersColumnVisible = value; }
		}
		
		public int DlSpeedColumnWidth{
			get { return dlSpeedColumnWidth; }
			set { dlSpeedColumnWidth = value; }
		}
		
		public bool DlSpeedColumnVisible{
			get { return dlSpeedColumnVisible; }
			set { dlSpeedColumnVisible = value; }
		}
		
		public int UpSpeedColumnWidth{
			get { return upSpeedColumnWidth; }
			set { upSpeedColumnWidth = value; }
		}
		
		public bool UpSpeedColumnVisible{
			get { return upSpeedColumnVisible; }
			set { upSpeedColumnVisible = value; }
		}
		
		public int RatioColumnWidth{
			get { return ratioColumnWidth; }
			set { ratioColumnWidth = value; }
		}
		
		public bool RatioColumnVisible{
			get { return ratioColumnVisible; }
			set { ratioColumnVisible = value; }
		}
		
		public int SizeColumnWidth{
			get { return sizeColumnWidth; }
			set { sizeColumnWidth = value; }
		}
		
		public bool SizeColumnVisible{
			get { return sizeColumnVisible; }
			set { sizeColumnVisible = value; }
		}
	}
}
