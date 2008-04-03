// XmlInterfaceSettingsController.cs created with MonoDevelop
// User: alan at 23:16 03/04/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;
using System.Xml.Serialization;

namespace Monsoon
{
	public class GConfInterfaceSettingsController : SettingsController<InterfaceSettings>
	{
		private static string SETTINGS_PATH = "InterfaceSettings/";

		public override void Load ()
		{
			GconfSettingsStorage gconf = GconfSettingsStorage.Instance;
			
			try {
				Settings.ShowDetails = (bool) gconf.Retrieve(SETTINGS_PATH + "showDetails");
			} catch(SettingNotFoundException) {
				Settings.ShowDetails = true;
			}
			
			try {
				Settings.ShowLabels = (bool) gconf.Retrieve(SETTINGS_PATH + "showLabels");
			} catch(SettingNotFoundException) {
				Settings.ShowLabels = true;
			}
			
			try {
				Settings.WindowHeight = (int) gconf.Retrieve(SETTINGS_PATH + "windowHeight");
			} catch(SettingNotFoundException) {
				Settings.WindowHeight = 480;
			}
			
			try {
				Settings.WindowWidth = (int) gconf.Retrieve(SETTINGS_PATH + "windowWidth");
			} catch(SettingNotFoundException) {
				Settings.WindowWidth = 640;
			}
			
			try { 
				Settings.VPaned = (int) gconf.Retrieve(SETTINGS_PATH + "vPaned");
			} catch(SettingNotFoundException) {
				Settings.VPaned = 145; 
			}
			
			try {
				Settings.HPaned = (int) gconf.Retrieve(SETTINGS_PATH + "hPaned");
			} catch(SettingNotFoundException) {
				Settings.HPaned = 160;
			}
			
			try {
				Settings.WindowYPos = (int) gconf.Retrieve(SETTINGS_PATH + "windowYPos");
			} catch(SettingNotFoundException) {
				Settings.WindowYPos = 0;
			}
			
			try{
				Settings.WindowXPos = (int) gconf.Retrieve(SETTINGS_PATH + "windowXPos");
			} catch(SettingNotFoundException) {
				Settings.WindowXPos = 0;
			}
			
			// Restore column order/width/visibility
			try{
				Settings.NameColumnWidth = (int) gconf.Retrieve(SETTINGS_PATH + "Columns/Name/Width");
			} catch(SettingNotFoundException){
				Settings.NameColumnWidth = 75;
			}
			
			try{
				Settings.NameColumnVisible = (bool) gconf.Retrieve(SETTINGS_PATH + "Columns/Name/Visible");
			} catch(SettingNotFoundException){
				Settings.NameColumnVisible = true;
			}
			
			try{
				Settings.StatusColumnWidth = (int) gconf.Retrieve(SETTINGS_PATH + "Columns/Status/Width");
			} catch(SettingNotFoundException){
				Settings.StatusColumnWidth = 75;
			}
			
			try{
				Settings.StatusColumnVisible = (bool) gconf.Retrieve(SETTINGS_PATH + "Columns/Status/Visible");
			} catch(SettingNotFoundException){
				Settings.StatusColumnVisible = true;
			}
			
			try{
				Settings.DoneColumnWidth = (int) gconf.Retrieve(SETTINGS_PATH + "Columns/Done/Width");
			} catch(SettingNotFoundException){
				Settings.DoneColumnWidth = 75;
			}
			
			try{
				Settings.DoneColumnVisible = (bool) gconf.Retrieve(SETTINGS_PATH + "Columns/Done/Visible");
			} catch(SettingNotFoundException){
				Settings.DoneColumnVisible = true;
			}
			
			try{
				Settings.SeedsColumnWidth = (int) gconf.Retrieve(SETTINGS_PATH + "Columns/Seeds/Width");
			} catch(SettingNotFoundException){
				Settings.SeedsColumnWidth = 75;
			}
			
			try{
				Settings.SeedsColumnVisible = (bool) gconf.Retrieve(SETTINGS_PATH + "Columns/Seeds/Visible");
			} catch(SettingNotFoundException){
				Settings.SeedsColumnVisible = true;
			}
			
			try{
				Settings.PeersColumnWidth = (int) gconf.Retrieve(SETTINGS_PATH + "Columns/Peers/Width");
			} catch(SettingNotFoundException){
				Settings.PeersColumnWidth = 75;
			}
			
			try{
				Settings.PeersColumnVisible = (bool) gconf.Retrieve(SETTINGS_PATH + "Columns/Peers/Visible");
			} catch(SettingNotFoundException){
				Settings.PeersColumnVisible = true;
			}
			
			try{
				Settings.DlSpeedColumnWidth = (int) gconf.Retrieve(SETTINGS_PATH + "Columns/DLSpeed/Width");
			} catch(SettingNotFoundException){
				Settings.DlSpeedColumnWidth = 75;
			}
			
			try{
				Settings.DlSpeedColumnVisible = (bool) gconf.Retrieve(SETTINGS_PATH + "Columns/DLSpeed/Visible");
			} catch(SettingNotFoundException){
				Settings.DlSpeedColumnVisible = true;
			}
			
			try{
				Settings.UpSpeedColumnWidth = (int) gconf.Retrieve(SETTINGS_PATH + "Columns/UPSpeed/Width");
			} catch(SettingNotFoundException){
				Settings.UpSpeedColumnWidth = 75;
			}
			
			try{
				Settings.UpSpeedColumnVisible = (bool) gconf.Retrieve(SETTINGS_PATH + "Columns/UPSpeed/Visible");
			} catch(SettingNotFoundException){
				Settings.UpSpeedColumnVisible = true;
			}
			
			try{
				Settings.RatioColumnWidth = (int) gconf.Retrieve(SETTINGS_PATH + "Columns/Ratio/Width");
			} catch(SettingNotFoundException){
				Settings.RatioColumnWidth = 75;
			}
			
			try{
				Settings.RatioColumnVisible = (bool) gconf.Retrieve(SETTINGS_PATH + "Columns/Ratio/Visible");
			} catch(SettingNotFoundException){
				Settings.RatioColumnVisible = true;
			}
			
			try{
				Settings.SizeColumnWidth = (int) gconf.Retrieve(SETTINGS_PATH + "Columns/Size/Width");
			} catch(SettingNotFoundException){
				Settings.SizeColumnWidth = 75;
			}
			
			try{
				Settings.SizeColumnVisible = (bool) gconf.Retrieve(SETTINGS_PATH + "Columns/Size/Visible");
			} catch(SettingNotFoundException){
				Settings.SizeColumnVisible = true;
			}
		}

		public override void Save ()
		{
			GconfSettingsStorage gconf = GconfSettingsStorage.Instance;
			
			gconf.Store(SETTINGS_PATH + "showDetails", Settings.ShowDetails);
			gconf.Store(SETTINGS_PATH + "showLabels", Settings.ShowLabels);
			
			gconf.Store(SETTINGS_PATH + "windowHeight", Settings.WindowHeight);
			gconf.Store(SETTINGS_PATH + "windowWidth", Settings.WindowWidth);
			gconf.Store(SETTINGS_PATH + "vPaned", Settings.VPaned);
			gconf.Store(SETTINGS_PATH + "hPaned", Settings.HPaned);
			gconf.Store(SETTINGS_PATH + "windowXPos", Settings.WindowXPos);
			gconf.Store(SETTINGS_PATH + "windowYPos", Settings.WindowYPos);
			
			// Columns
			gconf.Store(SETTINGS_PATH + "Columns/Name/Width", Settings.NameColumnWidth);
			gconf.Store(SETTINGS_PATH + "Columns/Name/Visible", Settings.NameColumnVisible);
			gconf.Store(SETTINGS_PATH + "Columns/Status/Width", Settings.StatusColumnWidth);
			gconf.Store(SETTINGS_PATH + "Columns/Status/Visible", Settings.StatusColumnVisible);
			gconf.Store(SETTINGS_PATH + "Columns/Done/Width", Settings.DoneColumnWidth);
			gconf.Store(SETTINGS_PATH + "Columns/Done/Visible", Settings.DoneColumnVisible);
			gconf.Store(SETTINGS_PATH + "Columns/Seeds/Width", Settings.SeedsColumnWidth);
			gconf.Store(SETTINGS_PATH + "Columns/Seeds/Visible", Settings.SeedsColumnVisible);
			gconf.Store(SETTINGS_PATH + "Columns/Peers/Width", Settings.SeedsColumnWidth);
			gconf.Store(SETTINGS_PATH + "Columns/Peers/Visible", Settings.PeersColumnVisible);
			gconf.Store(SETTINGS_PATH + "Columns/DLSpeed/Width", Settings.DlSpeedColumnWidth);
			gconf.Store(SETTINGS_PATH + "Columns/DLSpeed/Visible", Settings.DlSpeedColumnVisible);
			gconf.Store(SETTINGS_PATH + "Columns/UPSpeed/Width", Settings.UpSpeedColumnWidth);
			gconf.Store(SETTINGS_PATH + "Columns/UPSpeed/Visible", Settings.UpSpeedColumnVisible);
			gconf.Store(SETTINGS_PATH + "Columns/Ratio/Width", Settings.RatioColumnWidth);
			gconf.Store(SETTINGS_PATH + "Columns/Ratio/Visible", Settings.RatioColumnVisible);
			gconf.Store(SETTINGS_PATH + "Columns/Size/Width", Settings.SizeColumnWidth);
			gconf.Store(SETTINGS_PATH + "Columns/Size/Visible", Settings.SizeColumnVisible);
		}
	}
}
