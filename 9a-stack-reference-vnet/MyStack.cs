using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Network;
using System.Collections.Generic;

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

        var resourceGroup = new ResourceGroup($"rg-core-", new ResourceGroupArgs
        {
            Tags = commonTags
        });

        var virtualNetwork = new VirtualNetwork("vnet-core-", new VirtualNetworkArgs
        {
            AddressSpace = new Pulumi.AzureNative.Network.Inputs.AddressSpaceArgs
            {
                AddressPrefixes =
                {
                    "10.0.0.0/24",
                },
            },
            ResourceGroupName = resourceGroup.Name,
            Tags = commonTags
        });

        var networkSecurityGroup = new NetworkSecurityGroup("nsg-databases", new NetworkSecurityGroupArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Tags = commonTags
        });

        var subnet = new Subnet("snet-databases", new SubnetArgs
        {
            AddressPrefix = "10.0.0.0/25",
            ResourceGroupName = resourceGroup.Name,
            VirtualNetworkName = virtualNetwork.Name,
            NetworkSecurityGroup = new Pulumi.AzureNative.Network.Inputs.NetworkSecurityGroupArgs
            {
                Id = networkSecurityGroup.Id
            },
            ServiceEndpoints = new List<Pulumi.AzureNative.Network.Inputs.ServiceEndpointPropertiesFormatArgs>
            {
                new Pulumi.AzureNative.Network.Inputs.ServiceEndpointPropertiesFormatArgs
                {
                    Service = "Microsoft.Sql"
                }
            }
        });

        VirtualNetworkName = virtualNetwork.Name;
        SubnetId = subnet.Id;
    }

    [Output]
    public Output<string> VirtualNetworkName { get; set; }

    [Output]
    public Output<string> SubnetId { get; set; }
}
