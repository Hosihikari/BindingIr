namespace Hosihikari.Bindings.Ir;

public static class IrSchema
{
    public const string Uri = "hosihikari.bindings.ir/1";
    public const string Version = "1.6.0";
    public const string CoverageManifestVersion = "1.0.0";

    /// <summary>Accepts coverage produced by an earlier minor of the same major version.</summary>
    public static bool IsCompatibleVersion(string VersionText)
    {
        if (!System.Version.TryParse(VersionText, out var Candidate)
            || !System.Version.TryParse(Version, out var Current))
        {
            return false;
        }

        return Candidate.Major == Current.Major && Candidate.Minor <= Current.Minor;
    }
}
