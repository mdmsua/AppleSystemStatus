param name string
param location string
param sid string
param secrets array
param policies array

resource vault 'Microsoft.KeyVault/vaults@2019-09-01' = {
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

resource accessPolicies 'Microsoft.KeyVault/vaults/accessPolicies@2019-09-01' = {
  name: any('${vault.name}/add')
  properties: {
    accessPolicies: [for policy in policies: {
      tenantId: subscription().tenantId
      objectId: policy.oid
      permissions: {
        secrets: policy.permissions
      }
    }]
  }
}

resource secret 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = [for secret in secrets: {
  name: '${name}/${secret.name}'
  properties: {
    value: secret.value
  }
}]