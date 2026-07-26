using Pulumi;
using Azure = Pulumi.Azure;
using AzureNative = Pulumi.AzureNative;

return await Deployment.RunAsync(() =>
{
    var config = new Config();
    var domainName = config.Require("domainName");
    var resourceGroupName = config.Require("resourceGroupName");
    var staticSiteName = config.Require("staticSiteName");
    var location = config.Get("location") ?? "eastus2";

    var tags = new InputMap<string>
    {
        ["application"] = "espada",
        ["environment"] = "production",
        ["managed-by"] = "pulumi",
        ["workload"] = "website",
    };

    var resourceGroup = new AzureNative.Resources.ResourceGroup("website-resource-group", new()
    {
        ResourceGroupName = resourceGroupName,
        Location = location,
        Tags = tags,
    });

    var staticSite = new AzureNative.Web.StaticSite("website", new()
    {
        Name = staticSiteName,
        ResourceGroupName = resourceGroup.Name,
        Location = location,
        PublicNetworkAccess = "Enabled",
        Sku = new AzureNative.Web.Inputs.SkuDescriptionArgs
        {
            Name = "Free",
            Tier = "Free",
        },
        Tags = tags,
    });

    var dnsZone = new AzureNative.Dns.Zone("website-dns-zone", new()
    {
        ZoneName = domainName,
        ResourceGroupName = resourceGroup.Name,
        Location = "Global",
        ZoneType = AzureNative.Dns.ZoneType.Public,
        Tags = tags,
    });

    _ = new AzureNative.Dns.RecordSet("website-apex-alias", new()
    {
        RecordType = "A",
        RelativeRecordSetName = "@",
        ResourceGroupName = resourceGroup.Name,
        TargetResource = new AzureNative.Dns.Inputs.SubResourceArgs
        {
            Id = staticSite.Id,
        },
        Ttl = 300,
        ZoneName = dnsZone.Name,
    });

    _ = new AzureNative.Dns.RecordSet("website-www-cname", new()
    {
        CnameRecord = new AzureNative.Dns.Inputs.CnameRecordArgs
        {
            Cname = staticSite.DefaultHostname,
        },
        RecordType = "CNAME",
        RelativeRecordSetName = "www",
        ResourceGroupName = resourceGroup.Name,
        Ttl = 300,
        ZoneName = dnsZone.Name,
    });

    _ = new AzureNative.Dns.RecordSet("website-mail-mx", new()
    {
        MxRecords =
        {
            new AzureNative.Dns.Inputs.MxRecordArgs
            {
                Exchange = "mx1.privateemail.com",
                Preference = 10,
            },
            new AzureNative.Dns.Inputs.MxRecordArgs
            {
                Exchange = "mx2.privateemail.com",
                Preference = 10,
            },
        },
        RecordType = "MX",
        RelativeRecordSetName = "@",
        ResourceGroupName = resourceGroup.Name,
        Ttl = 1800,
        ZoneName = dnsZone.Name,
    });

    _ = new AzureNative.Dns.RecordSet("website-mail-spf", new()
    {
        RecordType = "TXT",
        RelativeRecordSetName = "@",
        ResourceGroupName = resourceGroup.Name,
        Ttl = 1800,
        TxtRecords =
        {
            new AzureNative.Dns.Inputs.TxtRecordArgs
            {
                Value = { "v=spf1 include:spf.privateemail.com ~all" },
            },
        },
        ZoneName = dnsZone.Name,
    });

    foreach (var hostName in new[] { "mail", "autoconfig", "autodiscover" })
    {
        _ = new AzureNative.Dns.RecordSet($"website-{hostName}-cname", new()
        {
            CnameRecord = new AzureNative.Dns.Inputs.CnameRecordArgs
            {
                Cname = "privateemail.com",
            },
            RecordType = "CNAME",
            RelativeRecordSetName = hostName,
            ResourceGroupName = resourceGroup.Name,
            Ttl = 1800,
            ZoneName = dnsZone.Name,
        });
    }

    var apexDomain = new Azure.AppService.StaticWebAppCustomDomain("website-apex-domain", new()
    {
        DomainName = domainName,
        StaticWebAppId = staticSite.Id,
        ValidationType = "dns-txt-token",
    });

    var wwwDomain = new Azure.AppService.StaticWebAppCustomDomain("website-www-domain", new()
    {
        DomainName = $"www.{domainName}",
        StaticWebAppId = staticSite.Id,
        ValidationType = "dns-txt-token",
    });

    _ = new AzureNative.Dns.RecordSet("website-domain-validation", new()
    {
        RecordType = "TXT",
        RelativeRecordSetName = "_dnsauth.www",
        ResourceGroupName = resourceGroup.Name,
        Ttl = 300,
        TxtRecords =
        {
            new AzureNative.Dns.Inputs.TxtRecordArgs
            {
                Value = { apexDomain.ValidationToken },
            },
            new AzureNative.Dns.Inputs.TxtRecordArgs
            {
                Value = { wwwDomain.ValidationToken },
            },
        },
        ZoneName = dnsZone.Name,
    });

    return new Dictionary<string, object?>
    {
        ["apexDomainValidation"] = apexDomain.ValidationType,
        ["canonicalUrl"] = $"https://{domainName}",
        ["defaultHostname"] = staticSite.DefaultHostname,
        ["nameServers"] = dnsZone.NameServers,
        ["resourceGroupName"] = resourceGroup.Name,
        ["staticSiteName"] = staticSite.Name,
        ["wwwDomainValidation"] = wwwDomain.ValidationType,
        ["wwwUrl"] = $"https://www.{domainName}",
    };
});