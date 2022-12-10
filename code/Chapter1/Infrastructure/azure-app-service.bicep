@description('The name of the database')
param appServicePlanName string = 'asp-${uniqueString(resourceGroup().id)}'

@description('List of connection strings')
param connectionStrings object = {}

@description('The Azure region that the database service must be created in')
param location string = resourceGroup().location

@description('Describes plan\'s pricing tier and instance size.')
@allowed([ 'F1', 'D1', 'B1', 'B2', 'B3', 'S1', 'S2', 'S3' ])
param skuName string = 'F1'

@description('The hostname of the database service')
param webServiceName string = 'web-${uniqueString(resourceGroup().id)}'

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: skuName
  }
}

resource website 'Microsoft.Web/sites@2022-03-01' = {
  name: webServiceName
  location: location
  tags: {
    'hidden-related:${appServicePlan.id}': 'empty'
  }
  properties: {
    serverFarmId: appServicePlan.id
  }
}

resource website_connectionstrings 'Microsoft.Web/sites/config@2022-03-01' = {
  name: 'connectionstrings'
  parent: website
  properties: connectionStrings
}

output websiteUrl string = 'https://${website.properties.defaultHostName}'
