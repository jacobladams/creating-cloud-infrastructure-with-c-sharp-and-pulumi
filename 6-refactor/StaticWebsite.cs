using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using System.IO;
using System.Linq;
using MimeTypes;

class StaticWebsite
{
    public Output<string> WebsiteUrl {get; private set;}

    public StaticWebsite(Input<string> resourceGroupName, Input<string> apiUrl, Dictionary<string, string> commonTags)
    {
        
        // Create an Azure resource (Storage Account)
        var storageAccount = new StorageAccount("stweb", new StorageAccountArgs
        {
            ResourceGroupName = resourceGroupName,
            Sku = new SkuArgs
            {
                Name = SkuName.Standard_LRS
            },
            Kind = Kind.StorageV2,
            AccessTier = AccessTier.Hot,
            AllowBlobPublicAccess = true,
            EnableHttpsTrafficOnly = true,
            MinimumTlsVersion = MinimumTlsVersion.TLS1_2,
            Tags = commonTags
        });

        var staticWebsite = new StorageAccountStaticWebsite($"stapp-{Pulumi.Deployment.Instance.ProjectName}", new StorageAccountStaticWebsiteArgs
        {
            ResourceGroupName = resourceGroupName,
            AccountName = storageAccount.Name,
            IndexDocument = "index.html"
        });

        string webFiles = Path.GetFullPath(@"..\CompanyDirectory.Web\bin\release\net6.0\publish\wwwroot");

        new DirectoryInfo(webFiles).EnumerateFiles("*.*", SearchOption.AllDirectories)
              .Select(file => new Blob(Path.GetRelativePath(webFiles, file.FullName),
                    new BlobArgs
                    {
                        ResourceGroupName = resourceGroupName,
                        AccountName = storageAccount.Name,
                        ContainerName = staticWebsite.ContainerName,
                        Type = BlobType.Block,
                        Source = new FileAsset(file.FullName),
                        ContentType = MimeTypeMap.GetMimeType(file.Extension)
                    })
             ).ToList();

        WebsiteUrl = storageAccount.PrimaryEndpoints.Apply(primaryEndpoints => primaryEndpoints.Web);

        new Blob("settings.json", new BlobArgs
        {
            ResourceGroupName = resourceGroupName,
            AccountName = storageAccount.Name,
            ContainerName = staticWebsite.ContainerName,
            Type = BlobType.Block,
            Source = apiUrl.ToOutput().Apply(url => (Pulumi.AssetOrArchive)new StringAsset($"{{\"api\":\"{url}\"}}"))
        });
    }
}