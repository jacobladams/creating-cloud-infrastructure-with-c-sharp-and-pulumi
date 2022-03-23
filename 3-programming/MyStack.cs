using System.Linq;
using System.IO;
using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using MimeTypes;

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

        var wwwRoot = new DirectoryInfo("wwwroot");
        IEnumerable<FileInfo> files = wwwRoot.EnumerateFiles("*.*", SearchOption.AllDirectories);
        foreach(FileInfo file in files)
        {

            new Blob(Path.GetRelativePath(wwwRoot.FullName, file.FullName), new BlobArgs
            {
                ResourceGroupName = resourceGroup.Name,
                AccountName = storageAccount.Name,
                ContainerName = staticWebsite.ContainerName,
                Type = BlobType.Block,
                Source = new FileAsset(file.FullName),
                ContentType = GetMimeType(file.Extension)
                // ContentType = MimeTypeMap.GetMimeType(file.Extension)
            });
        }

        // new DirectoryInfo("wwwroot").EnumerateFiles("*.*", SearchOption.AllDirectories)
        //     .Select(file=> new Blob(Path.GetRelativePath(wwwRoot.FullName, file.FullName), 
                //new BlobArgs
                // {
                //     ResourceGroupName = resourceGroup.Name,
                //     AccountName = storageAccount.Name,
                //     ContainerName = staticWebsite.ContainerName,
                //     Type = BlobType.Block,
                //     Source = new FileAsset(file.FullName),
                //     ContentType = MimeTypeMap.GetMimeType(file.Extension)
        // }));

        this.WebsiteUrl = storageAccount.PrimaryEndpoints.Apply(primaryEndpoints => primaryEndpoints.Web);
    }

    [Output]
    public Output<string> WebsiteUrl { get; set; }

    private string GetMimeType(string extension)
    {
        switch (extension.ToLower())
        {
             case ".html":
                return "text/html";
             case ".css":
                return "text/css";
            case ".js":
                return "application/javascript";
            case ".jpg":
                return "image/jpeg";
            default:
                return "application/octet-stream";
        }
    }
}
