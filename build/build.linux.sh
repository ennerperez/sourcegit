#!/bin/sh

rm -rf SourceGit
dotnet publish ../src/Updater/Updater.csproj -c Release -r linux-x64 -o SourceGit -p:PublishAot=true -p:PublishTrimmed=true -p:TrimMode=link --self-contained
dotnet publish ../src/SourceGit/SourceGit.csproj -c Release -r linux-x64 -o SourceGit -p:PublishAot=true -p:PublishTrimmed=true -p:TrimMode=link --self-contained
cp resources/SourceGit.desktop.template SourceGit/SourceGit.desktop.template
cp resources/App.icns SourceGit/SourceGit.icns
tar -zcvf SourceGit.linux-x64.tar.gz --exclude="*/en" --exclude="*/zh" --exclude="*/*.dbg" SourceGit
rm -rf SourceGit
