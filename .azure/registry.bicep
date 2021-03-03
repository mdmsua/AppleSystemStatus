param name string
param location string
param sites array

resource registry 'Microsoft.ContainerRegistry/registries@2020-11-01-preview' = {
  name: name
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
    networkRuleBypassOptions: 'AzureServices'
    networkRuleSet: {
      defaultAction: 'Allow'
      ipRules: []
      virtualNetworkRules: []
    }
    dataEndpointEnabled: false
    publicNetworkAccess: 'Enabled'
    zoneRedundancy: 'Disabled'
    policies: {
      quarantinePolicy: {
        status: 'disabled'
      }
      retentionPolicy: {
        status: 'disabled'
      }
      trustPolicy: {
        status: 'disabled'
      }
    }
    encryption: {
      status: 'disabled'
    }
  }
}

resource webhooks 'Microsoft.ContainerRegistry/registries/webhooks@2020-11-01-preview' = [for site in sites: {
  location: location
  name: site.name
  properties: {
    actions: [
      'push'
    ]
    scope: '${site.image}'
    serviceUri: site.webhook
    status: 'enabled'
  }
}]

output username string = listCredentials(registry.id, registry.apiVersion).username
output password string = listCredentials(registry.id, registry.apiVersion).passwords[0].value