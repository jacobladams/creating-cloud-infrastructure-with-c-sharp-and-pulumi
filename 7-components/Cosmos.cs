using Pulumi;
using Pulumi.AzureNative.DocumentDB;
using System.Collections.Generic;

class Cosmos
{
    public Output<string> ConnectionString { get; private set; }

    public Cosmos(Input<string> resourceGroupName, string name, Pulumi.Resource parent, Dictionary<string, string> commonTags)
    {

        var databaseAccount = new DatabaseAccount($"cosmos-{name}", new DatabaseAccountArgs
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
            Tags = commonTags
        }, new CustomResourceOptions { Parent = parent });

        var mongoDBResourceMongoDBDatabase = new MongoDBResourceMongoDBDatabase("database", new MongoDBResourceMongoDBDatabaseArgs
        {
            AccountName = databaseAccount.Name,
            DatabaseName = "directory",
            Resource = new Pulumi.AzureNative.DocumentDB.Inputs.MongoDBDatabaseResourceArgs
            {
                Id = "directory",

            },
            ResourceGroupName = resourceGroupName,
            Tags = commonTags
        }, new CustomResourceOptions { Parent = parent });

        var mongoDBResourceMongoDBCollection = new MongoDBResourceMongoDBCollection("collection", new MongoDBResourceMongoDBCollectionArgs
        {
            AccountName = databaseAccount.Name,
            CollectionName = "items",
            DatabaseName = mongoDBResourceMongoDBDatabase.Name,
            Resource = new Pulumi.AzureNative.DocumentDB.Inputs.MongoDBCollectionResourceArgs
            {
                Id = "items",
            },
            ResourceGroupName = resourceGroupName,
            Tags = commonTags
        }, new CustomResourceOptions { Parent = parent });

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