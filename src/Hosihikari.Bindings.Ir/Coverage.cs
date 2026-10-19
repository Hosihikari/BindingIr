using System.Text.Json.Serialization;

namespace Hosihikari.Bindings.Ir;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CoverageStatus
{
    [JsonStringEnumMemberName("bound")] Bound,
    [JsonStringEnumMemberName("skipped")] Skipped,
    [JsonStringEnumMemberName("excluded")] Excluded,
    [JsonStringEnumMemberName("unknown")] Unknown,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CoverageReason
{
    [JsonStringEnumMemberName("not_public")] NotPublic,
    [JsonStringEnumMemberName("private")] Private,
    [JsonStringEnumMemberName("mcnaqi")] Mcnaqi,
    [JsonStringEnumMemberName("internal_linkage")] InternalLinkage,
    [JsonStringEnumMemberName("unnamed_type")] UnnamedType,
    [JsonStringEnumMemberName("template_declaration")] TemplateDeclaration,
    [JsonStringEnumMemberName("function_macro")] FunctionMacro,
    [JsonStringEnumMemberName("friend")] Friend,
    [JsonStringEnumMemberName("deleted")] Deleted,
    [JsonStringEnumMemberName("platform_mismatch")] PlatformMismatch,
    [JsonStringEnumMemberName("out_of_scope")] OutOfScope,
    [JsonStringEnumMemberName("unrepresentable_type")] UnrepresentableType,
    [JsonStringEnumMemberName("inaccessible_constructor")] InaccessibleConstructor,
    [JsonStringEnumMemberName("by_value_derived")] ByValueDerived,
    [JsonStringEnumMemberName("no_layout")] NoLayout,
    [JsonStringEnumMemberName("operator_unmappable")] OperatorUnmappable,
    [JsonStringEnumMemberName("varargs_unsupported")] VarargsUnsupported,
    [JsonStringEnumMemberName("requires_missing_instantiation")] RequiresMissingInstantiation,
    [JsonStringEnumMemberName("macro_only")] MacroOnly,
}

public sealed class CoverageEntry
{
    public string Id { get; set; } = "";
    public string? Parent { get; set; }
    public string Surface { get; set; } = "";
    public string Configuration { get; set; } = "";
    public string Kind { get; set; } = "";
    public string QualifiedName { get; set; } = "";
    public CoverageStatus Status { get; set; }
    public List<string> Emitters { get; set; } = [];
    public CoverageReason? Reason { get; set; }
    public SourceLocation? Source { get; set; }
}

public sealed class CoverageCounts
{
    public int Total { get; set; }
    public int Bound { get; set; }
    public int Skipped { get; set; }
    public int Excluded { get; set; }
    public int Unknown { get; set; }
}

public sealed class CoverageSummary
{
    public string ManifestVersion { get; set; } = IrSchema.CoverageManifestVersion;
    public string IrVersion { get; set; } = IrSchema.Version;
    public string? IrManifestSha256 { get; set; }
    public List<string> Configurations { get; set; } = [];
    public CoverageCounts Declarations { get; set; } = new();
    public CoverageCounts Members { get; set; } = new();
    public Dictionary<string, CoverageCounts> BySurface { get; set; } = [];
    public Dictionary<string, int> ByReason { get; set; } = [];
    public List<string> Unaccounted { get; set; } = [];
    public string? ManifestSha256 { get; set; }
}
