using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Sql;
using Pulumi.Random;
using System.Collections.Generic;

class MyStack : Stack
{
    public MyStack()
    {
        var vnet = new StackReference($"jacobladams/9a-stack-reference-vnet/{Pulumi.Deployment.Instance.StackName}");
        var subnetId = vnet.RequireOutput("SubnetId").Apply(s => s.ToString());

        var commonTags = new Dictionary<string, string>
        {
            {"workload", Pulumi.Deployment.Instance.ProjectName},
            {"environment", Pulumi.Deployment.Instance.StackName},
            {"cost center", "IT"},
            {"owner", "Jake Adams"},
            {"demo", "true"}
        };

        var resourceGroup = new ResourceGroup($"rg-products-", new ResourceGroupArgs
        {
            Tags = commonTags
        });

        var password = new RandomPassword("password", new RandomPasswordArgs
        {
            Length = 32,
        });

        var sqlServer = new Server("sql-products-", new ServerArgs
        {
            AdministratorLogin = "dummylogin",
            AdministratorLoginPassword = password.Result,
            ResourceGroupName = resourceGroup.Name,
            Tags = commonTags,
        });

        var virtualNetworkRule = new VirtualNetworkRule("virtualNetworkRule", new VirtualNetworkRuleArgs
        {
            ResourceGroupName = resourceGroup.Name,
            ServerName = sqlServer.Name,
            VirtualNetworkRuleName = "vnet-firewall-rule",
            VirtualNetworkSubnetId = subnetId,
        });
    }

}
