@description('The admin user of the SQL Server')
param administratorLogin string = 'appadmin'

@description('The password of the admin user of the SQL Server')
@secure()
param administratorLoginPassword string

@description('The name of the database')
param databaseName string = 'sampledb'

@description('The hostname of the database service')
param databaseServiceName string = 'dbservice-${uniqueString(resourceGroup().id)}'

@description('The Azure region that the database service must be created in')
param location string = resourceGroup().location

@description('The SKU (or pricing plan) to use for the database')
param skuName string = 'Free'

resource sqlService 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: databaseServiceName
  location: location
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  name: databaseName
  parent: sqlService
  location: location
  sku: {
    name: skuName
  }
}

#disable-next-line  outputs-should-not-contain-secrets // Connection String required output for module.
output connectionString string = 'Server=tcp:${sqlService.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabase.name};User ID=${administratorLogin};Password=${administratorLoginPassword};'
