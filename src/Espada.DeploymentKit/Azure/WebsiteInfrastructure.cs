using Espada.DeploymentKit.Constants;
using Pulumi;
using Pulumi.Azure.AppService;
using Pulumi.AzureNative.Dns;
using Pulumi.AzureNative.Dns.Inputs;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Web.Inputs;
using StaticSite = Pulumi.AzureNative.Web.StaticSite;
using StaticSiteArgs = Pulumi.AzureNative.Web.StaticSiteArgs;

namespace Espada.DeploymentKit.Azure
{
    internal static class WebsiteInfrastructure
    {
        public static IDictionary<string, object?> Create(string location)
        {
            string domainName = AzureDeploymentConstants.WebsiteDomainName;
            string resourceGroupName = AzureDeploymentConstants.WebsiteResourceGroupName;
            string staticSiteName = AzureDeploymentConstants.WebsiteStaticSiteName;

            InputMap<string> tags = new()
            {
                ["application"] = "espada",
                ["environment"] = "production",
                ["managed-by"] = "pulumi",
                ["workload"] = "website"
            };

            ResourceGroup resourceGroup = new("website-resource-group",
                new ResourceGroupArgs { ResourceGroupName = resourceGroupName, Location = location, Tags = tags });

            StaticSite staticSite = new("website",
                new StaticSiteArgs
                {
                    Name = staticSiteName,
                    ResourceGroupName = resourceGroup.Name,
                    Location = location,
                    RepositoryUrl = "https://github.com/anton-kharchenko/Espada",
                    Branch = "master",
                    Provider = "GitHub",
                    PublicNetworkAccess = "Enabled",
                    Sku = new SkuDescriptionArgs { Name = "Free", Tier = "Free" },
                    Tags = tags
                });

            Zone dnsZone = new("website-dns-zone",
                new ZoneArgs
                {
                    ZoneName = domainName,
                    ResourceGroupName = resourceGroup.Name,
                    Location = "Global",
                    ZoneType = ZoneType.Public,
                    Tags = tags
                });

            _ = new RecordSet("website-apex-alias",
                new RecordSetArgs
                {
                    RecordType = "A",
                    RelativeRecordSetName = "@",
                    ResourceGroupName = resourceGroup.Name,
                    TargetResource = new SubResourceArgs { Id = staticSite.Id },
                    Ttl = 300,
                    ZoneName = dnsZone.Name
                });

            RecordSet wwwCname = new("website-www-cname",
                new RecordSetArgs
                {
                    CnameRecord = new CnameRecordArgs { Cname = staticSite.DefaultHostname },
                    RecordType = "CNAME",
                    RelativeRecordSetName = "www",
                    ResourceGroupName = resourceGroup.Name,
                    Ttl = 300,
                    ZoneName = dnsZone.Name
                });

            _ = new RecordSet("website-mail-mx",
                new RecordSetArgs
                {
                    MxRecords =
                    {
                        new MxRecordArgs { Exchange = "mx1.privateemail.com", Preference = 10 },
                        new MxRecordArgs { Exchange = "mx2.privateemail.com", Preference = 10 }
                    },
                    RecordType = "MX",
                    RelativeRecordSetName = "@",
                    ResourceGroupName = resourceGroup.Name,
                    Ttl = 1800,
                    ZoneName = dnsZone.Name
                });

            _ = new RecordSet("website-mail-spf",
                new RecordSetArgs
                {
                    RecordType = "TXT",
                    RelativeRecordSetName = "@",
                    ResourceGroupName = resourceGroup.Name,
                    Ttl = 1800,
                    TxtRecords = { new TxtRecordArgs { Value = { "v=spf1 include:spf.privateemail.com ~all" } } },
                    ZoneName = dnsZone.Name
                });

            foreach (string hostName in new[] { "mail", "autoconfig", "autodiscover" })
            {
                _ = new RecordSet($"website-{hostName}-cname",
                    new RecordSetArgs
                    {
                        CnameRecord = new CnameRecordArgs { Cname = "privateemail.com" },
                        RecordType = "CNAME",
                        RelativeRecordSetName = hostName,
                        ResourceGroupName = resourceGroup.Name,
                        Ttl = 1800,
                        ZoneName = dnsZone.Name
                    });
            }

            StaticWebAppCustomDomain apexDomain = new("website-apex-domain",
                new StaticWebAppCustomDomainArgs
                {
                    DomainName = domainName,
                    StaticWebAppId = staticSite.Id,
                    ValidationType = "dns-txt-token"
                });

            StaticWebAppCustomDomain wwwDomain = new("website-www-domain",
                new StaticWebAppCustomDomainArgs
                {
                    DomainName = $"www.{domainName}",
                    StaticWebAppId = staticSite.Id,
                    ValidationType = "cname-delegation"
                }, new CustomResourceOptions { DependsOn = { wwwCname }, IgnoreChanges = { "validationType" } });

            _ = new RecordSet("website-domain-validation",
                new RecordSetArgs
                {
                    RecordType = "TXT",
                    RelativeRecordSetName = "_dnsauth.www",
                    ResourceGroupName = resourceGroup.Name,
                    Ttl = 300,
                    TxtRecords = { new TxtRecordArgs { Value = { apexDomain.ValidationToken } } },
                    ZoneName = dnsZone.Name
                });

            return new Dictionary<string, object?>
            {
                ["apexDomainValidation"] = apexDomain.ValidationType,
                ["canonicalUrl"] = $"https://www.{domainName}",
                ["defaultHostname"] = staticSite.DefaultHostname,
                ["nameServers"] = dnsZone.NameServers,
                ["resourceGroupName"] = resourceGroup.Name,
                ["websiteResourceGroupName"] = resourceGroup.Name,
                ["staticSiteName"] = staticSite.Name,
                ["wwwDomainValidation"] = wwwDomain.ValidationType,
                ["wwwUrl"] = $"https://www.{domainName}"
            };
        }
    }
}