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
        var resourceGroup = new ResourceGroup("rg-pulumi-demo", new ResourceGroupArgs
        {
            Tags = new Dictionary<string, string>
            {
                {"workload", "pulumi demo"},
                {"environment", "dev"},
                {"cost center", "IT"},
                {"owner", "Jake Adams"},
                {"demo", "true"}
            }
        });

        // Create an Azure resource (Storage Account)
        var storageAccount = new StorageAccount("sa", new StorageAccountArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Sku = new SkuArgs
            {
                Name = SkuName.Standard_LRS
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

        // var storageContainer = new BlobContainer("$web", new BlobContainerArgs
        // {
        //     ResourceGroupName = resourceGroup.Name,
        //     AccountName = storageAccount.Name,
        //     PublicAccess = PublicAccess.Container,
        // });


        var indexBlob = new Blob("index.html", new BlobArgs
        {
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
