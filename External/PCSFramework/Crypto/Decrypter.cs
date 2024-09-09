using System.Security.Cryptography;

namespace PCS.Crypto
{
    public class Decrypter
    {
        private readonly byte[] _key;
        private readonly CryptoOption _option;

        public Decrypter(byte[] key, CryptoOption option)
        {
            _key = key;
            _option = option;
        }

        public byte[] Decrypt(byte[] data, byte[] iv)
        {
            using (var rijndael = new RijndaelManaged())
            {
                rijndael.SetOption(_option);
                rijndael.Key = _key;
                rijndael.IV = iv;

                using (var decryptor = rijndael.CreateDecryptor())
                {
                    return decryptor.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }
    }
}

