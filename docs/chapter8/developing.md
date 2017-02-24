# The Development Environment

In these last two chapters, I want to go over some of the complexities of developing mobile applications when there is a
cloud-enabled backend.  Working with cloud services presents its own special challenges, especially when you use the
features of the provider.  In the case of Azure App Service, this means that dealing with App Service Authentication
and App Service Push requires some special configuration.

## Working with Azure Mobile Apps Locally

In general, you can use Azure Mobile Apps locally by running inside a local IIS container.  If you are using
Visual Studio, then this is set up for you.  However, you will need to handle the SQL database connection.
You have two choices for this.  Firstly, you can use a SQL Azure instance and just point your server to that
instance.  Secondly, you can install SQL Express and use that instance instead.  Both are perfectly viable
options.  I like to use SQL Express in early development, then switch over to a SQL Azure instance as I get
closer to deployment.  Switching over when you are close to deployment enables to you detect any problems
in upgrades or the use of encrypted channels.

### Configuring SQL Express for Local Development

Start by downloading and installing the [Microsoft SQL Server Express][1] edition.  Azure Mobile Apps will work
with just about any recent SQL Server Express edition.  I personally recommend the
[**SQL Server 2016 SP1 Express**][2] edition.


The process by which SQL Server Express is installed varies by edition and version, so keep these tips in mind.

*  Always elect the custom installation option.
*  You need the Database Engine and the [Management Tools][3] (possibly via separate download)
*  You do not need reporting or integration services.
*  Use **Mixed mode** for authentication and set an **sa** password.
*  If possible, place the data directories on a different disk.

Once you have installed the database engine and management tools, you will need to create a user that has
permissions to create databases:

1.  Run the SQL Server Management Studio and connect to your local SQL Express instance.
2.  Ensure the **SQL Server and Windows Authentication mode** is checked in the **Properties** > **Security** page.
3.  Expand **Security** > **Logins** in the **Object Explorer**.
4.  Right-click the **Logins** node and select **New login...**.
    a.  Enter a unique login name
    b.  Select **SQL Server authentication**.
    c.  Enter a password, then enter the same password in **Confirm password**.
    d.  Click **OK**.
5.  Right-click on your new login and select **Properties**.
    a.  Click **Server Roles** under **Select a page**.
    b.  Check the box next to the **dbcreator** role.
    c. Click **OK**.
6.  Close the SQL Server Management Studio.

Ensure you record the username and password you selected.  You may need to assign additional server roles
or permissions depending on your specific database requirements.  Your connection string will need to look
like this:

```text
Server=127.0.0.1; Database=mytestdatabase; User Id=azuremobile; Password=T3stPa55word;
```

Replace the user ID and password with the user ID and password you just created.  The database will be created
for you, so ensure it is rememberable.  To set the connection string, you will need to edit the `Web.config`
file.  At around line 12, you will see the default connection string.  Simply replace it with your SQL Express
connection string:

```xml
  <connectionStrings>
    <add name="MS_TableConnectionString"
        connectionString="Data Server=127.0.0.1; Database=mytestdatabase; User Id=azuremobile; Password=T3stPa55word;"
        providerName="System.Data.SqlClient" />
  </connectionStrings>
```

You should now be able to press F5 on your server and run it locally.

### Configuring SQL Azure for Local Development

I have already discussed creating a SQL Azure instance.  By default, however, the SQL Azure instance
can only be used by other Azure resources as a security measure.  There is a firewall that limits
connectivity from the Internet to your database.  To run your server locally while connecting to the
SQL Azure instance, you need to do two things:

1.  Open the firewall for connections from your IP address.
2.  Update the connection string in your servers `Web.config` file.

From your development system (the workstation that you will be using to run the service locally),
open a browser and log into the [Azure portal].  You will note that there are two resources for your
SQL database - a server and a database.  Open the resource for the SQL server, then:

*  Click the **Firewall** menu option.
*  Click **+ Add client IP**.
*  Click **Save**, then **OK**.

If you are using a different workstation to run the service, then you can enter an explicit IP
address.  To get the connection strings:

*  From the SQL Server **Overview** page, click the SQL database.
*  From the SQL database **Overview** page, click **Show database connection strings**.
*  Copy the ADO.NET (SQL authentication) connection string.

This will need to be copied into the `Web.config` in the same way as the SQL Express version (above).
You will need to replace the `{your_username}` and `{your_password}` strings with the username and
password of your SQL server.  If you can't remember them, look in your App Service - they are available
in the configured connection string under **Application Settings**.

Once this done, you will be able to press F5 on your server and run it locally.

## Handling Cloud Services while Developing Locally

### Handling Authentication

### Handling Push Notifications

## Debugging your Cloud Mobile Backend

### Diagnostic Logging

### Using the Visual Studio Debugger

<!-- Links -->
[Azure portal]: https://portal.azure.com/
[1]: https://www.microsoft.com/en-us/sql-server/sql-server-editions-express
[2]: https://go.microsoft.com/fwlink/?LinkID=799012
[3]: https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms

