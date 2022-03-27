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
    public ThreeTierComponent(string name, Dictionary<string, string> tags, ComponentResourceOptions opts)
        : base("PulumiDemo:CustomComponents:ThreeTierComponent", name, opts)
    {

        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup($"rg-{name}", new ResourceGroupArgs
        {
            Tags = tags
        });

        var cosmos = new Cosmos(resourceGroup.Name, name, this, tags);

        CosmosConnectionString = CreateCosmosConnectionString(resourceGroup.Name, cosmos.DatabaseAccountName);

        var function = new Function(resourceGroup.Name, CosmosConnectionString, this, tags);

        var staticWebsite = new StaticWebsite(resourceGroup.Name, name, this, function.Url, tags);

        Endpoint = function.Url;

        WebsiteUrl = staticWebsite.WebsiteUrl;

        // Signal to the UI that this resource has completed construction.
        this.RegisterOutputs(new Dictionary<string, object?>
        {
            { "CosmosConnectionString", CosmosConnectionString },
            { "Endpoint", Endpoint },
            { "WebsiteUrl", WebsiteUrl }
        });

    }

    public Output<string> WebsiteUrl { get; set; }

    public Output<string> CosmosConnectionString { get; set; }

    public Output<string> Endpoint { get; set; }

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
