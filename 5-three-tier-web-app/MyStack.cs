using System.Resources;
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


        var databaseAccount = new DatabaseAccount("cosmos-directory", new DatabaseAccountArgs
        {
            // AccountName = "ddb1",
            ApiProperties = new Pulumi.AzureNative.DocumentDB.Inputs.ApiPropertiesArgs
            {
                ServerVersion = "4.0",
            },
            // BackupPolicy = new Pulumi.AzureNative.DocumentDB.Inputs.PeriodicModeBackupPolicyArgs
            // {
            //     PeriodicModeProperties = new Pulumi.AzureNative.DocumentDB.Inputs.PeriodicModePropertiesArgs
            //     {
            //         BackupIntervalInMinutes = 240,
            //         BackupRetentionIntervalInHours = 8,
            //     },
            //     Type = "Periodic",
            // },
            // ConsistencyPolicy = new Pulumi.AzureNative.DocumentDB.Inputs.ConsistencyPolicyArgs
            // {
            //     DefaultConsistencyLevel = DefaultConsistencyLevel.BoundedStaleness,
            //     MaxIntervalInSeconds = 10,
            //     MaxStalenessPrefix = 200,
            // },
            ConsistencyPolicy = new Pulumi.AzureNative.DocumentDB.Inputs.ConsistencyPolicyArgs
            {
                DefaultConsistencyLevel = DefaultConsistencyLevel.Session,
                // MaxIntervalInSeconds = 10,
                // MaxStalenessPrefix = 200,
            },
            // Cors = 
            // {
            //     new Pulumi.AzureNative.DocumentDB.Inputs.CorsPolicyArgs
            //     {
            //         AllowedOrigins = "https://test",
            //     },
            // },
            DatabaseAccountOfferType = DatabaseAccountOfferType.Standard,
            // DefaultIdentity = "FirstPartyIdentity",
            // EnableAnalyticalStorage = true,
            EnableFreeTier = true,
            // Identity = new Pulumi.AzureNative.DocumentDB.Inputs.ManagedServiceIdentityArgs
            // {
            //     Type = "SystemAssigned,UserAssigned",
            //     UserAssignedIdentities = 
            //     {
            //         { "/subscriptions/fa5fc227-a624-475e-b696-cdd604c735bc/resourceGroups/eu2cgroup/providers/Microsoft.ManagedIdentity/userAssignedIdentities/id1",  },
            //     },
            // },
            // IpRules = 
            // {
            //     new AzureNative.DocumentDB.Inputs.IpAddressOrRangeArgs
            //     {
            //         IpAddressOrRange = "23.43.230.120",
            //     },
            //     new AzureNative.DocumentDB.Inputs.IpAddressOrRangeArgs
            //     {
            //         IpAddressOrRange = "110.12.240.0/12",
            //     },
            // },
            // IsVirtualNetworkFilterEnabled = true,
            // KeyVaultKeyUri = "https://myKeyVault.vault.azure.net",
            Kind = "MongoDB",
            // Location = "westus",
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
            // NetworkAclBypass = "AzureServices",
            // NetworkAclBypassResourceIds = 
            // {
            //     "/subscriptions/subId/resourcegroups/rgName/providers/Microsoft.Synapse/workspaces/workspaceName",
            // },
            PublicNetworkAccess = "Enabled",
            ResourceGroupName = resourceGroup.Name,
            Tags = commonTags,
            //             VirtualNetworkRules = 
            //             {
            //                 new AzureNative.DocumentDB.Inputs.VirtualNetworkRuleArgs
            //                 {
            //                     Id = "/subscriptions/subId/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet1/subnets/subnet1",
            //                     IgnoreMissingV

            // NetServiceEndpoint = false,
            //                 },
            //             },
        });

        var mongoDBResourceMongoDBDatabase = new MongoDBResourceMongoDBDatabase("mongoDBResourceMongoDBDatabase", new MongoDBResourceMongoDBDatabaseArgs
        {
            AccountName = databaseAccount.Name,
            DatabaseName = "directory",
            // Location = "West US",
            // Options = ,
            Resource = new Pulumi.AzureNative.DocumentDB.Inputs.MongoDBDatabaseResourceArgs
            {
                Id = "directory",

            },
            ResourceGroupName = resourceGroup.Name,
            Tags = commonTags
        });

        var mongoDBResourceMongoDBCollection = new MongoDBResourceMongoDBCollection("mongoDBResourceMongoDBCollection", new MongoDBResourceMongoDBCollectionArgs
        {
            AccountName = databaseAccount.Name,
            CollectionName = "items",
            DatabaseName = mongoDBResourceMongoDBDatabase.Name,
            // Location = "West US",
            // Options = ,
            Resource = new Pulumi.AzureNative.DocumentDB.Inputs.MongoDBCollectionResourceArgs
            {
                Id = "items",
                //     Indexes = 
                //     {
                //         new AzureNative.DocumentDB.Inputs.MongoIndexArgs
                //         {
                //             Key = new AzureNative.DocumentDB.Inputs.MongoIndexKeysArgs
                //             {
                //                 Keys = 
                //                 {
                //                     "testKey",
                //                 },
                //             },
                //             Options = new AzureNative.DocumentDB.Inputs.MongoIndexOptionsArgs
                //             {
                //                 ExpireAfterSeconds = 100,
                //                 Unique = true,
                //             },
                //         },
                //     },
                //     ShardKey = 
                //     {
                //         { "testKey", "Hash" },
                //     },
            },
            ResourceGroupName = resourceGroup.Name,
            Tags = commonTags,
        });

        this.ConnectionString = Output.All(databaseAccount.Name, resourceGroup.Name).Apply(items =>
        {
            string accountName = items[0];
            string resourceGroupName = items[1];
            return Pulumi.AzureNative.DocumentDB.ListDatabaseAccountConnectionStrings.Invoke(new ListDatabaseAccountConnectionStringsInvokeArgs
            {
                AccountName = accountName,
                ResourceGroupName = resourceGroupName
            }).Apply(connectionStrings => connectionStrings.ConnectionStrings[0].ConnectionString);
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

        // Application insights
        // var appInsights = new Component("appInsights", new ComponentArgs
        // {
        //     ApplicationType = ApplicationType.Web,
        //     Kind = "web",
        //     ResourceGroupName = resourceGroup.Name,
        // });


        var apistorageAccount = new StorageAccount("stapi", new StorageAccountArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Sku = new SkuArgs
            {
                Name = SkuName.Standard_LRS,
            },
            Kind = Pulumi.AzureNative.Storage.Kind.StorageV2,
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
                        Value = GetConnectionString(resourceGroup.Name, apistorageAccount.Name),
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
        var storageAccount = new StorageAccount("stdirectory", new StorageAccountArgs
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

        var staticWebsite = new StorageAccountStaticWebsite("stapp-company-directory", new StorageAccountStaticWebsiteArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = storageAccount.Name,
            IndexDocument = "index.html"
        });

        string webFiles = Path.GetFullPath(@"..\CompanyDirectory.Web\bin\release\net6.0\publish\wwwroot");

        System.Console.WriteLine(webFiles);

        // new DirectoryInfo(webFiles).EnumerateFiles("*.*", SearchOption.AllDirectories)
        //       .Select(file => new Blob(Path.GetRelativePath(webFiles, file.FullName),
        //          new BlobArgs
        //          {
        //              ResourceGroupName = resourceGroup.Name,
        //              AccountName = storageAccount.Name,
        //              ContainerName = staticWebsite.ContainerName,
        //              Type = BlobType.Block,
        //              Source = new FileAsset(file.FullName),
        //              ContentType = MimeTypeMap.GetMimeType(file.Extension)
        //          })
        //      );
        // Directory.EnumerateFiles(webFiles, "*.*", new EnumerationOptions(){RecurseSubdirectories= true})
        new DirectoryInfo(webFiles).EnumerateFiles("*.*", SearchOption.AllDirectories)
              .Select(file =>
              {
                  System.Console.WriteLine(file.FullName);
                  return new Blob(Path.GetRelativePath(webFiles, file.FullName),
                    new BlobArgs
                    {
                        ResourceGroupName = resourceGroup.Name,
                        AccountName = storageAccount.Name,
                        ContainerName = staticWebsite.ContainerName,
                        Type = BlobType.Block,
                        Source = new FileAsset(file.FullName),
                        ContentType = MimeTypeMap.GetMimeType(file.Extension)
                    });

              }
             ).ToList();

        this.WebsiteUrl = storageAccount.PrimaryEndpoints.Apply(primaryEndpoints => primaryEndpoints.Web);



        // var indexBlob = new Blob("index.html", new BlobArgs
        // {
        //     ResourceGroupName = resourceGroup.Name,
        //     AccountName = storageAccount.Name,
        //     ContainerName = staticWebsite.ContainerName,
        //     Type = BlobType.Block,
        //     Source = new StringAsset("<html><body><h1>Hello World</h1></body></html>"),
        //     ContentType = "text/html"
        // });

        // string apiUrl = Output.Format($"https://{api.DefaultHostName}/api/Hello?name=Pulumi") ;

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


        // {
        //     public static Task<ListDatabaseAccountConnectionStringsResult> InvokeAsync(ListDatabaseAccountConnectionStringsArgs args, InvokeOptions? opts = null)
        //     public static Output<ListDatabaseAccountConnectionStringsResult> Invoke(ListDatabaseAccountConnectionStringsInvokeArgs args, InvokeOptions? opts = null)
        // }
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

    [Output]
    public Output<string> ConnectionString { get; set; }

    [Output]
    public Output<string> Endpoint { get; set; }
}
