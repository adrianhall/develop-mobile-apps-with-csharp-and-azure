# Relationships

One of the biggest benefits to using a SQL database over a NoSQL store is relationships between entities.  Relationships
provide the ability to normalize the data, allowing you to store the minimal amount of data for a specific use case on
the mobile device.  This reduces bandwidth usage and memory usage on the device.  Relationships are a good thing.

Unfortunately, relationships between tables are hard when one is working within an offline context.  This is primarily
caused by the need for resilience.  Because we can do many updates to the tables on the offline client, the transactions
that update the tables need to be co-ordinated.  This is practically impossible in an offline context where one of the
goals in bandwidth performance.

Azure Mobile Apps, when used in an offline context, has an operations table.  As you do each operation against a table,
an entry is made in the operations table.  The operations table is then replayed in order to the mobile backend to
effect changes in the remote database.  However, this also has the effect that we do not have transactions to allow the
updating of multiple tables within the database at the same time.  Each record in each table is updated individually.
The push process that offline sync uses has major ramifications for how relationships between tables work.  Specifically,
only 1-way relationships will work in an offline sync world.

!!! note "1-Way Relationships"
    You can define relationships in Entity Framework with or without a virtual back-reference.  Relationships without
    the virtual back-reference are known as 2-way relationships (because you can get back to the original model).  
    Relationships with only a forward reference (and no knowledge of the original model) are said to have a 1-way
    relationship.  A database model with only 1-way relationships can generally be represented with a tree structure.

Let's take a quick example.  We've been using the "task list" scenario for our testing thus far.  Let's say that each
task could be assigned a tag from a list of tags.   We can use a 1-way 1:1 relationship between the tasks and the tags.
To do that, we would store the Id of the tag in the task model.  If, however, we could attach many tags to a single
task, that would be a 1:Many relationship.

## 1:1 Relationships

Let's take a look at our task list example, from the perspective of the models on the server side:

```csharp
using Microsoft.Azure.Mobile.Server;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace ComplexTypes.DataObjects
{
    public class Tag : EntityData
    {
        public string TagName { get; set; }
    }
 
    public class TodoItem : EntityData
    {
        public string Text { get; set; }
 
        public bool Complete { get; set; }
 
        #region Relationships
        public string TagId { get; set; }
 
        [ForeignKey("TagId")]
        public Tag Tag { get; set; }
        #endregion
    }
}
```

1:1 relationships are defined using a foreign key in the SQL database.  We can use Entity Framework to define the foreign
key relationship easily.  In this case, our `TodoItem` model will, have a TagId that contains the Id field of the tag.  I
also created a pair of table controllers for these models in the normal manner.  Finally, I've created some records using
the `Seed()` method within the `App_Start\Startup.MobileApp.cs` file to give us some test data.

If we take a look at records through Postman, we will get the following:

![][relationships-1]

Note that the first item has a reference to a tag, by virtue of the TagId.  The second item does not have a tag assigned,
so the value of TagId is null.

When we implement the client, we are going to download these tables indepdendently.  The linkage and relationships between
the tables is lost when going from the backend to the offline client.  We have to link them together ourselves.  This is
why the "1-way" relationship is necessary.  In a 2-way relationship, a tag and task would have to be created at the same
time as part of an SQL transaction.  In a 1-way relationship, the tag can be created THEN the task that has the relationship
is created

> When you think of all the mobile applications you own, you will realize that 1-way relationships are the normal state of
affairs.  Very few relationships actually have to have the two-way relationship.

When you are developing the mobile client, the `Tag` is removed from the model:

```csharp
using TaskList.Helpers;
 
namespace TaskList.Models
{
    public class Tag : TableData
    {
        public string TagName { get; set; }
    }
 
    public class TodoItem : TableData
    {
        public string Text { get; set; }
        public bool Complete { get; set; }
 
        public string TagId { get; set; }
    }
}
```

One can easily retrieve the tag information with a LINQ query on the Tag table:

```csharp
var tag = tagTable.FirstOrDefault(tag => tag.Id.Equals(task.TagId)).Value;
```

You have to ensure that you create a tag before associating that tag with a task.  Once that is done, the natural ordering
within the operations table will ensure the tag is created on the mobile backend prior to being used.  At that point, the 
linkage between the tag table and the task table will be established.

!!! note "Using Relationships in Existing Databases"
    There are a lot of instances where 2-way relationships were established in the past, generally as part of an 
    existing application.  When we project these tables into the mobile offline realm, things break.  We have two
    options here.  Firstly, we can adjust the application and underlying database to use 1-way relationships.  
    Alternatively, we can use a custom API to do the transaction for us in an online manner.  


<!-- Images -->
[relationships-1]: img/relationships-1.PNG
