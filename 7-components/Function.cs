using System.Collections.Generic;
using Pulumi;
// using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using System.IO;
using System.Linq;

class Function
{
    public Output<string> Url { get; private set; }

    public Function(Input<string> resourceGroupName, Input<string> mongoConnectionString, Pulumi.Resource parent, Dictionary<string, string> commonTags)
    {
        var appServicePlan = new AppServicePlan("plan-", new AppServicePlanArgs
        {
            ResourceGroupName = resourceGroupName,

            // Run on Linux
            Kind = "Linux",

            // Consumption plan SKU
            Sku = new SkuDescriptionArgs
            {
                Tier = "Dynamic",
                Name = "Y1"
            },

            // For Linux, you need to change the plan to have Reserved = true property.
            Reserved = true,
            Tags = commonTags
        }, new CustomResourceOptions { Parent = parent });

        // var container = new BlobContainer("zips-container", new BlobContainerArgs
        // {
        //     AccountName = storageAccount.Name,
        //     PublicAccess = PublicAccess.None,
        //     ResourceGroupName = resourceGroup.Name,
        // });

        // var blob = new Blob("zip", new BlobArgs
        // {
        //     AccountName = storageAccount.Name,
        //     ContainerName = container.Name,
        //     ResourceGroupName = resourceGroup.Name,
        //     Type = BlobType.Block,
        //     Source = new FileArchive("../CompanyDirectory.API/bin/debug")
        // });

        // var codeBlobUrl = SignedBlobReadUrl(blob, container, storageAccount, resourceGroup);

        var apistorageAccount = new StorageAccount("stapi", new StorageAccountArgs
        {
            ResourceGroupName = resourceGroupName,
            Sku = new SkuArgs
            {
                Name = SkuName.Standard_LRS,
            },
            Kind = Pulumi.AzureNative.Storage.Kind.StorageV2,
            Tags = commonTags
        }, new CustomResourceOptions { Parent = parent });

        var api = new WebApp("func-", new WebAppArgs
        {
            Kind = "FunctionApp",
            ResourceGroupName = resourceGroupName,
            ServerFarmId = appServicePlan.Id,
            HttpsOnly = true,

            SiteConfig = new SiteConfigArgs
            {
                AppSettings = new[]
                {
                    new NameValuePairArgs{
                        Name = "AzureWebJobsStorage",
                        Value = GetStorageConnectionString(resourceGroupName, apistorageAccount.Name),
                    },
                    new NameValuePairArgs{
                        Name = "FUNCTIONS_EXTENSION_VERSION",
                        Value = "~4",
                    },
                    new NameValuePairArgs{
                        Name = "FUNCTIONS_WORKER_RUNTIME",
                        Value = "dotnet",
                    },
                    new NameValuePairArgs{
                        Name = "MongoConnectionString",
                        Value = mongoConnectionString,
                    },
                },
            },
            Tags = commonTags
        }, new CustomResourceOptions { Parent = parent });

        this.Url = Output.Format($"https://{api.DefaultHostName}/api");

    }

    private static Output<string> GetStorageConnectionString(Input<string> resourceGroupName, Input<string> accountName)
    {
        // Retrieve the primary storage account key.
        var storageAccountKeys = ListStorageAccountKeys.Invoke(new ListStorageAccountKeysInvokeArgs
        {
            ResourceGroupName = resourceGroupName,
            AccountName = accountName
        });

        return storageAccountKeys.Apply(keys =>
        {
            var primaryStorageKey = keys.Keys[0].Value;

        // Build the connection string to the storage account.
        return Output.Format($"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={primaryStorageKey}");
        });
    }
}