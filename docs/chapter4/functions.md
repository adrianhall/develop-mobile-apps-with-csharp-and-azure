WebJobs run in the context of your App Service Plan, which means they inherit the scaling capabilities of that plan,
and may cause the site to scale unnecessarily.  What should you do if you don't want this to happen?

Enter [Azure Functions][1].  Azure Functions are a technology for running WebJobs in a dynamic compute context.  Dynamic
Compute is a relative newcomer to the cloud computing stage and part of a new paradigm known as "[Serverless][2]".

Let's take a tour through the history of cloud.  In the beginning, cloud providers provided virtual machines, networking
and storage, also known as "[Infrastructure as a Service][3]" or IaaS.  You built a cloud service in much the same way
as you built an on-premise solution.  You cede control of the hardware to the cloud provider, but you are responsible
for the maintenance of the platform.

"[Platform as a Service][4]" or PaaS is a step up from this.  With PaaS, you cede control of the operating system, security
patching and platform maintenance to the cloud provider.  You are responsible for the code that is running your app.
With PaaS, however, you are still somewhat responsible and aware of the underlying infrastructure.  You generally are
making decisions on when to add a new virtual machine to the pool for scaling, for example.  The virtual machine is
present.

"[Software as a Service][5]" (or SaaS) is the opposite end of the cloud services to IaaS.  You are not responsible for
anything in the platform.  You just use the software.

![][img1]

A good analogy is how to get a meal.  IaaS is akin to going to the grocery store, picking all the ingredients you
need, preparing the ingredients, cooking the meal, and serving the meal to you and your guest.  SaaS is akin to
going out to a restaurant and telling the waiter what you want.  PaaS is similar to food box delivery services -
they provide the ingredients and the recipe, but you do the cooking.

In both of these cases, you are trading off management convenience for management control.  IaaS has lots of control,
but you have to do pretty much all the management yourself.   SaaS is run for you, but you have no control.  PaaS
is in between these two extremes.

But what about when you want even more management convenience than PaaS can offer, but you still want to run your
application?  Something inbetween PaaS and Saas, where scaling issues are taken care of for you.  This is where
[Serverless][2] technologies and dynamic compute come in.  With Serverless, you still manage your code.  However, they
are infinitely scalable.  That comes at a cost in terms of flexibility.  Serverless does not mean "without servers".
There are still servers involved.  You just don't need to manage them in any way.

Azure Functions are an implementation of Serverless technology to allow WebJobs to be written that happen in dynamic
compute.  You pay for the number of executions.  (Technically, pricing is more complex than this simplification, but
you will notice that your price goes up as the number of executions goes up).  You can consider Azure Functions as
"WebJobs as a Service".

This isn't the only Serverless technology on the Azure platform.  [Azure Logic Apps][6] is also a Serverless technology,
covering Workflow.  Function execution is also not the only serverless technology.  You can find examples in
authentication, message queuing, edge caching, search and others.

It's quite possible to write a mobile backend entirely in Azure Functions.  However, this is undesirable mostly
because some processes (most notably SQL database access) require a relatively lengthy cold-start process.  Functions
are short-lived processes and may change the server that they are running on frequently.  The mobile backend can
provide efficiencies in this scenario by keeping a pool of connections open to the SQL database.  This efficiency
is not possible in Azure Functions.

## Building an Azure Function

The first thing to note about Azure Functions is that they are a completely separate resource in the Azure Portal.
This means that they are built separately, charged separately and scale independently to your mobile backend.  This
is in contrast to WebJobs, where the WebJob shares infrastructure with the mobile backend and scales with the mobile
backend.

Start by logging in to the Azure Portal.

* Click on the **+ NEW** button, or the **+ Add** button on a resource group.
* Search for **Function App**, then click **Create**.
* Enter a unique name for the Function App.  The Function App is still an App Service, so you cannot name
   the Function App the same as your mobile backend.
* Set the **App Service plan** to be **Dynamic**.
* Select an initial memory allocation (MB) - if you don't know (and why would you?), select **128**.
* Pick your storage account that you created for the WebJobs demos (or create a new one).
* Click on **Create**.

There are a few pieces here that are new.  Firstly, all App Services have an App Service Plan.  The choices for
Azure Functions are "Classic" and "Dynamic".  A Classic plan acts like WebJobs.  You need to specify the details
on how big the virtual machines are and how many of them you want.  If you are going to do this, just write
a WebJob.  The true nature of Azure Functions lies in the "Dynamic" plan.

The second item of interest is the memory allocation.  The service is asking you to estimate how much memory
you think your Functions are going to need on a per-function execution.  I'm not quite sure how they expect
you to know this up front.  Fortunately, this number can be tweaked after the plan is created.  If you find
your functions are running out of memory, then you can increase it.  There are logs that will show the
exceptions your functions threw.  Look for "OutOfMemoryException" to see if the memory allocation needs to
be increased.

![][img2]

Once the deployment is complete, you will notice two (or potentially three) new resources.  The lightning
bolt type icon is the Function App and you will spend most of your time there.  You will also have a dynamic
app service plan - in this case, called SouthCentralUSPlan.  Finally, you might have an additional storage
account.   Just like WebJobs, Azure Functions needs a storage account to store runtime state and logs.

Now that you have a Function App, you can start creating functions.  Click on your Function App to open
the Functions Console.  Let's start by creating the same two WebJobs that we created in the last section,
but using Azure Functions.

## Database Cleanup with Azure Functions

Our first WebJob was a database cleanup process that was run on a schedule.  If this is your first Function,
then click on _Create your own custom function_.  If not, you can click on the _+ New Function_ link in the
side bar.  Your next task is to select a template:

![][img3]

We want a **TimerTrigger** for this example.  Note that we can write in multiple languages.  C# and JavaScript
are supported out of the box.  You can also bring other languages.  F# is a popular choice, for example, but
other supported languages include Python, PowerShell, bash, and PHP.  Click on the **TimerTrigger - C#** template,
which is near the end of the list.

You will need to name your function (I called mine **DatabaseCleanup**) and configure a schedule for the trigger.
The schedule is in a simplified cron-style expression.  3am is written as _0 0 3 * * *_.  Once you have set the
two fields, click on **Create**.

!!! tip
    You can create as many functions as you want inside of the Function App.  They will all run independently,
    so there is no reason to need more than one Function App.

At this point you have a fully functional Azure Function.  In the **Develop** tab, you can click on **Run** to
run your function.  The **Logs** panel will show you the logs for running the function.  If there is any output,
you can see it in the **Output** panel.  You can edit your code in-line.  There is just one method - the **Run()**
method.  It looks quite like the WebJob.  The trigger comes first, the output binding second and the **TraceWriter**
comes last for logging.  In a timer job, there is no output binding.

Replace the code within the editor with the following:

```csharp
#r "System.Data"

using System;
using System.Configuration;
using System.Data.SqlClient;

public static void Run(TimerInfo myTimer, TraceWriter log)
{
    var connectionString = ConfigurationManager.ConnectionStrings["MS_TableConnectionString"].ConnectionString;
    log.Info($"Using Connection String {connectionString}");

    using (var sqlConnection = new SqlConnection(connectionString))
    {
        using (var sqlCommand = sqlConnection.CreateCommand())
        {
            log.Info("Initiating SQL Connection");
            sqlConnection.Open();

            log.Info("Executing SQL Statement");
            sqlCommand.CommandText = "DELETE FROM [dbo].[TodoItems] WHERE [deleted] = 1 AND [updatedAt] < DATEADD(day, -7, SYSDATETIMEOFFSET())";
            var rowsAffected = sqlCommand.ExecuteNonQuery();
            log.Info($"{rowsAffected} rows deleted.");

            sqlConnection.Close();
        }
    }
}
```

Notice the `#r` directive.  Azure Functions comes with some built-in references.  They are listed in the [C# Reference][7].
System.Data is not one of those references, so you have to bring it in yourself.  The `#r` directive brings in the reference.

Other than that, this looks remarkably like the WebJob that does the same thing.  This is normal.  In fact, it's ok to
develop the functionality in WebJobs and then translate the WebJob to a function once you have it working.  There isn't
much tooling for Azure Functions right now, so you will likely be stuck with online editing.  Aside from the `#r`, you
will notice some other things:

* Functions doesn't use `Console`.  It provides a `TraceWriter` for logging instead.
* The signature of the **Run()** method is different than WebJobs.

Once you save the file, the function is automatically compiled.  If there are any compilation errors, they will show
up in the Logs panel.  If you click **Run** right now, you will see an error.  That's because the connection string
is not defined.  In WebJobs, you defined this connection string in the `App.config` file.  In Azure Functions, you
just have to set the connection string up:

* Click **Function app settings** in the lower left corner.
* Click **Go to App Service Settings**.
* Find and click **Data Connections**.

Now connect your database to the function app in the same way that you did for the mobile backend.  This reinforces,
for me anyway, that the Function App is an App Service.  It uses the same menu structure under the covers.  You can
also set additional app settings, link storage, and so on in the same way as on App Services.

When you click on **Run** in your function now, you will see the log output.

## Image Resize with Azure Functions

Let's create another C# Function.  Before you start, link your storage account to the Function App using the Data
Connections in the same way as you did your SQL database.  Then click on the **+ New Function** button and select
the **BlobTrigger - C#** template.

!!! warn
    Your storage account must be in the same region as your Function App.  In general, resources that talk to one
    another should be colocated in the same region.  However, this is a requirement for Azure Functions.


<!-- Images -->
[img1]: ./img/platform-view.PNG
[img2]: ./img/functions-creation-1.PNG
[img3]: ./img/functions-creation-2.PNG

<!-- Links -->
[1]: https://azure.microsoft.com/en-us/documentation/articles/functions-overview/
[2]: http://martinfowler.com/bliki/Serverless.html
[3]: https://en.wikipedia.org/wiki/Cloud_computing#Infrastructure_as_a_service_.28IaaS.29
[4]: https://en.wikipedia.org/wiki/Cloud_computing#Platform_as_a_service_.28PaaS.29
[5]: https://en.wikipedia.org/wiki/Cloud_computing#Software_as_a_service_.28SaaS.29
[6]: https://azure.microsoft.com/en-us/documentation/articles/app-service-logic-what-are-logic-apps/
[7]: https://azure.microsoft.com/en-us/documentation/articles/functions-reference-csharp/
