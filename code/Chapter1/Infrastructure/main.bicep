@description('The name of the web site (without the .azurewebsites.net)')
param webServiceName string = 'web-${uniqueString(resourceGroup().id)}'

@description('The name of the database service')
param databaseServiceName string = 'dbservice-${uniqueString(resourceGroup().id)}'

@description('The default location of all resources')
param location string = resourceGroup().location

module database './azure-sql.bicep' = {
  name: 'database'
  params: {
    administratorLogin: 'appadmin'
    administratorLoginPassword: 'MyPassword1234'
    databaseServiceName: databaseServiceName
    location: location
  }
}

module website './azure-app-service.bicep' = {
  name: webServiceName
  params: {
    webServiceName: webServiceName
    location: location
    connectionStrings: {
      DefaultConnection: { type: 'SQLAzure', value: database.outputs.connectionString }
    }
  }
}

output sqlDatabaseConnectionString string = database.outputs.connectionString
output mobileBackendUrl string = website.outputs.websiteUrl

