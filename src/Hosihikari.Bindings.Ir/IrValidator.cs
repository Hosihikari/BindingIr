namespace Hosihikari.Bindings.Ir;

public sealed class IrValidationResult
{
    public List<string> Errors { get; } = [];
    public bool IsValid => Errors.Count == 0;
    public void Add(string error) => Errors.Add(error);
}

public static class IrValidator
{
    private static readonly HashSet<string> Surfaces = new(StringComparer.Ordinal) { "bedrock", "levilamina", "endstone" };
    private static readonly HashSet<string> Access = new(StringComparer.Ordinal) { "public", "protected", "private" };
    private static readonly HashSet<string> Availability = new(StringComparer.Ordinal) { "present", "absent" };
    private static readonly HashSet<string> Emitters = new(StringComparer.Ordinal) { "interop", "shim" };
    private static readonly HashSet<string> Severities = new(StringComparer.Ordinal) { "error", "fatal", "warning", "note" };

    public static IrValidationResult Validate(IrManifest manifest, IReadOnlyList<IrRecord> records)
    {
        var result = new IrValidationResult();

        if (manifest.Schema != IrSchema.Uri)
        {
            result.Add($"manifest.schema must be '{IrSchema.Uri}', got '{manifest.Schema}'.");
        }

        if (!Version.TryParse(manifest.IrVersion, out _))
        {
            result.Add($"manifest.irVersion '{manifest.IrVersion}' is not a version.");
        }

        if (string.IsNullOrWhiteSpace(manifest.Frontend.Name))
        {
            result.Add("manifest.frontend.name is required.");
        }

        var configurations = new HashSet<string>(StringComparer.Ordinal);
        foreach (var configuration in manifest.Configurations)
        {
            if (string.IsNullOrWhiteSpace(configuration.Id))
            {
                result.Add("configuration.id is required.");
                continue;
            }

            if (!configurations.Add(configuration.Id))
            {
                result.Add($"duplicate configuration '{configuration.Id}'.");
            }

            if (string.IsNullOrWhiteSpace(configuration.DefinesHash))
            {
                result.Add($"configuration '{configuration.Id}' has no definesHash.");
            }
        }

        if (configurations.Count == 0)
        {
            result.Add("manifest must declare at least one configuration.");
        }

        foreach (var shard in manifest.Shards)
        {
            if (!configurations.Contains(shard.Configuration))
            {
                result.Add($"shard '{shard.Path}' references unknown configuration '{shard.Configuration}'.");
            }

            if (string.IsNullOrWhiteSpace(shard.Path) || string.IsNullOrWhiteSpace(shard.Sha256))
            {
                result.Add($"shard '{shard.Module}' is missing path or hash.");
            }
        }

        if (manifest.Diagnostics.Errors > 0 || manifest.Diagnostics.ShardsWithErrors.Count > 0)
        {
            result.Add("manifest reports parse errors; IR is incomplete.");
        }

        var declarations = new Dictionary<string, DeclarationRecord>(StringComparer.Ordinal);
        var members = new HashSet<string>(StringComparer.Ordinal);
        var keys = new HashSet<(string Id, string Configuration)>();

        foreach (var record in records)
        {
            switch (record)
            {
                case DeclarationRecord declaration:
                    if (string.IsNullOrWhiteSpace(declaration.Id))
                    {
                        result.Add("declaration.id is required.");
                        break;
                    }

                    if (!declarations.TryAdd(declaration.Id, declaration))
                    {
                        result.Add($"duplicate declaration '{declaration.Id}'.");
                    }

                    if (!Surfaces.Contains(declaration.Surface))
                    {
                        result.Add($"declaration '{declaration.Id}' has unknown surface '{declaration.Surface}'.");
                    }

                    if (!Access.Contains(declaration.Access))
                    {
                        result.Add($"declaration '{declaration.Id}' has unknown access '{declaration.Access}'.");
                    }

                    if (!keys.Add((declaration.Id, "")))
                    {
                        result.Add($"duplicate record key '({declaration.Id}, )'.");
                    }

                    break;

                case VariantRecord variant:
                    if (!declarations.ContainsKey(variant.DeclarationId))
                    {
                        result.Add($"variant references unknown declaration '{variant.DeclarationId}'.");
                    }

                    if (!configurations.Contains(variant.Configuration ?? ""))
                    {
                        result.Add($"variant for '{variant.DeclarationId}' has unknown configuration '{variant.Configuration}'.");
                    }

                    if (!Availability.Contains(variant.Availability))
                    {
                        result.Add($"variant for '{variant.DeclarationId}' has unknown availability '{variant.Availability}'.");
                    }

                    break;

                case MemberRecord member:
                    if (string.IsNullOrWhiteSpace(member.Id))
                    {
                        result.Add("member.id is required.");
                        break;
                    }

                    if (!declarations.ContainsKey(member.DeclarationId))
                    {
                        result.Add($"member '{member.Id}' references unknown declaration '{member.DeclarationId}'.");
                    }

                    if (!configurations.Contains(member.Configuration ?? ""))
                    {
                        result.Add($"member '{member.Id}' has unknown configuration '{member.Configuration}'.");
                    }

                    if (!keys.Add((member.Id, member.Configuration ?? "")))
                    {
                        result.Add($"duplicate record key '({member.Id}, {member.Configuration})'.");
                    }

                    members.Add(member.Id);

                    if (member.BitOffset is < 0 || member.BitWidth is < 0)
                    {
                        result.Add($"member '{member.Id}' has a negative bit offset or width.");
                    }

                    if (member.VTableIndex is < 0)
                    {
                        result.Add($"member '{member.Id}' has a negative vtable index.");
                    }

                    if (member.Type is null || string.IsNullOrWhiteSpace(member.Type.Canonical))
                    {
                        result.Add($"member '{member.Id}' has no canonical type.");
                    }

                    break;

                case TypeRecord type:
                    if (string.IsNullOrWhiteSpace(type.Id) || string.IsNullOrWhiteSpace(type.Canonical))
                    {
                        result.Add("type.id and type.canonical are required.");
                        break;
                    }

                    if (!keys.Add((type.Id, type.Configuration ?? "")))
                    {
                        result.Add($"duplicate record key '({type.Id}, {type.Configuration})'.");
                    }

                    break;

                case MacroRecord macro:
                    if (string.IsNullOrWhiteSpace(macro.Name))
                    {
                        result.Add("macro.name is required.");
                    }

                    break;

                case DiagnosticRecord diagnostic:
                    if (!Severities.Contains(diagnostic.Severity))
                    {
                        result.Add($"diagnostic '{diagnostic.Code}' has unknown severity '{diagnostic.Severity}'.");
                    }

                    if (diagnostic.Severity is "error" or "fatal")
                    {
                        result.Add($"diagnostic error '{diagnostic.Code}' at {diagnostic.File}:{diagnostic.Line}.");
                    }

                    break;
            }
        }

        foreach (var variant in records.OfType<VariantRecord>())
        {
            if (variant.Layout is null)
            {
                continue;
            }

            foreach (var field in variant.Layout.FieldOffsets)
            {
                if (!members.Contains(field.MemberId))
                {
                    result.Add($"layout of '{variant.DeclarationId}' references unknown member '{field.MemberId}'.");
                }
            }

            foreach (var slot in variant.Layout.VTableSlots)
            {
                if (!members.Contains(slot.MemberId))
                {
                    result.Add($"vtable of '{variant.DeclarationId}' references unknown member '{slot.MemberId}'.");
                }
            }
        }

        return result;
    }

    public static IrValidationResult ValidateCoverage(CoverageSummary summary, IReadOnlyList<CoverageEntry> entries)
    {
        var result = new IrValidationResult();

        if (summary.ManifestVersion != IrSchema.CoverageManifestVersion)
        {
            result.Add($"coverage.manifestVersion must be '{IrSchema.CoverageManifestVersion}'.");
        }

        if (summary.IrVersion != IrSchema.Version)
        {
            result.Add($"coverage.irVersion must be '{IrSchema.Version}', got '{summary.IrVersion}'.");
        }

        if (summary.Unaccounted.Count > 0)
        {
            result.Add($"coverage has {summary.Unaccounted.Count} unaccounted declarations.");
        }

        var keys = new HashSet<(string Id, string Configuration)>();
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Id))
            {
                result.Add("coverage.id is required.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.Configuration))
            {
                result.Add($"coverage entry '{entry.Id}' has no configuration.");
            }

            if (!keys.Add((entry.Id, entry.Configuration)))
            {
                result.Add($"duplicate coverage key '({entry.Id}, {entry.Configuration})'.");
            }

            switch (entry.Status)
            {
                case CoverageStatus.Bound:
                    if (entry.Reason is not null)
                    {
                        result.Add($"bound entry '{entry.Id}' must not have a reason.");
                    }

                    if (entry.Emitters.Count == 0 || entry.Emitters.Any(e => !Emitters.Contains(e)))
                    {
                        result.Add($"bound entry '{entry.Id}' has invalid emitters.");
                    }

                    break;

                case CoverageStatus.Skipped:
                case CoverageStatus.Excluded:
                    if (entry.Reason is null)
                    {
                        result.Add($"{entry.Status.ToString().ToLowerInvariant()} entry '{entry.Id}' must have a reason.");
                    }

                    if (entry.Emitters.Count != 0)
                    {
                        result.Add($"{entry.Status.ToString().ToLowerInvariant()} entry '{entry.Id}' must not have emitters.");
                    }

                    break;

                case CoverageStatus.Unknown:
                    result.Add($"coverage entry '{entry.Id}' is unknown.");
                    break;
            }
        }

        if (summary.Declarations.Unknown != 0 || summary.Members.Unknown != 0)
        {
            result.Add("coverage summary reports unknown entries.");
        }

        CheckCounts(result, "declarations", summary.Declarations, entries.Where(e => e.Parent is null));
        CheckCounts(result, "members", summary.Members, entries.Where(e => e.Parent is not null));

        return result;
    }

    private static void CheckCounts(
        IrValidationResult result,
        string label,
        CoverageCounts expected,
        IEnumerable<CoverageEntry> entries)
    {
        var list = entries.ToList();
        var actual = new CoverageCounts
        {
            Total = list.Count,
            Bound = list.Count(e => e.Status == CoverageStatus.Bound),
            Skipped = list.Count(e => e.Status == CoverageStatus.Skipped),
            Excluded = list.Count(e => e.Status == CoverageStatus.Excluded),
            Unknown = list.Count(e => e.Status == CoverageStatus.Unknown),
        };

        if (expected.Total != actual.Total
            || expected.Bound != actual.Bound
            || expected.Skipped != actual.Skipped
            || expected.Excluded != actual.Excluded
            || expected.Unknown != actual.Unknown)
        {
            result.Add($"coverage summary {label} counts do not match the entries.");
        }
    }
}
