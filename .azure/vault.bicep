param name string
param location string
param siteId string
param sid string
param siteName string
param sqlConnectionString string {
  secure: true
}
param storageConnectionString string {
  secure: true
}
param instrumentationKey string {
  secure: true
}
param registryPassword string {
  secure: true
}

resource vault 'Microsoft.KeyVault/vaults@2020-04-01-preview' = {
  name: name
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: siteId
        permissions: {
          secrets: [
            'get'
          ]
        }
      }
      {
        tenantId: subscription().tenantId
        objectId: sid
        permissions: {
          certificates: [
            'backup'
            'create'
            'delete'
            'deleteissuers'
            'get'
            'getissuers'
            'import'
            'list'
            'listissuers'
            'managecontacts'
            'manageissuers'
            'purge'
            'recover'
            'restore'
            'setissuers'
            'update'
          ]
          keys: [
            'backup'
            'create'
            'decrypt'
            'delete'
            'encrypt'
            'get'
            'import'
            'list'
            'purge'
            'recover'
            'restore'
            'sign'
            'unwrapKey'
            'update'
            'verify'
            'wrapKey'
          ]
          secrets: [
            'backup'
            'delete'
            'get'
            'list'
            'purge'
            'recover'
            'restore'
            'set'
          ]
        }
      }
    ]
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    enablePurgeProtection: true
    enableSoftDelete: true
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
      ipRules: []
      virtualNetworkRules: []
    }
    softDeleteRetentionInDays: 7
  }
}

resource sqlSecret 'Microsoft.KeyVault/vaults/secrets@2020-04-01-preview' = {
  name: '${name}/${siteName}-AppleSystemStatus'
  properties: {
    value: sqlConnectionString
  }
}

resource storageSecret 'Microsoft.KeyVault/vaults/secrets@2020-04-01-preview' = {
  name: '${name}/${siteName}-AzureWebJobsStorage'
  properties: {
    value: storageConnectionString
  }
}

resource instrumentationSecret 'Microsoft.KeyVault/vaults/secrets@2020-04-01-preview' = {
  name: '${name}/${siteName}-ApplicationInsightsInstrumentationKey'
  properties: {
    value: instrumentationKey
  }
}

resource registrySecret 'Microsoft.KeyVault/vaults/secrets@2020-04-01-preview' = {
  name: '${name}/${siteName}-DockerRegistryServerPassword'
  properties: {
    value: registryPassword
  }
}