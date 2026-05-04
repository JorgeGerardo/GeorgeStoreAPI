using System.Security.Cryptography;
using System.Text;

namespace GeorgeStore.Common;

public static class CryptographyGeneratorExtensions
{
    public static byte[] GetHash(this string value)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(value));
    }
    public static byte[] GetHash(this Guid value)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(value.ToString()));
    }
    public static string GetHashString(this byte[] value)
    {
        return Convert.ToBase64String(value);
    }

}
