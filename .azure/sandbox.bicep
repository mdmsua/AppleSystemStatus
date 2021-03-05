targetScope = 'subscription'

param name string
param sqlServerSaLogin string
param sqlServerSaPassword string {
  secure: true
}
param sqlServerLogin string
param sqlServerPassword string {
  secure: true
}
param oid string

var id = uniqueString(subscription().id, name)

resource rg 'Microsoft.Resources/resourceGroups@2020-06-01' = {
  name: id
  location: deployment().location
  tags: {
    branch: name
  }
}

module main 'main.bicep' = {
  scope: rg
  name: '${deployment().name}-main'
  params: {
    oid: oid
    globalPrefix: id
    sqlServerSaLogin: sqlServerSaLogin
    sqlServerSaPassword: sqlServerSaPassword
    sqlServerLogin: sqlServerLogin
    sqlServerPassword: sqlServerPassword
  }
}

output acrHost string = main.outputs.acrHost
output acrRepo string = main.outputs.acrRepo
output acrUser string = main.outputs.acrUser
output acrPass string = main.outputs.acrPass
output kvName string = main.outputs.kvName
output sqlHost string = main.outputs.sqlHost