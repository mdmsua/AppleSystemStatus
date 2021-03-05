param globalPrefix string = ''
param storageAccountName string = globalPrefix
param sqlServerName string = globalPrefix
param sqlServerSaLogin string
param sqlServerSaPassword string {
  secure: true
}
param sqlServerLogin string
param sqlServerPassword string {
  secure: true
}
param oid string = ''
param sid string = ''
param adLogin string = ''
param containerRegistryName string = globalPrefix
param insightsName string = globalPrefix
param serverFarmName string = globalPrefix
param siteName string = globalPrefix
param keyVaultName string = globalPrefix
param workspaceId string  = ''

param primaryLocation string = resourceGroup().location
param secondaryLocation string = 'westeurope'

var acr = '${containerRegistryName}${environment().suffixes.acrLoginServer}'
var acrUsernameKey = '${siteName}-DockerRegistryUsername'
var acrPasswordKey = '${siteName}-DockerRegistryPassword'
var sqlConnectionStringKey = '${siteName}-DatabaseConnectionString'
var aiInstrumentationKey = '${siteName}-AppInsightsInstrumentationKey'
var storageConnectionStringKey = '${siteName}-StorageConnectionString'

var sqlHost = '${sqlServerName}${environment().suffixes.sqlServerHostname}'

module storage 'storage.bicep' = {
  name: '${deployment().name}-storage'
  params: {
    location: secondaryLocation
    name: storageAccountName
  }
}

module sql 'sql.bicep' = {
  name: '${deployment().name}-sql'
  params: {
    name: sqlServerName
    location: primaryLocation
    adLogin: adLogin
    adminLogin: sqlServerSaLogin
    adminPassword: sqlServerSaPassword
    sid: sid
  }
}

module registry 'registry.bicep' = {
  name: '${deployment().name}-registry'
  dependsOn: [
    web
  ]
  params: {
    name: containerRegistryName
    location: primaryLocation
    site: web.outputs.site
  }
}

module insights 'insights.bicep' = {
  name: '${deployment().name}-insights'
  params: {
    name: insightsName
    location: secondaryLocation
    workspaceId: workspaceId
  }
}

module web 'web.bicep' = {
  name: '${deployment().name}-web'
  params: {
    farmName: serverFarmName
    siteName: siteName
    location: secondaryLocation
  }
}

module vault 'vault.bicep' = {
  name: '${deployment().name}-vault'
  dependsOn: [
    web
    storage
    insights
    registry
  ]
  params: {
    name: keyVaultName
    location: primaryLocation
    sid: sid
    policies: [
      {
        oid: web.outputs.site.oid
        permissions: [
          'get'
        ]
      }
      {
        oid: oid
        permissions: [
          'get'
          'list'
        ]
      }
    ]
    secrets: [
      {
        name: sqlConnectionStringKey
        value: 'Server=tcp:${sqlHost},1433;Initial Catalog=AppleSystemStatus;Persist Security Info=False;User ID=${sqlServerLogin};Password=${sqlServerPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
      }
      {
        name: storageConnectionStringKey
        value: storage.outputs.connectionString
      }
      {
        name: aiInstrumentationKey
        value: insights.outputs.instrumentationKey
      }
      {
        name: acrUsernameKey
        value: registry.outputs.username
      }
      {
        name: acrPasswordKey
        value: registry.outputs.password
      }
    ]
  }
}

module config 'config.bicep' = {
  name: '${deployment().name}-config'
  dependsOn: [
    registry
    vault
  ]
  params: {
    siteName: siteName
    vaultName: keyVaultName
    imageName: web.outputs.site.image
    acrHost: acr
    acrUsernameKey: acrUsernameKey
    acrPasswordKey: acrPasswordKey
    aiInstrumentationKey: aiInstrumentationKey
    sqlConnectionStringKey: sqlConnectionStringKey
    storageConnectionStringKey: storageConnectionStringKey
  }
}

output acrHost string = acr
output acrRepo string = '${acr}/${siteName}'
output acrUser string = acrUsernameKey
output acrPass string = acrPasswordKey
output kvName string = keyVaultName
output sqlHost string = sqlHost