# Deploy the mobile backend

Now that you've created, built, and tested your mobile backend, it's time to run it in the cloud.  While testing, you don't technically need to run the mobile backend in the cloud. However, the emulators and devices that you use to run the mobile application need to connect to the mobile backend via secure HTTP.  This means that the mobile backend needs a domain name and a real SSL certificate. The easiest way to accomplish this is "in the cloud".

The service we are going to use to host our code is [Azure App Service](https://learn.microsoft.com/azure/app-service/overview).  App Service is a PaaS (platform as a service) that allows you to run web services (including mobile backends) without having to worry about the underlying environment.  App Service automatically patches and maintains the OS and language frameworks for you, allowing you to spend your time writing code rather than maintaining services.  We'll also use [Azure SQL](https://learn.microsoft.com/azure/azure-sql/azure-sql-iaas-vs-paas-what-is-overview?view=azuresql) to store the data for our application.  Like App Service, Azure SQL is a PaaS (so you don't have to worry about maintaining the service) that runs runs the SQL Server database engine for you.

Deployment is split into two parts.  In the first half, I will show you the easiest way to create the two services that we need to run the mobile backend.  In the second half, we will publish the code onto the services that we've created in our Azure subscription.

!!! note
    When deploying to the cloud, the individual components of the infrastructure you deploy are called _resources_.

Before you begin, you must have an [Azure subscription](https://azure.microsoft.com/pricing/purchase-options/).  There are special pricing plans and offers for students, non-profits, and if you have access to a Visual Studio subscription.  If you are new to Azure, you can also get free services and credit to explore Azure.

!!! info "Pricing"
    In this project, we use the "free tier" for Azure App Service, and the most minimal tier for Azure SQL.  You will incur costs for the Azure SQL instance.

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

```text
@description('The name of the web site (without the .azurewebsites.net)')
param webServiceName string = 'web-${uniqueString(resourceGroup().id)}'

@description('The name of the database service')
param databaseServiceName string = 'dbservice-${uniqueString(resourceGroup().id)}'

module database './modules/azure-sql.bicep' = {
    name: 'database'
    params: {
        databaseServiceName: databaseServiceName
    }
}

module website './modules/azure-app-service.bicep' = {
    name: webServiceName
    params: {
        webServiceName: webServiceName
        connectionStrings: [
            { type: 'sql', name: 'DefaultConnection', value: database.outputs.connectionString }
        ]
    }
}

output mobileBackendUrl string = website.url
```

Every web service that is hosted on Azure App Service is hosted within the domain `azurewebsites.net`.  Since you are likely to be typing the web service name into other tools (like Postman), I've given you the option of entering your own name.  However, it must be globally unique, so I've also provided a mechanism for generating a unique name for the web service name. I've done the same thing for the database service name.

The template uses two [modules](https://learn.microsoft.com/azure/azure-resource-manager/bicep/modules) (which we'll write in a moment) - one for Azure SQL and one for Azure App Service.  Modules are parameterized and reusable snippets of bicep templates that we can use to organize our infrastructure.

### The Azure SQL module

### The Azure App Service module

