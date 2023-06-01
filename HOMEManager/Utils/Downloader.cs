namespace HOMEManager
{
    public class DLLManager
    {
        public const string ABAP_API = "https://resource.pokemon-home.com";
        public const string Mitake_API = "https://d2gf339i9nrwgc.cloudfront.net";
        
        private readonly Uri Api;
        private readonly HttpClient Client;
        public DLLManager(string url)
        {
            Api = new Uri(url);
            Client = new HttpClient
            {
                BaseAddress = Api
            };
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 6.0; Windows 98; Trident/5.1)");
        }

        public async Task<byte[]> DownloadFile(string path)
        {
            byte[] data = Array.Empty<byte>();
            if (Uri.TryCreate(Api, path, out var uri))
            {
                Console.WriteLine($"Downloading {Path.GetFileName(uri.AbsolutePath)}...");
                try
                {
                    data = await Client.GetByteArrayAsync(uri);
                }
                catch(Exception)
                {
                    Console.WriteLine($"Error while downloading {Path.GetFileName(uri.LocalPath)}");
                }
            }
            return data;
        }
    }

    [Flags]
    public enum DownloadMode
    {
        None,
        ABA,
        Mitake,
        All = ABA | Mitake
    }
}
