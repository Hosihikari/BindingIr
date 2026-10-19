# Hosihikari.Bindings.Ir

The versioned intermediate representation (IR) and coverage contract shared by the Hosihikari binding generator and its .NET emitters.

## Boundary

This repository contains only the contract: the JSONL schema, the C# model, the reader/writer, the canonical JSON hashing helpers, and the validator. It does not parse C++ headers, embed Clang, or generate bindings.

- IR **instances** are produced by the generator in the bindings CI and exchanged with `actions/upload-artifact` / `actions/download-artifact`. They are not committed here.
- The **coverage manifest** is derived from the IR by the emitters and committed in the bindings repository.
- The design rationale and the full schema are in [`Hosihikari/Design`](https://github.com/Hosihikari/Design): `docs/ir-and-coverage.md`, ADR-0013.

## Projects

| Path | Purpose |
|---|---|
| `src/Hosihikari.Bindings.Ir` | Models, JSONL reader/writer, canonical JSON + SHA-256, validators |
| `tests/Hosihikari.Bindings.Ir.Tests` | Dependency-free assertion host; exits non-zero on failure |

## Build and verify

```shell
dotnet run --project tests/Hosihikari.Bindings.Ir.Tests/Hosihikari.Bindings.Ir.Tests.csproj -c Release
```

The test host covers record/manifest round-trips, polymorphic records, canonical hashing, enum names, and the strict coverage gate (unknown entries, duplicate `(id, configuration)` keys, missing reasons, count mismatches).

## Versioning

`IrSchema.Version` is the IR schema version (SemVer). Additive changes bump minor; breaking changes bump major. Emitters must reject unsupported major versions rather than guess. `IrSchema.CoverageManifestVersion` versions the coverage manifest independently.

## Licence

LGPL-3.0-or-later, see `LICENSE.md`.
