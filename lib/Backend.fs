namespace qlib

open Microsoft.WindowsAzure.MobileServices

module Backend =
    let Client = new MobileServiceClient("https://queuedev.azure-mobile.net/", "DRMQvMGssPZvIuEaJGDBGgOfYzsYzB63")
    let Init() = CurrentPlatform.Init()

    let private table = Client.GetTable<QItem>()

    let SaveItem (item : QItem) = table.InsertAsync(item)