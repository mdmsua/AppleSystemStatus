param siteName string
param hostName string

resource binding 'Microsoft.Web/sites/hostNameBindings@2020-06-01' = {
  name: '${siteName}/${hostName}'
  properties: {
    customHostNameDnsRecordType: 'CName'
  } 
}