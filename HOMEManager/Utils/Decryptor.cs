using System.Security.Cryptography;
using System.Text;
using static HOMEManager.Rijndael;

namespace HOMEManager
{
    public static class Decryptor
    {
        public static class ABA
        {
            private const int Size = 0x400;
            private const int Padding = 0x20;

            private static readonly byte[] Key = Encoding.UTF8.GetBytes("lrZ6++Ln5tLnBsJk.ae6J8BLaLbMhVn@");
            private static readonly byte[] IV = Encoding.UTF8.GetBytes("X6TU@VYU$HyqKy57PfwWg7.t7wk2oqtg");
            private static int PaddedSize => Size + Padding;
            public static byte[] Decrypt(byte[] data)
            {
                var block = new byte[PaddedSize];
                Buffer.BlockCopy(data, 0, block, 0, PaddedSize);

                block = DecryptData(block, Key, IV, BlockSize.Block256, KeySize.Key256, EncryptionMode.ModeCBC);

                using var ms = new MemoryStream();
                ms.Write(block);
                ms.Write(data, PaddedSize, data.Length - PaddedSize);

                return ms.ToArray();
            }
        }

        public static class AES
        {
            private static Aes _aes;
            static AES()
            {
                _aes = Aes.Create();
                _aes.Mode = CipherMode.CBC;
                _aes.Padding = PaddingMode.PKCS7;
                _aes.KeySize = 0x100;
                _aes.BlockSize = 0x80;
            }

            private const int EncryptedSize = 0x400;
            private const int Padding = 0x10;
            private static int PaddedSize => EncryptedSize + Padding;

            public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
            {
                var block = new byte[PaddedSize];
                Buffer.BlockCopy(data, 0, block, 0, PaddedSize);

                block = _aes.CreateDecryptor(key, iv).TransformFinalBlock(block, 0, block.Length);

                using var ms = new MemoryStream();
                ms.Write(block);
                ms.Write(data, PaddedSize, data.Length - PaddedSize);

                return ms.ToArray();
            }
        }

        public static class XOR
        {
            private const int EncryptedSize = 0x100;
            public static void Decrypt(byte[] data, byte[] key)
            {
                var span = data.AsSpan();
                
                var count = EncryptedSize;
                if (count >= data.Length)
                {
                    count = data.Length;
                }

                for (int i = 0; i < count; i++)
                {
                    span[i] ^= data[i % data.Length];
                }
            }
        }
    }
}
