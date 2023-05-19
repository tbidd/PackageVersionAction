using System.Text;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Octokit;
using PackageVersionAction;
using static CommandLine.Parser;
using PackageVersion = PackageVersionAction.PackageVersion;


using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) => services.AddLogging(configure => configure.AddConsole()))
    .Build();

static TService Get<TService>(IHost host)
    where TService : notnull =>
    host.Services.GetRequiredService<TService>();


var parser = Default.ParseArguments<Inputs>(() => new(), args);
parser.WithNotParsed(errors =>
{
    Get<ILoggerFactory>(host).CreateLogger<Program>()
        .LogError("Failed to parse arguments {Errors}",
            string.Join(Environment.NewLine, errors.Select(error => error.ToString())));
    Environment.Exit(1);
});

await parser.WithParsedAsync(async inputs => await GetPackageVersion(inputs, host));
await host.RunAsync();


static async ValueTask GetPackageVersion(Inputs inputs, IHost host)
{
    var logger = Get<ILoggerFactory>(host).CreateLogger<Program>();
    logger.LogInformation("Arguments: {Arguments}", inputs);
    var exitCode = 0;
    
    try
    {
        var client = new GitHubClient(new ProductHeaderValue("PackageVersionAction"))
        {
            Credentials = new(Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? throw new("GITHUB_TOKEN is not set."))
        };

        var packageVersions = await client
            .Packages
            .PackageVersions
            .GetAllForOrg(inputs.Organisation, PackageType.Nuget, inputs.PackageName);
        PackageVersion packageVersion;
        
        if (packageVersions is { Count: not 0 })
        {
            packageVersion = PackageVersion.Parse(packageVersions[0].Name, inputs.PublishBeta).GetNextVersion();
        }
        else
        {
            packageVersion = new(1, 0, 0, inputs.PublishBeta ? 1 : null);
        }

        var version = $"PACKAGE_VERSION={packageVersion}";
        if (Environment.GetEnvironmentVariable("GITHUB_OUTPUT") is { Length: not 0 } gitHubOutputFile)
        {
            await using StreamWriter textWriter = new(gitHubOutputFile, true, Encoding.UTF8);
            await textWriter.WriteLineAsync(version);
            await textWriter.FlushAsync();
        }

        logger.LogInformation("NextVersion: {Version}", version);
        await ValueTask.CompletedTask;
    }
    catch (Exception e)
    {
        logger.LogError(e, "An error occured calculating the package version");
        exitCode = 1;
    }

    Environment.Exit(exitCode);
}