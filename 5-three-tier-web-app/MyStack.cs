using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;

class MyStack : Stack
{
    public MyStack()
    {
        var _commonTags = new Dictionary<string, string>
        {
            {"workload", Pulumi.Deployment.Instance.ProjectName},
            {"environment", "dev"},
            {"cost center", "IT"},
            {"owner", "Jake Adams"},
            {"demo", "true"}
        };

        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup("rg-pulumi-demo");

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

        var staticWebsite = new StorageAccountStaticWebsite("stapp-company-directory", new StorageAccountStaticWebsiteArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = storageAccount.Name,
            IndexDocument = "index.html"

        });

     
        var indexBlob = new Blob("index.html", new BlobArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = storageAccount.Name,
            ContainerName = staticWebsite.ContainerName,
            Type = BlobType.Block,
            Source = new StringAsset("<html><body><h1>Hello World</h1></body></html>"),
            ContentType = "text/html"
        });


       var appServicePlan = new AppServicePlan("plan-", new AppServicePlanArgs
        {
            ResourceGroupName = resourceGroup.Name,

            // Run on Linux
            Kind = "Linux",

            // Consumption plan SKU
            Sku = new SkuDescriptionArgs
            {
                Tier = "Dynamic",
                Name = "Y1"
            },

            // For Linux, you need to change the plan to have Reserved = true property.
            Reserved = true
        });

        var container = new BlobContainer("zips-container", new BlobContainerArgs
        {
            AccountName = storageAccount.Name,
            PublicAccess = PublicAccess.None,
            ResourceGroupName = resourceGroup.Name,
        });

        var blob = new Blob("zip", new BlobArgs
        {
            AccountName = storageAccount.Name,
            ContainerName = container.Name,
            ResourceGroupName = resourceGroup.Name,
            Type = BlobType.Block,
            Source = new FileArchive("../CompanyDirectory.API/bin/debug")
        });

        var codeBlobUrl = SignedBlobReadUrl(blob, container, storageAccount, resourceGroup);

        // Application insights
        // var appInsights = new Component("appInsights", new ComponentArgs
        // {
        //     ApplicationType = ApplicationType.Web,
        //     Kind = "web",
        //     ResourceGroupName = resourceGroup.Name,
        // });


        var app = new WebApp("func-", new WebAppArgs
        {
            Kind = "FunctionApp",
            ResourceGroupName = resourceGroup.Name,
            ServerFarmId = appServicePlan.Id,
            SiteConfig = new SiteConfigArgs
            {
                AppSettings = new[]
                {
                    new NameValuePairArgs{
                        Name = "AzureWebJobsStorage",
                        Value = GetConnectionString(resourceGroup.Name, storageAccount.Name),
                    },
                    // new NameValuePairArgs{
                    //     Name = "runtime",
                    //     Value = "python",
                    // },
                    // new NameValuePairArgs{
                    //     Name = "FUNCTIONS_WORKER_RUNTIME",
                    //     Value = "python",
                    // },
                    // new NameValuePairArgs{
                    //     Name = "WEBSITE_RUN_FROM_PACKAGE",
                    //     Value = codeBlobUrl,
                    // },
                    // new NameValuePairArgs{
                    //     Name = "APPLICATIONINSIGHTS_CONNECTION_STRING",
                    //     Value = Output.Format($"InstrumentationKey={appInsights.InstrumentationKey}"),
                    // },
                },
            },
        });

        // this.Endpoint = Output.Format($"https://{app.DefaultHostName}/api/Hello?name=Pulumi");


        this.WebsiteUrl = storageAccount.PrimaryEndpoints.Apply(primaryEndpoints => primaryEndpoints.Web);
    }

     private static Output<string> GetConnectionString(Input<string> resourceGroupName, Input<string> accountName)
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

    [Output]
    public Output<string> WebsiteUrl { get; set; }
}
