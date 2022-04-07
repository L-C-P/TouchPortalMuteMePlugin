#!/bin/bash

dotnet publish -c Release -r osx-x64 -p:PublishReadyToRun=true --self-contained
dotnet publish -c Release -r win-x64 -p:PublishReadyToRun=true --self-contained
dotnet publish -c Release -r linux-x64 -p:PublishReadyToRun=true --self-contained

mkdir "Release"

cd "bin/"

cp -R "Release/net6.0/osx-x64/publish" "TouchPortalMuteMePlugin"
zip -q -r "../Release/OSX_Touch_Portal_MuteMe_Plugin.tpp" "TouchPortalMuteMePlugin" -x "*.DS_Store"
rm -R "TouchPortalMuteMePlugin"

cp -R "Release/net6.0/win-x64/publish" "TouchPortalMuteMePlugin"
zip -q -r "../Release/WIN_Touch_Portal_MuteMe_Plugin.tpp" "TouchPortalMuteMePlugin" -x "*.DS_Store"
rm -R "TouchPortalMuteMePlugin"

cp -R "Release/net6.0/linux-x64/publish" "TouchPortalMuteMePlugin"
zip -q -r "../Release/LINUX_Touch_Portal_MuteMe_Plugin.tpp" "TouchPortalMuteMePlugin" -x "*.DS_Store"
rm -R "TouchPortalMuteMePlugin"

cd ".."