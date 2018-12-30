using Cake.Common.Tools.DotNetCore.DotNetCoreAliases;
using System.Xml.Linq;
using System.Linq;

#addin nuget:?package=SharpZipLib&version=1.1.0
#addin nuget:?package=Cake.Compression&version=0.2.1

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var srcPath = "./../src/";
var binPath = srcPath + "bin/";
var configurationDir =  binPath + Directory(configuration) + "/";
var csProjPath = srcPath + "WebDavSync.csproj";

string projVersion = "";
//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define runtime target platforms and their output directories
Dictionary<string, string> platforms = new Dictionary<string, string>() 
{
    {"linux-x64", configurationDir + "linux"},
};
//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(configurationDir);
});

Task("ObtainProjectVersion")
    .IsDependentOn("Clean")
    .Does(() =>
{
    Information("Starting Obtaining Project version");
    //Read the xml version from the .csproj file
    XDocument doc = XDocument.Load(csProjPath);
    var element = doc.Descendants().FirstOrDefault(x => x.Name == "PropertyGroup");
    var prefix = element.Descendants().FirstOrDefault(x => x.Name == "VersionPrefix");
    var suffix = element.Descendants().FirstOrDefault(x => x.Name == "VersionSuffix");
    
    projVersion = prefix.Value;
    if (!suffix.Value.ToLower().Equals("release")) projVersion += suffix.Value;
    Information("The following project version has been read from .csproj file: " + projVersion);
});

//Publish for each Platform that is configurated
Task("Publish")
    .IsDependentOn("ObtainProjectVersion")
    .DoesForEach(platforms.Keys,
    (targetPlatform) =>
{
    //Define the Settings
    var settings = new DotNetCorePublishSettings 
    {
        Framework = "netcoreapp2.1", 
        Configuration = configuration,
        OutputDirectory = platforms[targetPlatform],
        SelfContained = true,
        Runtime = targetPlatform
    };
    DotNetCorePublish(csProjPath, settings);
});

Task("Compress")
    .IsDependentOn("Publish")
    .DoesForEach(platforms.Values, 
    (outputDir) =>
{
    Zip(outputDir, string.Format("{0} {1}{2}", outputDir, projVersion, ".zip"));
});

Task("Cleanup")
    .IsDependentOn("Compress")
    .DoesForEach(platforms.Values,
    (outputDir) => 
{  
    DeleteDirectory(outputDir, true);
});
//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Cleanup");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);