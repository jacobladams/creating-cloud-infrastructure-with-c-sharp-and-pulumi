using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;

class MyStack : Stack
{
    public MyStack()
    {
        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup("rg-pulumi-demo");

        // Create an Azure resource (Storage Account)
        var storageAccount = new StorageAccount("stpulumidemo", new StorageAccountArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Sku = new SkuArgs
            {
                Name = SkuName.Standard_LRS
            },
            Kind = Kind.StorageV2,
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

        var indexBlob = new Blob("index.html", new BlobArgs
        {
            // BlobName = staticWebsite.IndexDocument.Apply(i=>i.Value),
            ResourceGroupName = resourceGroup.Name,
            AccountName = storageAccount.Name,
            ContainerName = staticWebsite.ContainerName,
            Type = BlobType.Block,
            Source = new StringAsset("<html><body><h1>Hello World</h1></body></html>"),
            ContentType = "text/html"
        });

        this.WebsiteUrl = storageAccount.PrimaryEndpoints.Apply(primaryEndpoints => primaryEndpoints.Web);
    }

    [Output]
    public Output<string> WebsiteUrl { get; set; }
}
