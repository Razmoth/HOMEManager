using HOMEManager;
using System.Reflection;

if (args.Length == 1)
{
    try
    {
        Stream stream;
        if (args[0] == "download")
        {
            stream = await DLLManager.DownloadFile(Utils.GetMDPath);
        }
        else
        {
            stream = File.OpenRead(args[0]);
        }
        var items = MDParser<JsonAssetBundleItem>.Read(stream);
        var tasks = new List<Task>();
        foreach (var item in items)
        {
            tasks.Add(Task.Run(() => Process(item)));
        }
        Task.WhenAll(tasks).Wait();

        static async void Process(JsonAssetBundleItem item)
        {
            using var data = await DLLManager.DownloadFile(item.Path);
            ABAPParser.Parse(data, item.nm);
        }
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error: {e}");
    }
}
else
{
    var versionString = Assembly.GetEntryAssembly()?
                                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                                .InformationalVersion
                                .ToString();

    Console.WriteLine(@$"HOMEManager v{versionString}
------------------------

Usage:
  HOMEManager <md_file_path> or (download)");
}
