using System.Text.Json;
using CommandLine;
using CommandLine.Text;

namespace PackageVersionAction;

public class Inputs
{
    [Option(longName: "majorVersion", HelpText = "The major version of the NuGet package to publish", Required = true)]
    public string? MajorVersion { get; set; }

    [Option(longName: "minorVersion", HelpText = "The minor version of the NuGet package to publish", Required = true)]
    public string? MinorVersion { get; set; }

    [Option(longName: "publishBeta", HelpText = " Whether to publish packages as beta", Required = false, Default = false)]
    public bool PublishBeta { get; set; }

    [Option(longName: "organisation", HelpText = "The organisation to query for the package version", Required = true)]
    public string Organisation { get; set; } = null!;

    [Option(longName:"packageName", HelpText = "The name of the package to query for the package version", Required = true)]
    public string? PackageName { get; set; }
    
    

    public override string ToString() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
}