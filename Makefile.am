
EXTRA_DIST = 	expansions.m4 \
		intltool-update.in \
		intltool-extract.in \
		intltool-merge.in

SUBDIRS = \		
	Monsoon \
	po

run:
	mono --debug Monsoon/bin/Release/Monsoon.exe -d

run-clean: all
	rm -rf ~/.config/monsoon
	make run

make run-profile:
	mono --profile=logging:s=8 Monsoon/bin/Release/Monsoon.exe -d
