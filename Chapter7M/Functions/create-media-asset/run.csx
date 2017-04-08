using Microsoft.WindowsAzure.MediaServices.Client;

private static readonly string mediaAccountName = Environment.GetEnvironmentVariable("MediaServicesAccountName");
private static readonly string mediaAccountKey = Environment.GetEnvironmentVariable("MediaServicesAccountKey");

public static void Run(Stream myBlob, string name, out string queueItem, TraceWriter log)
{
    log.Info($"create-media-asset received file {name}");
    log.Info($"Using Media Services Account {mediaAccountName}");

    MediaServicesCredentials credentials = new MediaServicesCredentials(mediaAccountName, mediaAccountKey);
    CloudMediaContext context = new CloudMediaContext(credentials);
    IAsset newAsset = context.Assets.Create(name, AssetCreationOptions.None);

    log.Info($"Asset Id = {newAsset.Id}, Path = {newAsset.Uri}");
    queueItem = $"id={newAsset.Id};path={newAsset.Uri.Segments[1]}";
}