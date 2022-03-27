using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.DocumentDB;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;

class MyStack : Stack
{
    public MyStack()
    {
        var commonTags = new Dictionary<string, string>
        {
            {"workload", Pulumi.Deployment.Instance.ProjectName},
            {"environment", Pulumi.Deployment.Instance.StackName},
            {"cost center", "IT"},
            {"owner", "Jake Adams"},
            {"demo", "true"}
        };

        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup($"rg-{Pulumi.Deployment.Instance.ProjectName}", new ResourceGroupArgs
        {
            Tags = commonTags
        });

        var cosmos = new Cosmos(resourceGroup.Name, commonTags);

        this.ConnectionString = CreateCosmosConnectionString(resourceGroup.Name, cosmos.DatabaseAccountName);


        var function = new Function(resourceGroup.Name, this.ConnectionString, commonTags);

        var staticWebsite = new StaticWebsite(resourceGroup.Name, function.Url, commonTags);

        this.Endpoint = function.Url;

        this.WebsiteUrl = staticWebsite.WebsiteUrl;
    }
 
    [Output]
    public Output<string> WebsiteUrl { get; set; }

    [Output]
    public Output<string> ConnectionString { get; set; }

    [Output]
    public Output<string> Endpoint { get; set; }
}
