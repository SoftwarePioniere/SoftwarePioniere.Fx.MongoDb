#load "./scripts/utils.cake"
#load "./scripts/gitversion.cake"
#load "./scripts/dotnet.cake"
#load "./scripts/git.cake"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////
var target          = Argument("target", "Default");
var configuration   = Argument("configuration", "Release");
var isDryRun        = HasArgument("dryrun1");


///////////////////////////////////////////////////////////////////////////////
// VARIABLES
///////////////////////////////////////////////////////////////////////////////

var artifactsDirectory  = Directory("./artifacts");
var version             = "0.0.0";
var solutionFile        = File("./SoftwarePioniere.MongoDb.sln");
var image               = "softwarepioniere/softwarepioniere.readmodel.mongodb";
var nugetApiKey         = "VSTS";
var vstsToken           = "XXX";

///////////////////////////////////////////////////////////////////////////////
// SETUP/TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{
    vstsToken           = EnvironmentVariable("VSTS_TOKEN") ?? vstsToken;
    nugetApiKey         = EnvironmentVariable("NUGET_API_KEY") ?? nugetApiKey;

    if (IsTfs(context)) {
        vstsToken = EnvironmentVariable("SYSTEM_ACCESSTOKEN");
        if (string.IsNullOrEmpty(vstsToken))
            throw new System.InvalidOperationException("Please allow VSTS Token Access");
    }

    MyGitVersion.Init(context);
    MyDotNet.Init(context, configuration, isDryRun, vstsToken, nugetApiKey);

});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Version")
 .Does((context) =>
{
    version = MyGitVersion.Calculate();
    Information("Version: {0}", version);
    SetBuildNumber(context, version);

    MyGitVersion.WriteArtifacts(artifactsDirectory);
});

Task("Clean")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] { artifactsDirectory });
});


Task("Restore")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .Does(context =>
{
    MyDotNet.RestoreSolution(solutionFile);
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .IsDependentOn("Restore")
    .Does(context =>
{

    MyDotNet.BuildSolution(solutionFile);

});


Task("Test")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .Does(context =>
{
    MyDocker.StopTestEnv(context);
    MyDocker.StartTestEnv(context);


    MyDotNet.TestProjects("./test/**/*.csproj");

    MyDotNet.PublishProjects(artifactsDirectory, "./test/**/*.csproj");

})
.Finally(() =>
{
    MyDocker.StopTestEnv();
})
;

Task("Pack")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .Does(context =>
{
    MyDotNet.PackSolution(solutionFile, artifactsDirectory);
});

Task("PushPackagesLocal")
    .Does(context =>
{
    var packageSource = context.Directory(@"c:\temp\packages-debug");
    context.Information("PackageSource Dir: {0}", packageSource);

    var settings = new DotNetCoreNuGetPushSettings {
        Source =  packageSource.Path.FullPath
    };

    MyDotNet.PushPackages(artifactsDirectory, settings);
});

Task("DockerBuild")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .Does(context =>
{
    MyDotNet.DockerBuild(image);
});

Task("DockerTest")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .Does(context =>
{

    // var env = new [] {
    //             $"SOPI_TESTS_MONGODB__PORT={EnvironmentVariable("SOPI_TESTS_MONGODB__PORT")}",
    //             $"SOPI_TESTS_MONGODB__DATABASEID=sopi-test-run"
    //     };

//   MyDotNet.DockerBuildTestImage(image + ".tests" , "SoftwarePioniere.ReadModel.Services.MongoDb.Tests");
   MyDotNet.DockerComposeTestProject(image + ".tests" , "SoftwarePioniere.ReadModel.Services.MongoDb.Tests", artifactsDirectory);


});

Task("DockerPack")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .Does(context =>
{
    MyDotNet.DockerPack(image);
});

Task("DockerPushPackages")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .Does(context =>
{
    MyDotNet.DockerPushPackages(image);
});


///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default");


Task("BuildTestPackLocalPush")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
  //  .IsDependentOn("Test")
    .IsDependentOn("Pack")
    .IsDependentOn("PushPackagesLocal")
    ;


Task("DockerBuildPush")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .IsDependentOn("DockerBuild")
    .IsDependentOn("DockerTest")
    .IsDependentOn("DockerPack")
    .IsDependentOn("DockerPushPackages")
    ;

Task("Test2")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .IsDependentOn("DockerBuild")
    .IsDependentOn("DockerTest")
    .IsDependentOn("DockerPack")
    ;

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);