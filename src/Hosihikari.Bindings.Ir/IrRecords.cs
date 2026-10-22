using System.Text.Json.Serialization;

namespace Hosihikari.Bindings.Ir;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "recordType")]
[JsonDerivedType(typeof(DeclarationRecord), "declaration")]
[JsonDerivedType(typeof(VariantRecord), "variant")]
[JsonDerivedType(typeof(MemberRecord), "member")]
[JsonDerivedType(typeof(TypeRecord), "type")]
[JsonDerivedType(typeof(MacroRecord), "macro")]
[JsonDerivedType(typeof(DiagnosticRecord), "diagnostic")]
public abstract class IrRecord
{
    public string? Configuration { get; set; }
}

public sealed class DeclarationRecord : IrRecord
{
    public string Id { get; set; } = "";
    public string Surface { get; set; } = "";
    public string Kind { get; set; } = "";
    public string QualifiedName { get; set; } = "";
    public string? ParentId { get; set; }
    public string Access { get; set; } = "public";
    public string Linkage { get; set; } = "external";
    public SourceLocation? Source { get; set; }
    public List<AttributeInfo> Attributes { get; set; } = [];
    public List<MacroUseInfo> Macros { get; set; } = [];
    public CommentInfo? Comment { get; set; }
    public TemplateInfo? Template { get; set; }
    public SignatureInfo? Signature { get; set; }
    public bool Deprecated { get; set; }
    public bool Abstract { get; set; }
    public bool CopyConstructible { get; set; }
    public bool Inline { get; set; }
    public bool Defined { get; set; }
    public bool Consteval { get; set; }
    public string? UnderlyingType { get; set; }
    public string? MangledName { get; set; }

    /// <summary>Linked symbol for symbol-derived declarations (for example a vtable).</summary>
    public string? Symbol { get; set; }

    /// <summary>Loaded virtual address for the symbol, when known.</summary>
    public ulong? Address { get; set; }

    /// <summary>Image-relative address for the symbol, when known.</summary>
    public ulong? Rva { get; set; }
}

public sealed class VariantRecord : IrRecord
{
    public string DeclarationId { get; set; } = "";
    public string Availability { get; set; } = "present";
    public LayoutInfo? Layout { get; set; }

    /// <summary>RTTI-derived base classes, independent of a full layout.</summary>
    public List<BaseInfo> Bases { get; set; } = [];

    /// <summary>Linked vtable symbol for symbol-derived classes.</summary>
    public string? Symbol { get; set; }

    /// <summary>Loaded vtable address for the configuration, when known.</summary>
    public ulong? Address { get; set; }

    /// <summary>Image-relative vtable address, when known.</summary>
    public ulong? Rva { get; set; }
}

public sealed class MemberRecord : IrRecord
{
    public string Id { get; set; } = "";
    public string DeclarationId { get; set; } = "";
    public string Kind { get; set; } = "";
    public string Name { get; set; } = "";
    public string Access { get; set; } = "public";
    public bool Static { get; set; }
    public bool Virtual { get; set; }
    public bool Pure { get; set; }
    public bool Inline { get; set; }
    public bool Defined { get; set; }
    public bool Consteval { get; set; }
    public bool VariableTemplate { get; set; }
    public bool MemberTemplate { get; set; }
    public string? RefQualifier { get; set; }
    public string? OverrideOf { get; set; }
    [JsonPropertyName("vtableIndex")]
    public int? VTableIndex { get; set; }
    public long? BitOffset { get; set; }
    public long? BitWidth { get; set; }
    public TypeRef? Type { get; set; }
    public SignatureInfo? Signature { get; set; }
    public List<AttributeInfo> Attributes { get; set; } = [];
    public List<MacroUseInfo> Macros { get; set; } = [];
    public CommentInfo? Comment { get; set; }

    /// <summary>Linked symbol for symbol-derived members.</summary>
    public string? Symbol { get; set; }

    /// <summary>Loaded virtual address for the member symbol, when known.</summary>
    public ulong? Address { get; set; }

    /// <summary>Image-relative address for the member symbol, when known.</summary>
    public ulong? Rva { get; set; }
}

public sealed class TypeRecord : IrRecord
{
    public string Id { get; set; } = "";
    public string Kind { get; set; } = "";
    public string Canonical { get; set; } = "";
    public TypeRef TypeRef { get; set; } = new();
}

public sealed class MacroRecord : IrRecord
{
    public string Name { get; set; } = "";
    public SourceLocation? Definition { get; set; }
    public SourceLocation? Expansion { get; set; }
    public bool FunctionLike { get; set; }
    public bool Variadic { get; set; }
    public List<string> Parameters { get; set; } = [];
    public string? Replacement { get; set; }
}

public sealed class DiagnosticRecord : IrRecord
{
    public string Severity { get; set; } = "warning";
    public string Code { get; set; } = "";
    public string Message { get; set; } = "";
    public string File { get; set; } = "";
    public int Line { get; set; }
    public int Column { get; set; }
    public string Surface { get; set; } = "";
}
