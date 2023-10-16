namespace App.Services;

public interface ICryptographyService
{
    string Encrypt(string stringToEncryption);
    string UnEncrypt(string stringToEncryption);
}