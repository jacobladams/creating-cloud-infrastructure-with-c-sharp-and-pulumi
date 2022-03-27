using Pulumi;
using Pulumi.AzureNative.Resources;
using System.Collections.Generic;

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

        CosmosConnectionString = cosmos.ConnectionString;

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

}
