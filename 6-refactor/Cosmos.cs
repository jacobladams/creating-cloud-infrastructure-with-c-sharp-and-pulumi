using Pulumi;
using Pulumi.AzureNative.DocumentDB;
using System.Collections.Generic;

class Cosmos
{
    public Output<string> ConnectionString { get; private set; }

    public Cosmos(Input<string> resourceGroupName, Dictionary<string, string> commonTags)
    {
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
            ResourceGroupName = resourceGroupName,
            Tags = commonTags,
        });

        var mongoDBResourceMongoDBDatabase = new MongoDBResourceMongoDBDatabase("directory", new MongoDBResourceMongoDBDatabaseArgs
        {
            AccountName = databaseAccount.Name,
            Resource = new Pulumi.AzureNative.DocumentDB.Inputs.MongoDBDatabaseResourceArgs
            {
                Id = "directory",

            },
            ResourceGroupName = resourceGroupName,
            Tags = commonTags
        });

        var mongoDBResourceMongoDBCollection = new MongoDBResourceMongoDBCollection("personnel", new MongoDBResourceMongoDBCollectionArgs
        {
            AccountName = databaseAccount.Name,
            DatabaseName = mongoDBResourceMongoDBDatabase.Name,
            Resource = new Pulumi.AzureNative.DocumentDB.Inputs.MongoDBCollectionResourceArgs
            {
                Id = "personnel",
            },
            ResourceGroupName = resourceGroupName,
            Tags = commonTags,
        });

        ConnectionString = CreateCosmosConnectionString(resourceGroupName, databaseAccount.Name);

    }

    private static Output<string> CreateCosmosConnectionString(Input<string> resourceGroupName, Input<string> databaseAccountName)
    {
        return Output.All(databaseAccountName, resourceGroupName).Apply(items =>
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
}