using System.Security.Cryptography;
using System.Text;

public static class PasswordHelper
{
    public static void CreatePasswordHash(string password, out string hash, out string salt)
    {
        using var hmac = new HMACSHA256();
        salt = Convert.ToBase64String(hmac.Key);

        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var combinedBytes = passwordBytes.Concat(hmac.Key).ToArray();

        hash = Convert.ToBase64String(
            SHA256.HashData(combinedBytes)
        );
    }

    public static bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        var saltBytes = Convert.FromBase64String(storedSalt);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var combinedBytes = passwordBytes.Concat(saltBytes).ToArray();

        var computedHash = Convert.ToBase64String(
            SHA256.HashData(combinedBytes)
        );

        return computedHash == storedHash;
    }
}
