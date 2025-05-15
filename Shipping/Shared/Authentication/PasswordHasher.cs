using System.Security.Cryptography;

namespace Shipping.Shared.Authentication;
public class PasswordHasher
{
    private const int HashSize = 32;
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        return CreateHashedPassword(password, salt);
    }

    public bool Verify(string passwordHash, string password)
    {
        var hashParts = passwordHash.Split('-');

        byte[] salt = Convert.FromHexString(hashParts[1]);

        var claimedPasswordHash = CreateHashedPassword(password, salt);

        if (claimedPasswordHash != passwordHash)
        {
            return false;
        }

        return true;
    }

    private string CreateHashedPassword(string password, byte[] salt)
    {
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            Algorithm,
            HashSize);

        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }
}