param name string
param location string
param adminLogin string
param adminPassword string {
  secure: true
}
param adLogin string
param sid string

resource server 'Microsoft.Sql/servers@2019-06-01-preview' = {
  name: name
  location: location
  properties: {
    administratorLogin: adminLogin
    administratorLoginPassword: adminPassword
    minimalTlsVersion: '1.2'
    version: '12.0'
    publicNetworkAccess: 'Enabled'
  }
}

resource administrator 'Microsoft.Sql/servers/administrators@2019-06-01-preview' = {
  name: '${name}/ActiveDirectory'
  dependsOn: [
    server
  ]
  properties: {
    administratorType: 'ActiveDirectory'
    login: adLogin
    sid: sid
    tenantId: subscription().tenantId
  }
}

resource database 'Microsoft.Sql/servers/databases@2020-08-01-preview' = {
  name: '${name}/AppleSystemStatus'
  location: location
  dependsOn: [
    server
  ]
  tags: {
    environment: 'production'
  }
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    storageAccountType: 'GRS'
  }
}

resource canary 'Microsoft.Sql/servers/databases@2020-08-01-preview' = {
  name: '${name}/AppleSystemStatus'
  location: location
  dependsOn: [
    server
  ]
  tags: {
    environment: 'canary'
  }
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    storageAccountType: 'GRS'
  }
}


resource firewall 'Microsoft.Sql/servers/firewallRules@2015-05-01-preview' = {
  name: '${name}/AllowAllWindowsAzureIps'
  dependsOn: [
    server
  ]
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}