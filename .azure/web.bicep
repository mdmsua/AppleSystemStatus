param farmName string
param siteName string
param location string
param vaultName string
param registryName string

var slot = 'canary'
var slotName = '${siteName}/${slot}'

resource farm 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: farmName
  location: location
  kind: 'linux'
  sku: {
    name: 'B1'
    tier: 'Basic'
    size: 'B1'
    family: 'B'
    capacity: 1
  }
  properties: {
    reserved: true
  }
}

resource site 'Microsoft.Web/sites@2020-06-01' = {
  name: siteName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  tags: {
    environment: 'production'
  }
  kind: 'functionapp,linux,container'
  properties: {
    enabled: true
    serverFarmId: farm.id
    httpsOnly: true
    reserved: true
    siteConfig: {
      alwaysOn: true
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=${siteName}-AzureWebJobsStorage)'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'DOCKER_ENABLE_CI'
          value: 'true'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://${registryName}${environment().suffixes.acrLoginServer}'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: registryName
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=${siteName}-DockerRegistryServerPassword)'
        }
        {
          name: 'DOCKER_CUSTOM_IMAGE_NAME'
          value: 'DOCKER|${registryName}${environment().suffixes.acrLoginServer}/${siteName}:latest'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=${siteName}-ApplicationInsightsInstrumentationKey)'
        }
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
      ]
      connectionStrings: [
        {
          name: 'AppleSystemStatus'
          type: 'SQLAzure'
          connectionString: '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=${siteName}-AppleSystemStatus)'
        }
      ]
      ftpsState: 'Disabled'
      healthCheckPath: '/healthcheck'
      http20Enabled: true
      linuxFxVersion: ''
      minTlsVersion: '1.2'
      numberOfWorkers: 1
      windowsFxVersion: ''
      use32BitWorkerProcess: false
      webSocketsEnabled: false
    }
  }
}

resource canary 'Microsoft.Web/sites/slots@2020-06-01' = {
  name: slotName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  tags: {
    environment: 'canary'
  }
  kind: 'functionapp,linux,container'
  properties: {
    enabled: true
    serverFarmId: farm.id
    httpsOnly: true
    reserved: true
    siteConfig: {
      alwaysOn: true
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=${siteName}-AzureWebJobsStorage)'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'DOCKER_ENABLE_CI'
          value: 'true'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://${registryName}${environment().suffixes.acrLoginServer}'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: registryName
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=${siteName}-DockerRegistryServerPassword)'
        }
        {
          name: 'DOCKER_CUSTOM_IMAGE_NAME'
          value: 'DOCKER|${registryName}${environment().suffixes.acrLoginServer}/${siteName}:canary'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=${siteName}-ApplicationInsightsInstrumentationKey)'
        }
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
      ]
      connectionStrings: [
        {
          name: 'AppleSystemStatus'
          type: 'SQLAzure'
          connectionString: '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=${siteName}-CanarySystemStatus)'
        }
      ]
      ftpsState: 'Disabled'
      healthCheckPath: '/healthcheck'
      http20Enabled: true
      linuxFxVersion: ''
      minTlsVersion: '1.2'
      numberOfWorkers: 1
      windowsFxVersion: ''
      use32BitWorkerProcess: false
      webSocketsEnabled: false
    }
  }
}

var sitePublishingUsername = list(resourceId('Microsoft.Web/sites/config', siteName, 'publishingcredentials'), site.apiVersion).properties.publishingUserName
var sitePublishingPassword = list(resourceId('Microsoft.Web/sites/config', siteName, 'publishingcredentials'), site.apiVersion).properties.publishingUserName
var slotPublishingUsername = list(resourceId('Microsoft.Web/sites/config', slotName, 'publishingcredentials'), site.apiVersion).properties.publishingUserName
var slotPublishingPassword = list(resourceId('Microsoft.Web/sites/config', slotName, 'publishingcredentials'), site.apiVersion).properties.publishingPassword

output sites array = [
  {
    name: siteName
    webhook: 'https://${sitePublishingUsername}:${sitePublishingPassword}@${siteName}.scm.azurewebsites.net/docker/hook'
    oid: site.identity.principalId
    image: '${siteName}:latest'
  }
  {
    name: slot
    webhook: 'https://${slotPublishingUsername}:${slotPublishingPassword}@${slot}.scm.azurewebsites.net/docker/hook'
    oid: canary.identity.principalId
    image: '${siteName}:canary'
  }
]