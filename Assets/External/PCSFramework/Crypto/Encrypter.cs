using System.IO;
using System.Security.Cryptography;

namespace PCS.Crypto
{
    public class Encrypter
    {
        private readonly byte[] _key;
        private readonly CryptoOption _option;

        public Encrypter(byte[] key, CryptoOption option)
        {
            _key = key;
            _option = option;
        }

        public byte[] Encrypt(byte[] data)
        {
            using (var rijndael = new RijndaelManaged())
            {
                rijndael.SetOption(_option);
                rijndael.Key = _key;
                rijndael.GenerateIV();
                var iv = rijndael.IV;
                
                using (var encryptor = rijndael.CreateEncryptor())
                using (var memoryStream = new MemoryStream(data.Length + iv.Length))
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    //암호화된 데이터 헤더에 iv를 붙임
                    memoryStream.Write(iv, 0, iv.Length);
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }
    }
}
