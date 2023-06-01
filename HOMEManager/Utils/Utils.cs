using System.Text;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.IO.Compression;
using System.Buffers;
using AssetStudio;

namespace HOMEManager
{
    public static class Utils
    {
        private const string CDN = "ph.prd.cdnfiles";
        private const string MD = "md/dro/diff/MD_AssetbundleDLPackVer.snd";
        private const string PN = "MitakeCommon";
        private const string RPN = "Models/Android";
        private readonly static string OUTPUT = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bundles");
        private readonly static AssetsManager AssetsManager = new AssetsManager();

        public static int MDVR = 58800;

        static Utils()
        {
            Directory.CreateDirectory(OUTPUT);
            AssetsManager = new AssetsManager() { Game = GameManager.GetGame(GameType.Normal) };
        }

        public static string GetPNPath => $"{RPN}/{PN}.bin";
        public static string GetMDPath => $"{CalculateFileHash(MDVR)}/{MD}";
        public static string GetBundlePath(int id, string name) => $"{CalculateFileHash(id)}/dro/{name}.abap";
        public static string GetBundleLocalPath(string name) => Path.Combine(OUTPUT, $"{name}.abap");
        public static string GetMitakePath(string project, string name) => Path.Combine(RPN, project, name);
        public static string GetMitakeLocalPath(string project, string name) => GetFilePath(project, name);
        public static string GetFilePath(string bundle, string name)
        {
            var path = Path.Combine(OUTPUT, bundle, $"{name}.unity3d");
            var dir = Path.GetDirectoryName(path);
            Directory.CreateDirectory(dir);
            return path;
        }
        public static void WriteToJSON(object obj, string path)
        {
            var str = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.WriteAllText(path, str);
        }
        public static string CalculateFileHash(int id)
        {
            var hash = MD5Hash($"{CDN}/{id}");
            var idx = Convert.ToByte(hash[..1], 16);
            hash = string.Concat(hash[idx..], hash[..idx]);
            return MD5Hash(hash);
        }
        private static string MD5Hash(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            bytes = MD5.HashData(bytes);
            return Convert.ToHexString(bytes).ToLower();
        }
        public static string DeflateJson(byte[] data, uint deflatedSize) => Encoding.UTF8.GetString(Deflate(data, deflatedSize));

        public static byte[] Deflate(byte[] data, uint deflatedSize)
        {
            var deflatedBytes = new byte[deflatedSize];

            using var ms = new MemoryStream(data);
            using var deflateStream = new DeflateStream(ms, CompressionMode.Decompress);
            
            int totalRead = 0;
            while (totalRead < deflatedSize)
            {
                int bytesRead = deflateStream.Read(deflatedBytes.AsSpan(totalRead));
                if (bytesRead == 0) break;
                totalRead += bytesRead;
            }

            if (totalRead !=  deflatedSize)
            {
                throw new Exception($"Invalid data !! Expected {deflatedSize}, Got {totalRead} instaed");
            }

            return deflatedBytes;
        }

        public static bool TryGetManifest(string path, out byte[] data)
        {
            data = Array.Empty<byte>();

            try
            {
                AssetsManager.LoadFiles(path);
                var objects = AssetsManager.assetsFileList.SelectMany(x => x.Objects).ToArray();
                if (objects.Length == 2)
                {
                    if (objects.FirstOrDefault(x => x.type == ClassIDType.TextAsset) is TextAsset textAsset && textAsset.m_Name.Equals(Path.GetFileNameWithoutExtension(path), StringComparison.OrdinalIgnoreCase))
                    {
                        data = textAsset.m_Script;
                        AssetsManager.Clear();
                        return true;
                    }
                }
            }
            catch (Exception e) { }

            AssetsManager.Clear();
            return false;
        }
    }
}
