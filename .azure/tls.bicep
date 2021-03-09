param hostName string
param siteName string
param farmId string

resource certificate 'Microsoft.Web/certificates@2020-06-01' = {
  name: hostName
  location: resourceGroup().location
  properties: {
    canonicalName: hostName
    serverFarmId: farmId
    domainValidationMethod: 'http-token'
  }
}

resource binding 'Microsoft.Web/sites/hostNameBindings@2020-06-01' = {
  name: '${siteName}/${hostName}'
  properties: {
    sslState: 'SniEnabled'
    thumbprint: certificate.properties.thumbprint
  } 
}