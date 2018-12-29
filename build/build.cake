using Cake.Common.Tools.DotNetCore.DotNetCoreAliases;
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

//Publish for each Platform that is configurated
Task("Publish")
    .IsDependentOn("Clean")
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
    DotNetCorePublish(srcPath + "WebDavSync.csproj", settings);
});

Task("Compress")
    .IsDependentOn("Publish")
    .DoesForEach(platforms.Values, 
    (outputDir) =>
{
    Zip(outputDir, outputDir + ".zip");
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Compress");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);