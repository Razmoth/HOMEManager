namespace HOMEManager
{
    public static class DLLManager
    {
        private const string API = "https://resource.pokemon-home.com";
        
        private readonly static Uri Api = new(API);
        private readonly static HttpClient client;
        static DLLManager()
        {
            client = new HttpClient
            {
                BaseAddress = Api
            };
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 6.0; Windows 98; Trident/5.1)");
        }

        public static async Task<Stream> DownloadFile(string path)
        {
            MemoryStream data = new();
            if (Uri.TryCreate(Api, path, out var uri))
            {
                Console.WriteLine($"Downloading {Path.GetFileName(uri.AbsolutePath)}...");
                var temp = await client.GetStreamAsync(uri);
                temp.CopyTo(data);
                data.Position = 0;
            }
            return data;
        }
    }
}
