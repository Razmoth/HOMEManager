using HOMEManager;
using System.Reflection;

if (args.Length >= 1 && Enum.TryParse<DownloadMode>(args[0], true, out var mode))
{
    if (mode.HasFlag(DownloadMode.ABA))
    {
        if (args.Length == 2 && int.TryParse(args[1], out var mdvr))
        {
            Utils.MDVR = mdvr;
        }
        await DownloadABAP();
    }
    if (mode.HasFlag(DownloadMode.Mitake))
    {
        await DownloadMitake();
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
  HOMEManager <mode> (MDVR if {DownloadMode.ABA} mode)
  Available Modes: [{string.Join(',', Enum.GetNames(typeof(DownloadMode)))}]");
}

static async Task<bool> DownloadABAP()
{
    try
    {
        var dllManager = new DLLManager(DLLManager.ABAP_API);
        var data = await dllManager.DownloadFile(Utils.GetMDPath);
        var items = MDParser<JsonAssetBundleItem>.Read(data);
        foreach (var item in items)
        {
            var bytes = await dllManager.DownloadFile(item.Path);
            ABAPParser.Parse(bytes, item.nm);
        }
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error: {e}");
        return false;
    }
    return true;
}

static async Task<bool> DownloadMitake()
{
    try
    {
        var dllManager = new DLLManager(DLLManager.Mitake_API);
        var data = await dllManager.DownloadFile(Utils.GetPNPath);
        return await DownloadManifest(data, dllManager);
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error: {e}");
        return false;
    }
}

static async Task<bool> DownloadManifest(byte[] data, DLLManager dllManager)
{
    var manifest = ABDownloadManifest.Parse(data);

    var i = 0;
    var total = 0;
    var records = new Queue<ABRecord>(manifest.Records);
    while (records.TryDequeue(out var record))
    {
        if (!record.IsStreamingSceneAssetBundle)
        {
            var bytes = await dllManager.DownloadFile(record.Url);
            record.Unpack(bytes);

            if (total < records.Count)
            {
                total = records.Count;
            }

            Console.WriteLine($"[{i}/{total}] Processing {record.Name}...");
            if (Utils.TryGetManifest(record.Path, out var subManifestBytes))
            {
                File.Delete(record.Path);
                var subManifest = ABDownloadManifest.Parse(subManifestBytes);
                subManifest.Records.ToList().ForEach(records.Enqueue);
            }
        }

        i++;
    }

    return true;
}
