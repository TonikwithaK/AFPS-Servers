namespace AFPS_Servers.Service.Interfaces;
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}