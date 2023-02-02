using System.Text;
using static HOMEManager.Rijndael;

namespace HOMEManager
{
    public static class DecryptUtils
    {
        private const int Size = 0x400;
        private const int Padding = 0x20;

        private static readonly byte[] Key = Encoding.UTF8.GetBytes("lrZ6++Ln5tLnBsJk.ae6J8BLaLbMhVn@");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("X6TU@VYU$HyqKy57PfwWg7.t7wk2oqtg");
        private static int PaddedSize => Size + Padding;
        public static byte[] Decrypt(byte[] data)
        {
            var block = new byte[PaddedSize];
            Buffer.BlockCopy(data, 0, block, 0, Size);

            block = DecryptData(block, Key, IV, BlockSize.Block256, KeySize.Key256, EncryptionMode.ModeCBC);

            using var ms = new MemoryStream();
            ms.Write(block);
            ms.Write(data, PaddedSize, data.Length - PaddedSize);

            return ms.ToArray();
        }
    }
}
