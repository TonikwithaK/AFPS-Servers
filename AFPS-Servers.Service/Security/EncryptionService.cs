using AFPS_Servers.Service.Config;
using AFPS_Servers.Service.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace AFPS_Servers.Service.Security
{
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptionService(IOptions<EncryptionConfig> options)
        {
            var config = options.Value;

            _key = Encoding.UTF8.GetBytes(config.Key);
            _iv = Encoding.UTF8.GetBytes(config.IV);

            if (_key.Length != 32)
                throw new ArgumentException("Key must be 32 bytes for AES-256.");
            if (_iv.Length != 16)
                throw new ArgumentException("IV must be 16 bytes.");
        }

        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var writer = new StreamWriter(cs))
                writer.Write(plainText);

            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string cipherText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);
            return reader.ReadToEnd();
        }
    }
}
