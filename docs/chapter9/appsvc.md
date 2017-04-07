# Safe Deployments

At some point, you will have a production app that needs updating.  Hopefully, you will have many thousands of users by that point and they are clamouring for an update.  If this is you, congratulations.  You have a successful mobile app.  Now, how do you update your app without your users noticing?

A big part of the success of any update process is to ensure your Entity Framework code is appropriately updating the database and that the database can handle both the old schema and new schema.  That means being able to cope with optional fields, and using default values.  It also means ensuring that you are using automatic code-first migrations, which I covered way back in [Chapter 3](../chapter3/server.md). 

In this section, we will cover two topics:

*  How do I upgrade my mobile backend without my users noticing?
*  How do I scale my site to elastically manage load?

Fortunately, Azure App Service has built-in features for both these questions.

## Using Slots

Let's start with upgrading the mobile backend.  The feature that we are going to use is **Deployment Slots**.  A slot is a second (or third or fourth) copy of your mobile backend that shares all (or most) of the settings of the original, but runs different code.  Slots is available in Standard and above App Service Plans.  A Standard App Service has 5 slots and a Premium App Service has 20 slots. If your site is not running on a Standard service, use **Scale Up** to change the App Service Plan to a Standard or better plan.

!!! tip "Check your App Service Plan"
    Each App Service Plan has a number of slots.  This is the number of web sites or mobile backends you can run on the same App Service Plan.  Remember an App Service Plan is a set of virtual machines that run the same set of web sites.   The slot is also a web site.

Let's say you want to deploy your site with zero downtime.  Establish a slot called "staging":

*  Open your App Service in the [Azure portal].
*  Click **Deployment slots**.
*  Click **Add Slot**.
*  Enter `staging` in the **Name** field.
*  Click **OK**.

This is a one-time activity.  If the main site is called `mywebsite.azurewebsites.net`, then the new slot will be called `mywebsite-staging.azurewebsites.net`.  We can deploy the server to the new slot in almost exactly the same way as before.  Instead of selecting your website, expand the website, select **Deployment Slots** then the name of the slot that was created:

![Select the slot][img]

!!! tip "Update your client to use -staging"
    You can produce a test version of your mobile client that uses the staging slot as the URL.  This allows you to do ad-hoc testing prior to doing the update to production.

Once the mobile backend is deployed to the staging slot, you can do any testing required before deploying to production.  The next step is to turn staging into production:

*  Open your App Service in the [Azure portal].
*  Click **Deployment slots**.
*  Click **Swap**.
*  Select the Source and Destination slots to swap.
*  Click **OK**.

At this point, the internal routing will swap the two services.  Existing requests that are already being handled by the old server will be completed on the old server, but new requests will be handled by the new server.  Once the swap is complete, there will be no more requests being dealt with on the old server.  

If your test client (configured to use the -staging URL) is run, it will be using the old server.  The old server is in the -staging slot now.

### Best Practices: 3 Slots

I recommend using three slots:

*  Production
*  Staging
*  LastKnownGood

When you swap the slot, do two swaps:

*  First, deploy your new server to _Staging_.
*  Then, swap _Production_ with _Staging_.  Your new server is now in production.
*  Finally, swap _Staging_ with _LastKnownGood_.  This saves the (working) production server.

If your new server runs into problems and you have to switch back to the old server, it is stored away in the _LastKnownGood_ slot.  You can restore the old (working) server by swapping _LastKnownGood_ with _Production_.

### Best Practices: Continuous Deployment

I also recommend linking a Continuous Deployment stream to the staging slot, rather than deploying via Visual Studio.  Continuous Deployment automatically deploys code from a source code repository like GitHub or Visual Studio Team Services.  To link staging to a GitHub repository:

*  Open your App Service in the [Azure portal].
*  Click **Deployment slots**.
*  Click the staging slot name.
*  Click **Deployment options**.
*  Click **Choose Source**, then select GitHub.
*  If necessary, click **Authorization** to log in to GitHub.
*  If necessary, click **Choose your organization** to select a GitHub organization.
*  Click **Choose project** and select your GitHub repository.
*  Click **Choose branch** and select the branch you wish to deploy.
*  Click **OK**.

The service will download the latest source code from the specified branch, build the server and deploy it.  You can check the status of the deployment through **Deployment options** on the staging slot as well.

As a best practice, create a new branch (I call my branch "azure") and deploy that branch.  When you wish to update the deployed server, merge from the master branch to the azure branch and then push to the remote repository.  This will then be picked up by your App Service and the new service will be deployed.

By combining continuous deployment with slots, you can have control over the deployment to staging and control the production deployment easily.  This is a powerful "DevOps" combination of features.

## Scaling your Site

I hope that your mobile app is wildly successful.  If it is, one copy of the server will not be enough to handle the connections.  You will need to scale both the mobile backend and the database.  How will you know when to scale?

*  For the App Service, keep an eye on the **Metrics per Instance (App Service plan)**.  If your production service is getting close to 80% CPU or close to memory limits, it's probably time to scale the site.
*  For the database, check the **Query Performance Insight**.  Your database is limited to a certain number of DTUs (database transaction units - a blend of CPU and I/O operations).

In a production app, you want to scale your mobile backend according to demand.  This means that the service spins up additional copies of the App Service as needed.  To set up automatic scaling for an App Service:

*  Open your App Service in the [Azure portal].
*  Click **Scale out (App Service plan).
*  Select **CPU Percentage** in the **Scale by** drop-down.
*  Select a minimum and maximum number of instances (for example, 1-10 for a standard App Service Plan).
*  Select a target range for CPU (for example, 50-75%)
*  Optionally, fill in the **Notifications** section.
*  Click **Save**.

The App Service Plan will try and keep an appropriate number of instances such that the CPU is kept in the specified range.  If the average CPU drops below the threshold, the App Service Plan will take one of the instances out of server.  No new connections will be sent to the instance, the existing requests will be allowed to complete.  Once all connections are complete, the instance will be turned off.   You only pay for the instances that are actually running.

You can choose to send an email when a scale event happens.  You can also configure a Webhook so that your operations group is notified more broadly.  For example, you could use this facility to run an Azure Function that sends a notification to a Slack channel when an instance is added, or when more the last instance is added (indicating extreme load where your users are going to be impacted).

## Best Practices for Production Apps

We've mentioned a few best practices here, so here is a wrap-up:

*  Run production apps on Standard or Premium App Services.
*  Set up automatic scaling based on CPU percentage.
*  Set up alerting so that you are informed when the last instance is added.
*  Use three slots (production, staging and lastknowngood) to ensure no downtime.
*  Use continuous deployment to the staging slot rather than deploy from Visual Studio.

Follow these best practices and you will ensure your users are not limited by mobile backend deployments or load.

<!-- Images -->
[img1]: ./img/publish-to-slot.PNG

<!-- Links -->
[Azure portal]: https://portal.azure.com

