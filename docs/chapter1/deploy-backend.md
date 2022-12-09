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

Next, login to Azure using `az login`.  This will pop up a browser and ask you to log in to your subscription.  Use the same credentials that you use to log in to the Azure portal.

!!! tip
    The `az login` mechanism works for most cases.  If you obtained your subscription through your workplace, you may need to use [an alternate mechanism to sign in](https://learn.microsoft.com/cli/azure/authenticate-azure-cli) with the Azure CLI.

If you only have one subscription, nothing more is needed to configure the Azure CLI.  If you have multiple subscriptions, you must check to ensure the Azure CLI is using the right subscription:

* Check which subscription is being used with the `az account show` command.
* Get a list of subscriptions you have access to with the `az account list` command.
* Change the active subscription with the `az account set --subscription "<name or guid>"` command.

!!! tip
    Subscriptions are identified by a globally unique ID (or GUID), but also have a settable friendly name.  You can use either the name or the GUID when setting the subscription.
