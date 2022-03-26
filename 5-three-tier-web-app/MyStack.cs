using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.DocumentDB;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using System.IO;
using System.Linq;
using MimeTypes;

class MyStack : Stack
{
    public MyStack()
    {
        var commonTags = new Dictionary<string, string>
        {
            {"workload", Pulumi.Deployment.Instance.ProjectName},
            {"environment", "dev"},
            {"cost center", "IT"},
            {"owner", "Jake Adams"},
            {"demo", "true"}
        };

        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup($"rg-{Pulumi.Deployment.Instance.ProjectName}", new ResourceGroupArgs
        {
            Tags = commonTags
        });

        var databaseAccount = new DatabaseAccount($"cosmos-{Pulumi.Deployment.Instance.ProjectName}", new DatabaseAccountArgs
        {
            ApiProperties = new Pulumi.AzureNative.DocumentDB.Inputs.ApiPropertiesArgs
            {
                ServerVersion = "4.0",
            },
            ConsistencyPolicy = new Pulumi.AzureNative.DocumentDB.Inputs.ConsistencyPolicyArgs
            {
                DefaultConsistencyLevel = DefaultConsistencyLevel.Session,
            },
            DatabaseAccountOfferType = DatabaseAccountOfferType.Standard,
            EnableFreeTier = true,
            Kind = "MongoDB",
            Locations =
            {
                new Pulumi.AzureNative.DocumentDB.Inputs.LocationArgs
                {
                    FailoverPriority = 0,
                    IsZoneRedundant = false,
                    LocationName = "eastus",
                },
                new Pulumi.AzureNative.DocumentDB.Inputs.LocationArgs
                {
                    FailoverPriority = 1,
                    IsZoneRedundant = false,
                    LocationName = "southcentralus",
                },
            },
            PublicNetworkAccess = "Enabled",
            ResourceGroupName = resourceGroup.Name,
            Tags = commonTags,
        });

        var mongoDBResourceMongoDBDatabase = new MongoDBResourceMongoDBDatabase("database", new MongoDBResourceMongoDBDatabaseArgs
        {
            AccountName = databaseAccount.Name,
            DatabaseName = "directory",
            Resource = new Pulumi.AzureNative.DocumentDB.Inputs.MongoDBDatabaseResourceArgs
            {
                Id = "directory",

            },
            ResourceGroupName = resourceGroup.Name,
            Tags = commonTags
        });

        var mongoDBResourceMongoDBCollection = new MongoDBResourceMongoDBCollection("collection", new MongoDBResourceMongoDBCollectionArgs
        {
            AccountName = databaseAccount.Name,
            CollectionName = "items",
            DatabaseName = mongoDBResourceMongoDBDatabase.Name,
            Resource = new Pulumi.AzureNative.DocumentDB.Inputs.MongoDBCollectionResourceArgs
            {
                Id = "items",
            },
            ResourceGroupName = resourceGroup.Name,
            Tags = commonTags,
        });

        this.ConnectionString = CreateCosmosConnectionString(resourceGroup.Name, databaseAccount.Name);

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
            Reserved = true,
            Tags = commonTags
        });

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
            ResourceGroupName = resourceGroup.Name,
            Sku = new SkuArgs
            {
                Name = SkuName.Standard_LRS,
            },
            Kind = Pulumi.AzureNative.Storage.Kind.StorageV2,
            Tags = commonTags,
        });

        var api = new WebApp("func-", new WebAppArgs
        {
            Kind = "FunctionApp",
            ResourceGroupName = resourceGroup.Name,
            ServerFarmId = appServicePlan.Id,
            HttpsOnly = true,

            SiteConfig = new SiteConfigArgs
            {
                AppSettings = new[]
                {
                    new NameValuePairArgs{
                        Name = "AzureWebJobsStorage",
                        Value = GetStorageConnectionString(resourceGroup.Name, apistorageAccount.Name),
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
                        Value = ConnectionString,
                    },
                },
            },
            Tags = commonTags
        });

        // Create an Azure resource (Storage Account)
        var storageAccount = new StorageAccount("stweb", new StorageAccountArgs
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
            MinimumTlsVersion = MinimumTlsVersion.TLS1_2,
            Tags = commonTags
        });

        var staticWebsite = new StorageAccountStaticWebsite($"stapp-{Pulumi.Deployment.Instance.ProjectName}", new StorageAccountStaticWebsiteArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = storageAccount.Name,
            IndexDocument = "index.html"
        });

        string webFiles = Path.GetFullPath(@"..\CompanyDirectory.Web\bin\release\net6.0\publish\wwwroot");

        new DirectoryInfo(webFiles).EnumerateFiles("*.*", SearchOption.AllDirectories)
              .Select(file => new Blob(Path.GetRelativePath(webFiles, file.FullName),
                    new BlobArgs
                    {
                        ResourceGroupName = resourceGroup.Name,
                        AccountName = storageAccount.Name,
                        ContainerName = staticWebsite.ContainerName,
                        Type = BlobType.Block,
                        Source = new FileAsset(file.FullName),
                        ContentType = MimeTypeMap.GetMimeType(file.Extension)
                    })
             ).ToList();

        this.WebsiteUrl = storageAccount.PrimaryEndpoints.Apply(primaryEndpoints => primaryEndpoints.Web);

        new Blob("settings.json", new BlobArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = storageAccount.Name,
            ContainerName = staticWebsite.ContainerName,
            Type = BlobType.Block,
            Source = api.DefaultHostName.Apply(hostName => (Pulumi.AssetOrArchive)new StringAsset($"{{\"api\":\"https://{hostName}/api/Hello?name=Pulumi\"}}")),
            ContentType = "application/json"
        });

        this.Endpoint = Output.Format($"https://{api.DefaultHostName}/api/Hello?name=Pulumi");

        this.WebsiteUrl = storageAccount.PrimaryEndpoints.Apply(primaryEndpoints => primaryEndpoints.Web);
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

    private static Output<string> CreateCosmosConnectionString(Input<string> resourceGroupName, Input<string> databaseAccountName)
    {
        return  Output.All(databaseAccountName, resourceGroupName).Apply(items =>
        {
            string accountName = items[0];
            string resourceGroupName = items[1];
            return Pulumi.AzureNative.DocumentDB.ListDatabaseAccountConnectionStrings.Invoke(new ListDatabaseAccountConnectionStringsInvokeArgs
            {
                AccountName = accountName,
                ResourceGroupName = resourceGroupName
            }).Apply(connectionStrings => connectionStrings.ConnectionStrings[0].ConnectionString);
        });
    }

    [Output]
    public Output<string> WebsiteUrl { get; set; }

    [Output]
    public Output<string> ConnectionString { get; set; }

    [Output]
    public Output<string> Endpoint { get; set; }
}
