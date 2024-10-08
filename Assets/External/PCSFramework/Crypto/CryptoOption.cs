using System;
using System.Security.Cryptography;

namespace PCS.Crypto
{
    [Serializable]
    public class CryptoOption
    {
        public int KeySize = 128;
        /// <summary> Change by byte /// </summary>
        public int KeyBytes => KeySize / 8;

        public int BlockSize = 128;

        /// <summary> Change by byte /// </summary>
        public int BlockBytes => BlockSize / 8;

        public PaddingMode Padding = PaddingMode.PKCS7;

        public CipherMode Mode = CipherMode.CBC;

        public bool IsAES => BlockSize == 128 && (KeySize == 128 || KeySize == 192 || KeySize == 256);
    }
}
