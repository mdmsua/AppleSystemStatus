param globalPrefix string = resourceGroup().name
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
param secondaryLocation string = resourceGroup().location

var acr = '${containerRegistryName}${environment().suffixes.acrLoginServer}'
var keyPrefix = siteName == globalPrefix ? '' : '${siteName}-'

var acrUsernameKey = 'RegistryUsername'
var acrPasswordKey = 'RegistryPassword'
var aiInstrumentationKey = 'InstrumentationKey'
var storageConnectionStringKey = 'StorageConnectionString'
var sqlServerUsernameKey = 'ServerUsername'
var sqlServerPasswordKey = 'ServerPassword'
var sqlDatabaseUsernameKey = '${keyPrefix}DatabaseUsername'
var sqlDatabasePasswordKey = '${keyPrefix}DatabasePassword'
var sqlConnectionStringKey = '${keyPrefix}DatabaseConnectionString'

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
        name: sqlServerUsernameKey
        value: sqlServerSaLogin
      }
      {
        name: sqlServerPasswordKey
        value: sqlServerSaPassword
      }
      {
        name: sqlDatabaseUsernameKey
        value: sqlServerLogin
      }
      {
        name: sqlDatabasePasswordKey
        value: sqlServerPassword
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
output sqlHost string = sqlHost
output txtToken string = web.outputs.site.verification
output webFarm string = web.outputs.site.farm
output webName string = siteName