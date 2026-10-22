namespace Hosihikari.Bindings.Ir;

public sealed class FrontendInfo
{
    public string Name { get; set; } = "libtooling";
    public string Version { get; set; } = "";
    public string Llvm { get; set; } = "";
}

public sealed class SourcePin
{
    public string Surface { get; set; } = "";
    public string Repo { get; set; } = "";
    public string Commit { get; set; } = "";
    public string Path { get; set; } = "";
}

public sealed class IrConfiguration
{
    public string Id { get; set; } = "";
    public string Os { get; set; } = "";
    public string Arch { get; set; } = "";
    public string Target { get; set; } = "";
    public List<string> Defines { get; set; } = [];
    public string DefinesHash { get; set; } = "";
}

public sealed class IrShard
{
    public string Surface { get; set; } = "";
    public string Configuration { get; set; } = "";
    public string Module { get; set; } = "";
    public string Path { get; set; } = "";
    public string Sha256 { get; set; } = "";
    public string Sha256Compressed { get; set; } = "";
    public int Records { get; set; }
}

public sealed class IrDiagnostics
{
    public int Errors { get; set; }
    public int Warnings { get; set; }
    public List<string> ShardsWithErrors { get; set; } = [];
}

public sealed class IrCounts
{
    public int Declarations { get; set; }
    public int Variants { get; set; }
    public int Members { get; set; }
}

public sealed class IrSupportSymbol
{
    public string Symbol { get; set; } = "";
    public ulong? Rva { get; set; }
    public ulong? Address { get; set; }
}

public sealed class IrManifest
{
    public string Schema { get; set; } = IrSchema.Uri;
    public string IrVersion { get; set; } = IrSchema.Version;
    public FrontendInfo Frontend { get; set; } = new();
    public List<SourcePin> Sources { get; set; } = [];
    public List<IrConfiguration> Configurations { get; set; } = [];
    public List<IrShard> Shards { get; set; } = [];
    public IrDiagnostics Diagnostics { get; set; } = new();
    public IrCounts Counts { get; set; } = new();
    public Dictionary<string, Dictionary<string, IrSupportSymbol>> SupportSymbols { get; set; } = new(StringComparer.Ordinal);
}
