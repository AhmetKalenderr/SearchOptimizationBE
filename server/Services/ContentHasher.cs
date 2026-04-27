using System.Security.Cryptography;
using System.Text;

namespace SearchOptimizationBE.Services;

public static class ContentHasher
{
    public static byte[] Sha256(string title, string content)
    {
        var canonical = (title?.Trim() ?? string.Empty) + "\n\n" + (content?.Trim() ?? string.Empty);
        return SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
    }
}
