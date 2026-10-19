using System.Security.Cryptography;
using System.Text;

namespace Hosihikari.Bindings.Ir;

public static class IrHash
{
    public static string Sha256Hex(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    public static string Sha256Hex(ReadOnlySpan<byte> value) =>
        Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();

    public static string Sha256Hex(Stream stream) =>
        Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();

    public static string CanonicalJsonSha256(string json) =>
        Sha256Hex(IrCanonicalJson.Canonicalize(json));
}
