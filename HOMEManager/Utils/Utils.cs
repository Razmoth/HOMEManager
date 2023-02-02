using System.Text;
using Newtonsoft.Json;
using System.Security.Cryptography;
namespace HOMEManager
{
    public static class Utils
    {
        private const int MDVR = 50900;
        private const string CDN = "ph.prd.cdnfiles";
        private const string MD = "md/dro/diff/MD_AssetbundleDLPackVer.snd";
        private readonly static string OUTPUT = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bundles");

        static Utils()
        {
            Directory.CreateDirectory(OUTPUT);
        }

        public static string GetMDPath => $"{CalculateFileHash(MDVR)}/{MD}";
        public static string GetBundlePath(int id, string name) => $"{CalculateFileHash(id)}/dro/{name}.abap";
        public static string GetBundleLocalPath(string name) => Path.Combine(OUTPUT, $"{name}.abap");
        public static string GetFilePath(string bundle, string name)
        {
            var dir = Path.Combine(OUTPUT, bundle);
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"{name}.unity3d");
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
    }
}
