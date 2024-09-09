using System;
using PCS.Common;

namespace PCS.Crypto
{
    public class CryptoManager
    {
        private readonly byte[] _key;
        private readonly CryptoOption _option;
        private readonly Encrypter _encrypter;
        private readonly Decrypter _decrypter;

        public CryptoManager(string key) : this(key, BaseConfig.Instance.DefaultCryptoOption) { }
        public CryptoManager(string key, CryptoOption option) : this(Convert.FromBase64String(key), option) { }

        public CryptoManager(byte[] key, CryptoOption option)
        {
            _key = key;
            _option = option;
            _encrypter = new Encrypter(key, option);
            _decrypter = new Decrypter(key, option);
        }

        public byte[] Encrypt(byte[] data) => _encrypter.Encrypt(data);
        public byte[] Decrypt(byte[] data)
        {
            var iv = new byte[_option.BlockBytes];
            var encrypted = new byte[data.Length - _option.BlockBytes];

            Buffer.BlockCopy(data, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(data, iv.Length, encrypted, 0, encrypted.Length);

            return _decrypter.Decrypt(encrypted, iv);
        }
    }
}
