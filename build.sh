#!/bin/bash

dotnet publish -c Release -r osx-x64 -p:PublishReadyToRun=true --self-contained
dotnet publish -c Release -r win-x64 -p:PublishReadyToRun=true --self-contained
dotnet publish -c Release -r linux-x64 -p:PublishReadyToRun=true --self-contained

cd "bin/"

#mkdir -p "TouchPortalMuteMePlugin"
mkdir -p "TouchPortalMuteMePlugin/osx"
mkdir -p "TouchPortalMuteMePlugin/win"
mkdir -p "TouchPortalMuteMePlugin/linux"

cp -R "Release/net6.0/osx-x64/publish/" "TouchPortalMuteMePlugin/osx/"
cp -R "Release/net6.0/win-x64/publish/" "TouchPortalMuteMePlugin/win/"
cp -R "Release/net6.0/linux-x64/publish/" "TouchPortalMuteMePlugin/linux/"

cp "Release/net6.0/osx-x64/publish/icon24.png" "TouchPortalMuteMePlugin/"
cp "Release/net6.0/osx-x64/publish/entry.tp" "TouchPortalMuteMePlugin/"

zip -q -r "../Touch_Portal_MuteMe_Plugin.tpp" "TouchPortalMuteMePlugin" -x "*.DS_Store"
rm -R "TouchPortalMuteMePlugin"

cd ".."