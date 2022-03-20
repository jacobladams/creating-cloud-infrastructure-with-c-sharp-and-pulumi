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
        });

        var storageContainer = new BlobContainer("test", new BlobContainerArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = storageAccount.Name,
        });
        
        var indexBlob = new Blob("test.txt", new BlobArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = storageAccount.Name,
            ContainerName = storageContainer.Name,
            Type = BlobType.Block,
            Source = new StringAsset("test 123"),
        });
    }
}
