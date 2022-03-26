using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.DocumentDB;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;

class ThreeTierComponent : Pulumi.ComponentResource
{
    public ThreeTierComponent(string name, ComponentResourceOptions opts)
        : base("PulumiDemo:CustomComponents:ThreeTierComponent", name, opts)
    {
        // initialization logic.

         var commonTags = new Dictionary<string, string>
        {
            {"workload", Pulumi.Deployment.Instance.ProjectName},
            {"environment", "dev"},
            {"cost center", "IT"},
            {"owner", "Jake Adams"},
            {"demo", "true"}
        };

        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup($"rg-{name}", new ResourceGroupArgs
        {
            Tags = commonTags
        });

        var cosmos = new Cosmos(resourceGroup.Name, name, this, commonTags);

        Output<string> connectionString = CreateCosmosConnectionString(resourceGroup.Name, cosmos.DatabaseAccountName);



        var function = new Function(resourceGroup.Name, connectionString, this, commonTags);

        var staticWebsite = new StaticWebsite(resourceGroup.Name, name, this, function.Url, commonTags);

        // this.Endpoint = function.Url;

        // this.WebsiteUrl = staticWebsite.WebsiteUrl;

        // Signal to the UI that this resource has completed construction.
        // this.RegisterOutputs();

        this.RegisterOutputs(new Dictionary<string, object>
        {
            { "CosmosConnectionString", connectionString },
            { "Endpoint", function.Url },
            { "WebsiteUrl", staticWebsite.WebsiteUrl }
        });

    }

    //  [Output]
    // public Output<string> WebsiteUrl { get; set; }

    // [Output]
    // public Output<string> ConnectionString { get; set; }

    // [Output]
    // public Output<string> Endpoint { get; set; }

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
}
