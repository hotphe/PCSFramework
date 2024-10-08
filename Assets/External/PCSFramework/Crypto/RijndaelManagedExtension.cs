using PCS.Crypto;
using System.Security.Cryptography;


public static class RijndaelManagedExtension
{
    public static void SetOption(this RijndaelManaged rijndael, CryptoOption option)
    {
        rijndael.KeySize = option.KeySize;
        rijndael.BlockSize = option.BlockSize;
        rijndael.Padding = option.Padding;
        rijndael.Mode = option.Mode;
    }
}