param siteName string
param vaultName string
param imageName string
param acrHost string
param acrUsernameKey string
param acrPasswordKey string
param aiInstrumentationKey string
param storageConnectionStringKey string
param sqlConnectionStringKey string

resource appSettings 'Microsoft.Web/sites/config@2020-09-01' = {
  name: '${siteName}/appsettings'
  properties: {
      AzureWebJobsStorage: '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=${storageConnectionStringKey})'
      FUNCTIONS_EXTENSION_VERSION: '~3'
      DOCKER_ENABLE_CI: 'true'
      DOCKER_REGISTRY_SERVER_URL: 'https://${acrHost}'
      DOCKER_REGISTRY_SERVER_USERNAME: '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=${acrUsernameKey})'
      DOCKER_REGISTRY_SERVER_PASSWORD: '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=${acrPasswordKey})'
      APPINSIGHTS_INSTRUMENTATIONKEY: '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=${aiInstrumentationKey})'
      WEBSITES_ENABLE_APP_SERVICE_STORAGE: 'false'
  }
}

resource web 'Microsoft.Web/sites/config@2020-09-01' = {
  name: '${siteName}/web'
  properties: {
    linuxFxVersion: 'DOCKER|${acrHost}/${imageName}'
  }
}

resource connectionStrings 'Microsoft.Web/sites/config@2020-09-01' = {
  name: '${siteName}/connectionstrings'
  properties: {
    AppleSystemStatus: {
      type: 'SQLAzure'
      value: '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=${sqlConnectionStringKey})'
    }
  }
}