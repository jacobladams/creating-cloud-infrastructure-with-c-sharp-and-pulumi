using System;
using System.Linq;
using System.IO;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using MimeTypes;

class MyStack : Stack
{
    public MyStack()
    {
        var config = new Pulumi.Config();
        // string storageSku = Enum.Parse<AccessTier>config.RequireObject<SkuName>("storage-sku");
        // string accessTier = config.Get("access-tier") ?? "Hot";

        // SkuName storageSku = config.RequireObject<SkuName>("storage-sku");
        string storageSku = config.Require("storage-sku");

        // AccessTier accessTier = config.GetObject<AccessTier?>("access-tier") ?? AccessTier.Hot;


        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup($"rg-{Pulumi.Deployment.Instance.ProjectName}");

        // Create an Azure resource (Storage Account)
        var storageAccount = new StorageAccount("stpulumidemo", new StorageAccountArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Sku = new SkuArgs
            {
                // Name = SkuName.Standard_LRS
                Name = storageSku
            },
            Kind = Kind.StorageV2,
            AccessTier = AccessTier.Hot,
            AllowBlobPublicAccess = true,
            EnableHttpsTrafficOnly = true,
            MinimumTlsVersion = MinimumTlsVersion.TLS1_2
        });

        var staticWebsite = new StorageAccountStaticWebsite("stapp-pulumi-demo", new StorageAccountStaticWebsiteArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = storageAccount.Name,
            IndexDocument = "index.html"

        });

        Directory.EnumerateFiles("wwwroot").Select(file => new Blob(Path.GetFileName(file), new BlobArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = storageAccount.Name,
            ContainerName = staticWebsite.ContainerName,
            Type = BlobType.Block,
            Source = new FileAsset(file),
            ContentType = MimeTypeMap.GetMimeType(Path.GetExtension(file))
        }));

        this.WebsiteUrl = storageAccount.PrimaryEndpoints.Apply(primaryEndpoints => primaryEndpoints.Web);
    }

    [Output]
    public Output<string> WebsiteUrl { get; set; }

}
