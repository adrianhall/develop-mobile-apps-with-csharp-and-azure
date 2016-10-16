I'd like to think that my mobile backend is self-managing.   Just like the App Service it is running on, it cleans up
after itself, always works and never needs a backup or maintenance.  Alas, some things can't be encoded in a client to
server transaction.  For example, in [chapter 3][1], we discussed offline synchronization and the need for soft delete.
The soft delete process just marks the records as deleted.  In a busy database, those deleted records still take up
space and resources during searches.  One of the maintenance tasks we need to do is to clean up the database.

There is almost always a need for [backend processing][4] that is not related to the client-server communication.  That is
where [Azure Functions][5] and [WebJobs][6] come in.  They handle the custom processing that is not initiated by a mobile
client.  There are several examples aside from the aforementioned soft delete cleanup:

* You [upload files][2] and want to process them before letting other users download them.
* You want to do [sentiment analysis][3] or other machine learning on incoming database records.
* You need to do workflows like order fulfillment.
* You need to handle data securely and cannot transmit it to the mobile client for processing.

In all these cases, the initiation may come from the mobile client or it may be scheduled.  However, the defining
characteristic of these requirements is that the code must be run asynchronously (the mobile client is not waiting
for the result) and may take longer than the time for a normal request.

You can consider Azure Functions as "WebJobs as a Service".  WebJobs run on the same set of virtual machines that
are running your mobile backend.  They have access to the same resources (like CPU and memory), so they can interfere
with the running of your app.  Azure Functions can run this way as well, but they really take off when running in
Dynamic Compute mode (which is the default).  In Dynamic Compute, they run in spare compute power and potentially
on a completely different set of virtual machines, so they don't interfere with your mobile backend.

I recommend WebJobs for clean-up or maintenance tasks - things like cleaning up the database on a regular basis.
Event-driven tasks should be handled by Azure Functions.

## The Database Clean Up WebJob

As an example WebJob, let's implement the database clean-up process as a WebJob.  This will run on a regular
basis (say, once a day) and during processing, it will delete all records in our TodoItem table that are
deleted where the updatedAt field is older than 7 days.  The SQL command to run is this:

```sql
DELETE FROM [dbo].[TodoItems]
WHERE
    [deleted] AND [updatedAt] < DATEADD(day, -7, SYSDATETIMEOFFSET())
GO
```

A WebJob is always a separate project from your mobile backend.  To create a WebJob:

* Right click the solution.
* Choose **Add** -> **New Item...**.
* Search for **WebJob**.

    ![][img1]

* Select **Azure WebJob** with a **Visual C#** language.
* Enter a suitable name for the WebJob and click **Add**.

Once the scaffolding has finished and the package restore is done, you will see a `Program.cs` file.  A WebJob
is just a console application running with the WebJobs SDK.  By default, the WebJob will wait around for a
trigger.  We don't have a trigger since we are going to run this code each and every time the scheduler runs
it.  As a result, our code in Program.cs is relatively simple:

```csharp
using System;
using System.Data.SqlClient;
using System.Diagnostics;

namespace CleanupDatabaseWebJob
{
    class Program
    {
        static void Main()
        {
            var connectionString = Environment.GetEnvironmentVariable("SQLCONNSTR_MS_TableConnectionString");

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    Debug.WriteLine("[CleanupDatabaseWebJob] Initiating SQL Connection");
                    sqlConnection.Open();

                    Debug.WriteLine("[CleanupDatabaseWebJob] Executing SQL Statement");
                    sqlCommand.CommandText = "DELETE FROM [dbo].[TodoItems] WHERE [deleted] AND[updatedAt] < DATEADD(day, -7, SYSDATETIMEOFFSET())";
                    var rowsAffected = sqlCommand.ExecuteNonQuery();
                    Debug.WriteLine($"[CleanupDatabaseWebJob] {rowsAffected} rows deleted.");

                    sqlConnection.Close();
                }
            }
        }
    }
}
```

To deploy a WebJob, we need to link it to our mobile backend:

* Right click the **Backend** project.
* Select **Add** then **Existing Project as Azure WebJob**.

    ![][img2]

* Fill in the **Project name** and **WebJob name** (if you named your project differently).
* In **WebJob run mode**, select **Run on a Schedule**.
* Pick a schedluel.  I picked:
    * **Recurrence** = **Recurring Job**
    * **No end date** checked
    * **Recur every** = 1 days
    * **Starting time** = 3 a.m.
    * **Ending time** = 4 a.m.

The limits for the ending time ensures that your WebJob doesn't run forever.  You may have a WebJob that runs
a complicated report each night to consolidate a lot of data into something that can be downloaded by your
mobile client.  These reports can sometimes run for hours.  You should place reasonable limits on your WebJob
to ensure that they don't affect the operation of your site.

Right click your **Backend** project and select **Publish...**  Since you have published before, the dialog
will be on the **Preview** tab.  Click on the **Preview** button to generate a view of what would happen:

![][img3]

The WebJob has been bundled with your mobile backend for publication.  If you log into the portal, there are
several things you can do.  Go to your mobile backend.  In the menu, select **WebJobs** to see your configured
WebJob.  You can trigger a run of the WebJob independently of the schedule that you configured when you linked
the WebJob to the mobile backend.  You can also view the logs for the WebJob.

<!-- Images -->
[img1]: ./img/webjob-create-1.PNG
[img2]: ./img/webjob-create-2.PNG
[img3]: ./img/webjob-create-3.PNG

<!-- Links -->
[1]: ../chapter3/dataconcepts.md
[2]: ./recipes.md#storage-related-operations
[3]: https://en.wikipedia.org/wiki/Sentiment_analysis
[4]: https://azure.microsoft.com/en-us/documentation/articles/best-practices-background-jobs/
[5]: https://azure.microsoft.com/en-us/services/functions/
[6]: https://azure.microsoft.com/en-us/documentation/articles/app-service-webjobs-readme/
