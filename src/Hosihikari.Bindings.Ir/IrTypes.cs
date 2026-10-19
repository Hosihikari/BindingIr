namespace Hosihikari.Bindings.Ir;

public sealed class SourceLocation
{
    public string File { get; set; } = "";
    public int Line { get; set; }
    public int Column { get; set; }
}

public sealed class CommentInfo
{
    public string? Raw { get; set; }
    public string? Brief { get; set; }
}

public sealed class AttributeInfo
{
    public string Name { get; set; } = "";
    public List<string> Arguments { get; set; } = [];
    public SourceLocation? Source { get; set; }
}

public sealed class MacroUseInfo
{
    public string Name { get; set; } = "";
    public string? Expansion { get; set; }
}

public sealed class TemplateParameter
{
    public string Kind { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Type { get; set; }
    public string? Default { get; set; }
}

public sealed class TemplateArgument
{
    public string Kind { get; set; } = "";
    public string Value { get; set; } = "";
}

public sealed class TemplateInfo
{
    public List<TemplateParameter> Parameters { get; set; } = [];
    public List<TemplateArgument> Arguments { get; set; } = [];
    public string? SpecializationKind { get; set; }
    public string? InstantiationKind { get; set; }
}

public sealed class ParameterInfo
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string? Default { get; set; }
}

public sealed class SignatureInfo
{
    public string Canonical { get; set; } = "";
    public string ReturnType { get; set; } = "";
    public List<ParameterInfo> Parameters { get; set; } = [];
    public bool Variadic { get; set; }
    public bool Const { get; set; }
    public bool Noexcept { get; set; }
    public string? RefQualifier { get; set; }
}

public sealed class TypeRef
{
    public string Kind { get; set; } = "";
    public string Canonical { get; set; } = "";
    public bool Const { get; set; }
    public bool Volatile { get; set; }
    public string? Identifier { get; set; }
    public string? Pointee { get; set; }
    public string? ElementType { get; set; }
    public int? ArraySize { get; set; }
    public string? CallingConvention { get; set; }
    public bool Variadic { get; set; }
    public List<TemplateArgument> TemplateArguments { get; set; } = [];
}

public sealed class FieldOffsetInfo
{
    public string MemberId { get; set; } = "";
    public long BitOffset { get; set; }
    public long BitWidth { get; set; }
}

public sealed class BaseInfo
{
    public string DeclarationId { get; set; } = "";
    public long Offset { get; set; }
    public string Access { get; set; } = "public";
    public bool Virtual { get; set; }
}

public sealed class VTableSlotInfo
{
    public string MemberId { get; set; } = "";
    public int Slot { get; set; }
    public long Offset { get; set; }
}

public sealed class LayoutInfo
{
    public long Size { get; set; }
    public long Align { get; set; }
    public List<FieldOffsetInfo> FieldOffsets { get; set; } = [];
    public List<BaseInfo> Bases { get; set; } = [];
    public List<VTableSlotInfo> VTableSlots { get; set; } = [];
}
