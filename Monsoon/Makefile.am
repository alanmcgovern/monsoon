
EXTRA_DIST = $(NLOG_DLL_SOURCE)

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG
ASSEMBLY = bin/Debug/Monsoon.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

MONSOON_EXE_CONFIG_SOURCE=Monsoon.exe.config
HIGH_PNG_SOURCE=icons/high.png
HIGHEST_PNG_SOURCE=icons/highest.png
IMMEDIATE_PNG_SOURCE=icons/immediate.png
RSS_PNG_SOURCE=icons/rss.png
LOWEST_PNG_SOURCE=icons/lowest.png
MONSOON_DESKTOP_SOURCE=monsoon.desktop
DONOTDOWNLOAD_PNG_SOURCE=icons/donotdownload.png
NORMAL_PNG_SOURCE=icons/normal.png
NLOG_DLL_SOURCE=NLog.dll
LOW_PNG_SOURCE=icons/low.png

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+
ASSEMBLY = bin/Release/Monsoon.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = exe
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release

MONSOON_EXE_CONFIG_SOURCE=Monsoon.exe.config
HIGH_PNG_SOURCE=icons/high.png
HIGHEST_PNG_SOURCE=icons/highest.png
IMMEDIATE_PNG_SOURCE=icons/immediate.png
RSS_PNG_SOURCE=icons/rss.png
LOWEST_PNG_SOURCE=icons/lowest.png
MONSOON_DESKTOP_SOURCE=monsoon.desktop
DONOTDOWNLOAD_PNG_SOURCE=icons/donotdownload.png
NORMAL_PNG_SOURCE=icons/normal.png
NLOG_DLL_SOURCE=NLog.dll
LOW_PNG_SOURCE=icons/low.png

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=.resources.dll

UNSTABLE_LIBS = ./libs/MonoTorrent.dll \
		./libs/Mono.Nat.dll

PROGRAMFILES = \
	$(MONSOON_EXE_CONFIG) \
	$(NLOG_DLL) \
	$(UNSTABLE_LIBS)

LINUX_DESKTOPAPPLICATIONS = \
	$(MONSOON_DESKTOP)  

PROGRAMFILES_ICONS = \
	$(HIGH_PNG) \
	$(HIGHEST_PNG) \
	$(IMMEDIATE_PNG) \
	$(RSS_PNG) \
	$(LOWEST_PNG) \
	$(DONOTDOWNLOAD_PNG) \
	$(NORMAL_PNG) \
	$(LOW_PNG)  

BINARIES = \
	$(MONSOON)  


	
all: update-libs $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_DESKTOPAPPLICATIONS) $(PROGRAMFILES_ICONS) $(BINARIES) 
	cp ./libs/MonoTorrent.dll ./bin/Release/MonoTorrent.dll
	cp ./libs/Mono.Nat.dll ./bin/Release/Mono.Nat.dll

./libs/Mono.Nat.dll: update-libs
./libs/MonoTorrent.dll: update-libs

update-libs:
	mkdir -p ./libs
	cp `pkg-config --variable=Libraries monotorrent` ./libs/MonoTorrent.dll
	cp `pkg-config --variable=Libraries mono.nat` ./libs/Mono.Nat.dll


FILES =  \
	AboutDialog.cs \
	AssemblyInfo.cs \
	ByteConverter.cs \
	CellRendererPiece.cs \
	CreateTorrentDialog.cs \
	CreateTorrentProgressDialog.cs \
	Dbus/CommandsReceivedEventArgs.cs \
	Dbus/DBusInstance.cs \
	Dbus/ICommandParser.cs \
	Defines.cs \
	EditColumnsDialog.cs \
	FileTreeView.cs \
	gtk-gui/generated.cs \
	gtk-gui/Monsoon.CreateTorrentDialog.cs \
	gtk-gui/Monsoon.CreateTorrentProgressDialog.cs \
	gtk-gui/Monsoon.EditColumnsDialog.cs \
	gtk-gui/Monsoon.LoadTorrentDialog.cs \
	gtk-gui/Monsoon.MainWindow.cs \
	gtk-gui/Monsoon.PreferencesDialog.cs \
	gtk-gui/Monsoon.RssManagerDialog.cs \
	gtk-gui/Monsoon.UnhandledExceptionDialog.cs \
	LabelTreeView.cs \
	libs/NotifySharp/Global.cs \
	libs/NotifySharp/Notification.cs \
	libs/TrayLib.cs \
	ListenPortController.cs \
	LoadTorrentDialog.cs \
	Main.cs \
	MainWindow.cs \
	Menus/SpeedLimitMenu.cs \
	Menus/SpeedMenuItem.cs \
	Model/TorrentFileModel.cs \
	PeerTreeView.cs \
	PiecesTreeView.cs \
	PreferencesDialog.cs \
	RssManager/RssFilter.cs \
	RssManager/RssManagerController.cs \
	RssManager/RssManagerDialog.cs \
	RssManager/RssReader.cs \
	RssManager/TorrentRssWatcher.cs \
	RssManager/TorrentRssWatcherEventArgs.cs \
	Settings/GconfEngineSettingsController.cs \
	Settings/GConfInterfaceSettingsController.cs \
	Settings/GconfPreferencesSettingsController.cs \
	Settings/GconfSettingsStorage.cs \
	Settings/GconfTorrentSettingsController.cs \
	Settings/InterfaceSettings.cs \
	Settings/PreferencesSettings.cs \
	Settings/SettingNotFoundException.cs \
	Settings/SettingsController.cs \
	Settings/XmlRssFeedsController.cs \
	Settings/XmlRssFiltersController.cs \
	Settings/XmlRssHistoryController.cs \
	Settings/XmlTorrentLabelController.cs \
	Settings/XmlTorrentStorageController.cs \
	Ticker.cs \
	TorrentContextMenu.cs \
	TorrentController.cs \
	TorrentFileSettingsModel.cs \
	TorrentFilterModel.cs \
	TorrentLabel.cs \
	TorrentStorage.cs \
	TorrentTreeView.cs \
	UnhandledExceptionDialog.cs \
	View/NatWidget.cs

DATA_FILES = \
	icons/rss.png \
	icons/donotdownload.png \
	icons/high.png \
	icons/highest.png \
	icons/immediate.png \
	icons/low.png \
	icons/lowest.png \
	icons/normal.png \
	monsoon.desktop \
	Monsoon.exe.config 

RESOURCES = \
	gtk-gui/gui.stetic \
	gtk-gui/objects.xml 

EXTRAS = \
	monsoon.in 

REFERENCES =  \
	System \
	System.Xml \
	$(NDESK_DBUS_GLIB_10_LIBS) \
	Mono.Posix \
	$(GCONF_SHARP_20_LIBS) \
	$(GTK_SHARP_20_LIBS) \
	$(GLIB_SHARP_20_LIBS)

DLL_REFERENCES =  \
		$(NLOG_DLL) \
		./libs/Mono.Nat.dll \
		./libs/MonoTorrent.dll

CLEANFILES = $(PROGRAMFILES) $(LINUX_DESKTOPAPPLICATIONS) $(PROGRAMFILES_ICONS) $(BINARIES) $(UNSTABLE_LIBS)

include $(top_srcdir)/Makefile.include

MONSOON_EXE_CONFIG = $(BUILD_DIR)/Monsoon.exe.config
HIGH_PNG = $(BUILD_DIR)/icons/high.png
HIGHEST_PNG = $(BUILD_DIR)/icons/highest.png
IMMEDIATE_PNG = $(BUILD_DIR)/icons/immediate.png
RSS_PNG = $(BUILD_DIR)/icons/rss.png
LOWEST_PNG = $(BUILD_DIR)/icons/lowest.png
MONSOON_DESKTOP = $(BUILD_DIR)/monsoon.desktop
DONOTDOWNLOAD_PNG = $(BUILD_DIR)/icons/donotdownload.png
MONSOON = $(BUILD_DIR)/monsoon
NORMAL_PNG = $(BUILD_DIR)/icons/normal.png
NLOG_DLL = $(BUILD_DIR)/NLog.dll
LOW_PNG = $(BUILD_DIR)/icons/low.png

$(eval $(call emit-deploy-target,MONSOON_EXE_CONFIG))
$(eval $(call emit-deploy-target,HIGH_PNG))
$(eval $(call emit-deploy-target,HIGHEST_PNG))
$(eval $(call emit-deploy-target,IMMEDIATE_PNG))
$(eval $(call emit-deploy-target,RSS_PNG))
$(eval $(call emit-deploy-target,LOWEST_PNG))
$(eval $(call emit-deploy-target,MONSOON_DESKTOP))
$(eval $(call emit-deploy-target,DONOTDOWNLOAD_PNG))
$(eval $(call emit-deploy-wrapper,MONSOON,monsoon,x))
$(eval $(call emit-deploy-target,NORMAL_PNG))
$(eval $(call emit-deploy-target,NLOG_DLL))
$(eval $(call emit-deploy-target,LOW_PNG))


$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(build_resx_resources) : %.resources: %.resx
	resgen2 '$<' '$@'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(dir $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
