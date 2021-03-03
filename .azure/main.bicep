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
param objectId string
param containerRegistryName string = globalPrefix
param insightsName string = globalPrefix
param serverFarmName string = globalPrefix
param siteName string
param keyVaultName string = globalPrefix
param workspaceId string {
  default: ''
}

var defaultLocation = 'germanywestcentral'
var serviceLocation = 'westeurope'

module storage './storage.bicep' = {
  name: '${deployment().name}-storage'
  params: {
    location: serviceLocation
    name: storageAccountName
  }
}

module sql './sql.bicep' = {
  name: '${deployment().name}-sql'
  params: {
    name: sqlServerName
    location: defaultLocation
    adLogin: 'dmytro@mdmsft.net'
    adminLogin: sqlServerSaLogin
    adminPassword: sqlServerSaPassword
    sid: objectId
  }
}

module registry './registry.bicep' = {
  name: '${deployment().name}-registry'
  params: {
    name: containerRegistryName
    location: defaultLocation
    siteName: siteName
    webhookUri: 'https://${web.outputs.publishingUsername}:${web.outputs.publishingPassword}@${siteName}.scm.azurewebsites.net/docker/hook'
  }
}

module insights './insights.bicep' = {
  name: '${deployment().name}-insights'
  params: {
    name: insightsName
    location: serviceLocation
    workspaceId: workspaceId
  }
}

module web './web.bicep' = {
  name: '${deployment().name}-web'
  params: {
    farmName: serverFarmName
    siteName: siteName
    location: serviceLocation
    vaultName: keyVaultName
    registryName: containerRegistryName
  }
}

module vault './vault.bicep' = {
  name: '${deployment().name}-vault'
  params: {
    name: keyVaultName
    location: defaultLocation
    sid: objectId
    instrumentationKey: insights.outputs.instrumentationKey
    registryPassword: registry.outputs.password
    siteId: web.outputs.objectId
    siteName: siteName
    sqlConnectionString: 'Server=tcp:${sqlServerName}${environment().suffixes.sqlServerHostname},1433;Initial Catalog=AppleSystemStatus;Persist Security Info=False;User ID=${sqlServerLogin};Password=${sqlServerPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
    storageConnectionString: storage.outputs.connectionString
  }
}