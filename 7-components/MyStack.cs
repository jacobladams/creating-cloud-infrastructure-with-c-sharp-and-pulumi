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
            {"environment", "dev"},
            {"cost center", "IT"},
            {"owner", "Jake Adams"},
            {"demo", "true"}
        };

        var component = new ThreeTierComponent(Pulumi.Deployment.Instance.ProjectName, commonTags, new ComponentResourceOptions{});

        this.CosmosConnectionString = component.CosmosConnectionString;

        this.Endpoint = component.Endpoint;

        this.WebsiteUrl = component.WebsiteUrl;
    }
 
    [Output]
    public Output<string> WebsiteUrl { get; set; }

    [Output]
    public Output<string> CosmosConnectionString { get; set; }

    [Output]
    public Output<string> Endpoint { get; set; }
}
