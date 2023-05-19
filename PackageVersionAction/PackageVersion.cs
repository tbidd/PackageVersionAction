namespace PackageVersionAction;

public sealed record PackageVersion(int Major, int Minor, int Patch, int? BetaNumber)
{
    public static PackageVersion Parse(string version, bool isBeta)
    {
        version = version.Replace("nuget-", string.Empty);
        if (isBeta is false)
        {
            version = version.Split('-')[0];
        }

        var versionParts = version.Split(".");
        var major = int.Parse(versionParts[0]);
        var minor = int.Parse(versionParts[1]);
        int patch;
        int? betaVersion = null;
        if (!versionParts[2].Contains("beta") && isBeta is false)
        {
            patch = int.Parse(versionParts[2]);
        }
        else
        {
            var patchSplit = versionParts[2].Split("-");
            patch = int.Parse(patchSplit[0]);
            betaVersion = patchSplit.Length > 1 ? int.Parse(patchSplit[1].Replace("beta", string.Empty)) : 0;
        }

        return new(major, minor, patch, betaVersion);
    }

    public PackageVersion IncrementMajor() => this with { Major = Major + 1 };

    public PackageVersion IncrementMinor() => this with { Minor = Minor + 1 };
    public PackageVersion IncrementPatch() => this with { Patch = Patch + 1 };
    
    public PackageVersion GetNextVersion() => BetaNumber is not null ? IncrementBeta() : IncrementPatch();
    public PackageVersion IncrementBeta() => this with { BetaNumber = BetaNumber + 1 };
    public override string ToString() => $"{Major}.{Minor}.{Patch}{(BetaNumber is not null ? $"-beta{BetaNumber}" : string.Empty)}";
}