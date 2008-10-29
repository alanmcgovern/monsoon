
EXTRA_DIST = 	expansions.m4 \
		Monsoon/NLog.dll \
		bitsharp/MonoTorrent/bin/Release/MonoTorrent.dll \
		mono.nat/Mono.Nat/bin/Release/Mono.Nat.dll \
		intltool-update.in \
		intltool-extract.in \
		intltool-merge.in

SUBDIRS = 	bitsharp \
		mono.nat \		
		Monsoon \
		po
run: all
	mono --debug Monsoon/bin/Release/Monsoon.exe
run-clean: all
	rm -rf ~/.config/monsoon
	make run
