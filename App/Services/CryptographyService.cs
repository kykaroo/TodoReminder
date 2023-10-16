using System.Security.Cryptography;
using System.Text;

namespace App.Services;

public class CryptographyService : ICryptographyService
{
    public string Encrypt(string stringToEncryption)
    {
        var md5 = MD5.Create();

        var bytes = Encoding.ASCII.GetBytes(stringToEncryption);
        var hash = md5.ComputeHash(bytes);

        var sb = new StringBuilder();

        foreach (var i in hash)
        {
            sb.Append(i.ToString("X2"));
        }

        return Convert.ToString(sb)!;
    }

    public string UnEncrypt(string stringToEncryption)
    {
        throw new NotImplementedException();
    }
}