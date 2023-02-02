using HOMEManager;

using var stream = await DLLManager.DownloadFile(Utils.GetMDPath);
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