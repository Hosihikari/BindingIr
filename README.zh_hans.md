# Hosihikari.Bindings.Ir

[English](README.md) | [简体中文](README.zh_hans.md)

星光绑定生成器与 .NET 发射器共享的版本化中间表示（IR）与覆盖契约。

## 边界

本仓只含契约：JSONL schema、C# 模型、读写器、规范 JSON 哈希与校验器。它不解析 C++ 头文件、不嵌入 Clang、不生成绑定。

- IR **实例**由绑定仓 CI 中的生成器产出，经 `actions/upload-artifact` / `download-artifact` 交换，不提交到本仓。
- **覆盖清单**由发射器从 IR 派生，提交在绑定仓。
- 设计理由与完整 schema 在内部 `Hosihikari/Design` 仓（私有）：`docs/ir-and-coverage.md`、ADR-0013。

## 工程

| 路径 | 用途 |
|---|---|
| `src/Hosihikari.Bindings.Ir` | 模型、JSONL 读写、规范 JSON + SHA-256、校验器 |
| `tests/Hosihikari.Bindings.Ir.Tests` | 无依赖断言宿主；失败时以非零退出 |

## 构建与验证

```shell
dotnet run --project tests/Hosihikari.Bindings.Ir.Tests/Hosihikari.Bindings.Ir.Tests.csproj -c Release
```

断言宿主覆盖记录/清单往返、多态记录、规范哈希、枚举名，以及严格覆盖门禁（unknown、重复 `(id, configuration)`、缺失原因、计数不一致）。

## 版本

`IrSchema.Version` 是 IR schema 版本（SemVer）。加法变更提升 minor，破坏性变更提升 major。发射器必须拒绝不支持的主版本，不得猜测。`IrSchema.CoverageManifestVersion` 独立版本化覆盖清单。

## 许可证

LGPL-3.0-or-later，见 `LICENSE.md`。
