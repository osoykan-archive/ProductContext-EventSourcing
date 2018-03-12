var target = Argument("target", "Default");

var projectName = "ProductContext";
var solution = "./" + projectName + ".sln";
var deployable_project = "./src/" + projectName + ".WebApi/" + projectName + ".WebApi.csproj";
var distDirectory = Directory("./artifacts/");
var configuration = Argument<string>("cfg", "Release");
var framework = Argument<string>("framework", "netcoreapp2.0");

Task("Clean")
    .Does(() =>
    {
        Information("Cleaning Binary folders");
        CleanDirectories("./**/bin");
        CleanDirectories("./**/obj");
        CleanDirectories(distDirectory);
    });

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetCoreRestore(solution);
    });

 Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        DotNetCoreBuild(solution, new DotNetCoreBuildSettings
        {
            Configuration = configuration,
        });
    });

Task("PublishWeb")
    .IsDependentOn("Build")  
    .Does(() =>
    {
    var settings = new DotNetCorePublishSettings
        {
            Framework = framework,
            Configuration = configuration,
            OutputDirectory = distDirectory
        };

     DotNetCorePublish(deployable_project, settings);
    });


Task("Default")
    .IsDependentOn("PublishWeb");

RunTarget(target);