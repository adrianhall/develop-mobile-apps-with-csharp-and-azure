# Deploy the mobile backend

Now that you've created, built, and tested your mobile backend, it's time to run it in the cloud.  While testing, you don't technically need to run the mobile backend in the cloud. However, the emulators and devices that you use to run the mobile application need to connect to the mobile backend via secure HTTP.  This means that the mobile backend needs a domain name and a real SSL certificate. The easiest way to accomplish this is "in the cloud".

The service we are going to use to host our code is [Azure App Service](https://learn.microsoft.com/azure/app-service/overview).  App Service is a PaaS (platform as a service) that allows you to run web services (including mobile backends) without having to worry about the underlying environment.  App Service automatically patches and maintains the OS and language frameworks for you, allowing you to spend your time writing code rather than maintaining services.  We'll also use [Azure SQL](https://learn.microsoft.com/azure/azure-sql/azure-sql-iaas-vs-paas-what-is-overview?view=azuresql) to store the data for our application.  Like App Service, Azure SQL is a PaaS (so you don't have to worry about maintaining the service) that runs runs the SQL Server database engine for you.

Deployment is split into two parts.  In the first half, I will show you the easiest way to create the two services that we need to run the mobile backend.  In the second half, we will publish the code onto the services that we've created in our Azure subscription.

!!! note
    When deploying to the cloud, the individual components of the infrastructure you deploy are called _resources_.

Before you begin, you must have an [Azure subscription](https://azure.microsoft.com/pricing/purchase-options/).  There are special pricing plans and offers for students, non-profits, and if you have access to a Visual Studio subscription.  If you are new to Azure, you can also get free services and credit to explore Azure.

!!! info "Pricing"
    In this project, we use the "free tier" for Azure App Service and Azure SQL, which provides a very low capacity capability.  Your subscription may not allow the use of these tiers.

## Configure the Azure CLI

Before we get started with deploying infrastructure, we need to set up the Azure CLI.  There are multiple ways of creating infrastructure resources in Azure.  You can, for instance, point and click your way around the [Azure portal](https://portal.azure.com).  You can also go through a wizard experience inside Visual Studio to create the resources.  However, I find that the easiest way to define the infrastructure and do repeatable deployments is on the command line.  To do this, you need to download, install, and configure the Azure CLI.

Start by [downloading and installing](https://learn.microsoft.com/cli/azure/install-azure-cli).  The Azure folks have provided step-by-step instructions on how to do this (linked).  Once you have finished the installation, you should be able to run `az version` on the command line.  This short test will ensure you have installed the tool correctly.

```powershell
PS> az version
{
  "azure-cli": "2.43.0",
  "azure-cli-core": "2.43.0",
  "azure-cli-telemetry": "1.0.8",
  "extensions": {}
}
```

Next, login to Azure using `az login`.  This will pop up a browser and ask you to log in to your subscription.  Use the same credentials that you use to log in to the Azure portal.

!!! tip
    The `az login` mechanism works for most cases.  If you obtained your subscription through your workplace, you may need to use [an alternate mechanism to sign in](https://learn.microsoft.com/cli/azure/authenticate-azure-cli) with the Azure CLI.

If you only have one subscription, nothing more is needed to configure the Azure CLI.  If you have multiple subscriptions, you must check to ensure the Azure CLI is using the right subscription:

* Check which subscription is being used with the `az account show` command.
* Get a list of subscriptions you have access to with the `az account list` command.
* Change the active subscription with the `az account set --subscription "<name or guid>"` command.

!!! tip
    Subscriptions are identified by a globally unique ID (or GUID), but also have a settable friendly name.  You can use either the name or the GUID when setting the subscription.

Finally, before you continue, make sure you have upgraded your Azure CLI to the latest version.  You can do this with two commands:

```powershell
PS> az upgrade
PS> az bicep upgrade
```

## Build a bicep template

Every single cloud service provider has a mechanism for creating infrastructure resources as a group.  In AWS, for example, this functionality is called CloudFormation.  In Azure, it's called Azure Resource Manager.  Both of these technologies use JSON templates to describe the functionality.  However, JSON templates are not the easiest to write.  As a result, Azure has created [bicep](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/overview?tabs=bicep) - a domain specific language for describing your Azure infrastructure.

When using bicep, you describe the resources you want to deploy in a declarative way.  For instance, our infrastructure consists of an Azure App Service and an Azure SQL database.  We can describe these in a `main.bicep` file:

```text linenums="1" title="main.bicep"
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
    connectionStrings: [
      { type: 'sql', name: 'DefaultConnection', value: database.outputs.connectionString }
    ]
  }
}

output sqlDatabaseConnectionString string = database.outputs.connectionString
output mobileBackendUrl string = website.outputs.websiteUrl
```

Every web service that is hosted on Azure App Service is hosted within the domain `azurewebsites.net`.  Since you are likely to be typing the web service name into other tools (like Postman), I've given you the option of entering your own name.  However, it must be globally unique, so I've also provided a mechanism for generating a unique name for the web service name. I've done the same thing for the database service name.

!!! tip
    There is no "Visual Studio friendly" place to put these files.  I tend to put them in a folder called `Infrastructure` with the project.  There is also no standard naming scheme, but Azure samples use the name `main.bicep` for the service definition.

The template uses two [modules](https://learn.microsoft.com/azure/azure-resource-manager/bicep/modules) (which we'll write in a moment) - one for Azure SQL and one for Azure App Service.  Modules are parameterized and reusable snippets of bicep templates that we can use to organize our infrastructure.

When we get to the end of the deployment, there are two output lines - one for the SQL database connection string and one for the mobile backend URL.  

### The Azure SQL module

Each module is simply another bicep file with a number of parameters.  Just like the main bicep file, you can give those parameters defaults.  The defaults can be overridden on deployment from the main bicep file, or you can deploy just the module with a parameters file.  Here is the module definition

```text linenums="1" title="azure-sql.bicep"
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
```

The `azure-sql.bicep` file contains two resources - one for the service and one for the database.  Note that we default the `skuName` parameter to **Free**.  This allows us to select the free tier for Azure SQL.  This tier is not available in all regions and you can only have one of them.  If you cannot use the free tier, just add a parameter to your `main.bicep` to set the `skuName: 'Basic'` to select the smallest non-free SKU.

Once the database is created, we create a connection string that uses the administrator login and password.  

!!! tip
    It's generally considered "bad form" to output a secret from a module.  Instead, you should write out the appropriate value for the resource and construct the connection string within another module.  I find this problematic for Azure SQL specifically since there is no way to get the administrator password later on.  As a result, I disable the warning about secrets for Azure SQL connection string outputs.

Bicep modules like this are designed to be reusable, and tend to be "cut-and-paste" compatible.

### The Azure App Service module

Similar to the mechanism that Azure SQL uses (where there is a split between the database service and the database itself), there is a split between the servers that run the web site and the web site itself.  The servers that run the web site are called an _App Service Plan_.  The `azure-app-service.bicep` file contains two resources:

```text linenums="1" title="azure-app-service.bicep"
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
```

This module will create an App Service Plan and then create a website on top of that App Service Plan.  It will also create an override for the connection string within our `appsettings.json` file so that the mobile backend will use the cloud database when it is published.

There is, of course, a lot more that we can do in these modules.  They are meant to serve as a starting point for your own investigations.  We'll be taking a closer look at these files later in the book.

## Deploy the infrastructure

It's now time to create some resources.  In Azure, resources are collected into _resource groups_.  You can put whatever you want into a resource group.  Most people put collections of resources that work together to implement a specific application together.

First, create a resource group:

```powershell
PS> az group create -g chapter1 -l southcentralus
```

You must specify a default location for a resource group.  This allows us to define the resources so that they are all colocated in the same region.  Next, run the deployment:

```powershell
PS> az deployment group create -g chapter1 -n azuredeploy --template-file ./main.bicep
```

Deployments can take a while; the time taken depends on the number and type of resources that you are deploying.  Once the deployment is complete, you will see a JSON output that contains information about the deployment. 

!!! tip
    Make sure you always "name" your deployments with the `-n <name>` parameter.  This will ensure you can review the response from the deployment later on.

You can review the deployment using the `az deployment group show` command.  To get the output you saw before, use the following:

```powershell
PS> az deployment group show -g chapter1 -n azuredeploy
```

This is a lot of information, however.  It's generally better to just get portions of the output in a friendlier format.  For instance, this command gets the outputs of the deployment in YAML:

```powershell
PS> az deployment group show -g chapter1 -n azuredeploy --query properties.outputs -o yaml
mobileBackendUrl:
  type: String
  value: https://web-mzlfzhipvfftk.azurewebsites.net
sqlDatabaseConnectionString:
  type: String
  value: Server=tcp:dbservice-mzlfzhipvfftk.database.windows.net,1433;Initial Catalog=sampledb;User
    ID=appadmin;Password=MyPassword1234;
```

We will need the `mobileBackendUrl` value later on, so make a note of that.  You can use the `sqlDatabaseConnectionString` to browse the data in the SQL database later on.
