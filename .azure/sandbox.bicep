targetScope = 'subscription'

param name string
param branch string
param sqlServerAdLogin string
param sqlServerSaLogin string
param sqlServerSaPassword string {
  secure: true
}
param sqlServerLogin string
param sqlServerPassword string {
  secure: true
}
param sid string
param oid string
param primaryLocation string = deployment().location
param secondaryLocation string = deployment().location

var domain = 'elcontoso.com'
var hostname = '${name}.${domain}'

resource rg 'Microsoft.Resources/resourceGroups@2020-06-01' = {
  name: name
  location: deployment().location
  tags: {
    branch: branch
  }
}

module main 'main.bicep' = {
  scope: rg
  name: '${deployment().name}-main'
  params: {
    oid: oid
    primaryLocation: primaryLocation
    secondaryLocation: secondaryLocation
    sqlServerSaLogin: sqlServerSaLogin
    sqlServerSaPassword: sqlServerSaPassword
    sqlServerLogin: sqlServerLogin
    sqlServerPassword: sqlServerPassword
  }
}

module dns 'dns.bicep' = {
  name: '${deployment().name}-dns'
  dependsOn: [
    main
  ]
  scope: resourceGroup('elcontoso')
  params: {
    name: name
    zone: domain
    token: main.outputs.txtToken
  }
}

module binding 'binding.bicep' = {
  name: '${deployment().name}-binding'
  dependsOn: [
    dns
  ]
  scope: rg
  params: {
    siteName: name
    hostName: hostname
  }
}

module tls 'tls.bicep' = {
  name: '${deployment().name}-tls'
  dependsOn: [
    binding
  ]
  scope: rg
  params: {
    hostName: hostname
    siteName: main.outputs.webName
    farmId: main.outputs.webFarm
  }
}

output acrHost string = main.outputs.acrHost
output acrRepo string = main.outputs.acrRepo
output sqlHost string = main.outputs.sqlHost
output webHost string = 'https://${hostname}'