using Hosihikari.Bindings.Ir;

var failures = new List<string>();

void Check(string name, bool condition)
{
    if (!condition)
    {
        failures.Add(name);
    }
}

var manifest = new IrManifest
{
    Frontend = new FrontendInfo { Name = "libtooling", Version = "0.1.0", Llvm = "23.1.3" },
    Sources = [new SourcePin { Surface = "bedrock", Repo = "LiteLDev/LeviLamina", Commit = "abc", Path = "src/mc" }],
    Configurations =
    [
        new IrConfiguration
        {
            Id = "win-x64-server",
            Os = "windows",
            Arch = "x64",
            Target = "server",
            Defines = ["_WIN32", "LL_PLAT_S"],
            DefinesHash = "sha256:defines",
        },
    ],
};

var declaration = new DeclarationRecord
{
    Id = "bedrock:c:@S@Player",
    Surface = "bedrock",
    Kind = "class",
    QualifiedName = "mc::Player",
};

var variant = new VariantRecord
{
    DeclarationId = declaration.Id,
    Configuration = "win-x64-server",
    Availability = "present",
};

var member = new MemberRecord
{
    Id = declaration.Id + "@F@health",
    DeclarationId = declaration.Id,
    Configuration = "win-x64-server",
    Kind = "field",
    Name = "health",
    Access = "protected",
    BitOffset = 64,
    BitWidth = 32,
    Type = new TypeRef { Kind = "fundamental", Canonical = "int" },
};

var records = new List<IrRecord> { declaration, variant, member };
var validation = IrValidator.Validate(manifest, records);
Check("valid IR", validation.IsValid);
failures.AddRange(validation.Errors.Select(error => "IR: " + error));

var method = new MemberRecord
{
    Id = declaration.Id + "@M@getId",
    DeclarationId = declaration.Id,
    Configuration = "win-x64-server",
    Kind = "method",
    Name = "getId",
    Access = "public",
    Signature = new SignatureInfo { Canonical = "mc::Player::getId", ReturnType = "int" },
};
Check("valid method IR", IrValidator.Validate(manifest, new List<IrRecord>(records) { method }).IsValid);

using (var stream = new MemoryStream())
{
    IrJson.WriteRecords(stream, records);
    stream.Position = 0;
    var read = IrJson.ReadRecords(stream);
    Check("record round trip", read.Count == 3 && read[0] is DeclarationRecord && read[1] is VariantRecord && read[2] is MemberRecord);
    Check("polymorphic declaration", read[0] is DeclarationRecord d && d.QualifiedName == "mc::Player");
}

using (var stream = new MemoryStream())
{
    IrJson.WriteManifest(stream, manifest);
    stream.Position = 0;
    var read = IrJson.ReadManifest(stream);
    Check("manifest round trip", read.Configurations.Count == 1 && read.Configurations[0].Id == "win-x64-server");
}

var unordered = "{\"b\":2,\"a\":1}";
var ordered = "{\"a\":1,\"b\":2}";
Check("canonical order", IrCanonicalJson.Canonicalize(unordered) == ordered);
Check("canonical hash", IrHash.CanonicalJsonSha256(unordered) == IrHash.CanonicalJsonSha256(ordered));

var lateDiscriminator = "{\"id\":\"late\",\"kind\":\"class\",\"qualifiedName\":\"late\",\"recordType\":\"declaration\",\"surface\":\"bedrock\"}";
using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(lateDiscriminator + "\n")))
{
    var read = IrJson.ReadRecords(stream);
    Check("out-of-order discriminator", read.Count == 1 && read[0] is DeclarationRecord);
}

var symbolDeclaration = new DeclarationRecord
{
    Id = "bedrock:Actor",
    Surface = "bedrock",
    Kind = "class",
    QualifiedName = "Actor",
    Access = "unknown",
    Symbol = "??_7Actor@@6B@",
    Address = 0x1402DA38F0,
    Rva = 0x2DA38F0,
};

var symbolVariant = new VariantRecord
{
    DeclarationId = symbolDeclaration.Id,
    Configuration = "win-x64-server",
    Symbol = "??_7Actor@@6B@",
    Address = 0x1402DA38F0,
    Rva = 0x2DA38F0,
};

var symbolMember = new MemberRecord
{
    Id = "bedrock:Actor@Actor::canAttack#bool(Actor*,bool,)",
    DeclarationId = symbolDeclaration.Id,
    Configuration = "win-x64-server",
    Kind = "method",
    Name = "canAttack",
    Access = "unknown",
    Virtual = true,
    VTableIndex = 12,
    Symbol = "?_canAttack@Actor@@UEBA_NPEAV1@_N@Z",
    Address = 0x1400BD3D0,
    Rva = 0xBD3D0,
    Signature = new SignatureInfo { Canonical = "Actor::canAttack", ReturnType = "bool" },
};

var symbolRecords = new List<IrRecord> { symbolDeclaration, symbolVariant, symbolMember };
Check("symbol IR valid", IrValidator.Validate(manifest, symbolRecords).IsValid);

using (var stream = new MemoryStream())
{
    IrJson.WriteRecords(stream, symbolRecords);
    stream.Position = 0;
    var read = IrJson.ReadRecords(stream);
    Check(
        "symbol fields round trip",
        read.Count == 3
        && read[0] is DeclarationRecord sd && sd.Symbol == "??_7Actor@@6B@" && sd.Address == 0x1402DA38F0 && sd.Rva == 0x2DA38F0
        && read[1] is VariantRecord sv && sv.Symbol == "??_7Actor@@6B@" && sv.Address == 0x1402DA38F0
        && read[2] is MemberRecord sm && sm.Symbol == "?_canAttack@Actor@@UEBA_NPEAV1@_N@Z" && sm.Address == 0x1400BD3D0 && sm.VTableIndex == 12);
}

Check("coverage version compatibility", IrSchema.IsCompatibleVersion("1.3.0"));
Check("incompatible coverage version", !IrSchema.IsCompatibleVersion("2.0.0"));

var entryJson = IrJson.Serialize(new CoverageEntry
{
    Id = "x",
    Surface = "bedrock",
    Configuration = "win-x64-server",
    Status = CoverageStatus.Excluded,
    Reason = CoverageReason.Mcnaqi,
});
Check("enum names", entryJson.Contains("\"status\":\"excluded\"") && entryJson.Contains("\"reason\":\"mcnaqi\""));

var declarationEntry = new CoverageEntry
{
    Id = declaration.Id,
    Surface = "bedrock",
    Configuration = "win-x64-server",
    Kind = "class",
    QualifiedName = "mc::Player",
    Status = CoverageStatus.Bound,
    Emitters = ["interop", "shim"],
};

var memberEntry = new CoverageEntry
{
    Id = member.Id,
    Parent = declarationEntry.Id,
    Surface = "bedrock",
    Configuration = "win-x64-server",
    Kind = "field",
    QualifiedName = "mc::Player::health",
    Status = CoverageStatus.Skipped,
    Reason = CoverageReason.UnrepresentableType,
};

var entries = new List<CoverageEntry> { declarationEntry, memberEntry };
var summary = new CoverageSummary
{
    Configurations = ["win-x64-server"],
    Declarations = new CoverageCounts { Total = 1, Bound = 1 },
    Members = new CoverageCounts { Total = 1, Skipped = 1 },
};

var coverageValidation = IrValidator.ValidateCoverage(summary, entries);
Check("valid coverage", coverageValidation.IsValid);
failures.AddRange(coverageValidation.Errors.Select(error => "coverage: " + error));

var duplicate = new List<CoverageEntry>(entries) { declarationEntry };
Check("duplicate coverage rejected", !IrValidator.ValidateCoverage(summary, duplicate).IsValid);

var unknownEntry = new CoverageEntry
{
    Id = "unknown",
    Surface = "bedrock",
    Configuration = "win-x64-server",
    Kind = "class",
    Status = CoverageStatus.Unknown,
};

Check(
    "unknown coverage rejected",
    !IrValidator.ValidateCoverage(
        new CoverageSummary
        {
            Configurations = ["win-x64-server"],
            Declarations = new CoverageCounts { Total = 1, Unknown = 1 },
        },
        [unknownEntry]
    ).IsValid
);

var missingReason = new CoverageEntry
{
    Id = "missing-reason",
    Surface = "bedrock",
    Configuration = "win-x64-server",
    Kind = "class",
    Status = CoverageStatus.Skipped,
};

Check(
    "missing reason rejected",
    !IrValidator.ValidateCoverage(
        new CoverageSummary
        {
            Configurations = ["win-x64-server"],
            Declarations = new CoverageCounts { Total = 1, Skipped = 1 },
        },
        [missingReason]
    ).IsValid
);

if (failures.Count > 0)
{
    Console.Error.WriteLine($"{failures.Count} check(s) failed:");
    foreach (var failure in failures)
    {
        Console.Error.WriteLine(" - " + failure);
    }

    return 1;
}

Console.WriteLine("IR contract checks passed.");
return 0;
