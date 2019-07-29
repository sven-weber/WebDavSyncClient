# WebDavSyncClient

A cross-platform client for WebDav Synchronisation written in .net Core. 
The Client supports Mac, Linux and Windows but unfortunately is currently only tested in Linux. 

The plan is to run the synchronisation as a background service in the future. 

### This Project is in alpha state and in active development

Hopefully I will be able to release the first version soon. 


## Build 

### Prerequisites
In oder to build this app the following prequisites are needed:

- .Net core sdk version >= 2.1

A description on how to install the .net core sdk can be found [here](https://dotnet.microsoft.com/download).
Its available for Windows, Mac and Linux.

### Build Script
All steps required for the build can be found in the included Cake build script (see [here](https://cakebuild.net)). The script is located in the ".\build" folder and is available for 
bash ("build.sh") and powershell ("build.ps1"). All requirements for the build will automatically
be downloaded when the script is executed. The build output will be located at ".\src\bin\Release\"