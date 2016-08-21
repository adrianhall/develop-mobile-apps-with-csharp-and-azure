# Handling Data in Mobile Clients

Pretty much any non-trivial mobile client will require access to data.  Although we have already brushed on handling data within the client, this
section will go deeper into the data handling aspects on the client.  We will cover how to get and process data, how to deal with performance and
reliability and some of the quirks that one must deal with when dealing with offline data.

## An Online Client

We've already seen an example of an online client in our online `TaskList` project.  There is a method for obtaining a reference to an online table:

```csharp
var table = client.GetTable<Model>();
```

This method relies on the fact that the table name is the same as the model.  One must have a consistent naming scheme - the model on the server,
table controller on the server, model on the client and table on the client must all be based on the same root name.  This is definitely a best
practice.  You can produce an un-typed table:

```csharp
var table = client.GetTable("todoitem");
```

This version of the method returns an untyped table.  Whereas a typed table is based on a concrete model, an untyped table is based on a JSON
object.  This allows one to access data when the model is unknown or hard to represent in a model.  You should never use an untyped table unless
there is no other way of achieving whatever operation you need.

All tables implement the `IMobileServiceTable` interface:

> **ReadAsync()** performs reads against the table.
> **LookupAsync()** reads a single record in the table, identified by its id.
> **InsertAsync()** inserts a new record into the table.
> **UpdateAsync()** updates an existing record in the table.
> **DeleteAsync()** deletes a record in the table.
> **UndeleteAsync()** un-deletes a deleted record (if soft-delete is turned on).

When developing my interface, I tend to wrap my table interface into another class.  This isn't because I like wrapping classes.  Rather it is
because the return values from many of the methods are not compatible with the general patterns used when working with a UI.  For instance, the
ReadAsync() method returns an `IEnumerable<>` type.  However, the standard list management in Xamarin and UWP applications use an
`ObservableCollection<>` instead.  One has to do a conversion from one to the other.

Let's look at a standard table wrapper:

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using TaskList.Abstractions;

namespace TaskList.Services
{
    public class AzureCloudTable<T> : ICloudTable<T> where T : TableData
    {
        IMobileServiceTable<T> table;

        public AzureCloudTable(MobileServiceClient client)
        {
            this.table = client.GetTable<T>();
        }

        #region ICloudTable interface
        public async Task<T> CreateItemAsync(T item)
        {
            await table.InsertAsync(item);
            return item;
        }

        public async Task<T> UpsertItemAsync(T item)
        {
            return (item.Id == null) ?
                await CreateItemAsync(item) :
                await UpdateItemAsync(item);
        }

        public async Task DeleteItemAsync(T item)
            => await table.DeleteAsync(item);

        public async Task<ICollection<T>> ReadAllItemsAsync()
            => await table.ToListAsync();

        public async Task<T> ReadItemAsync(string id)
            => await table.LookupAsync(id);

        public async Task<T> UpdateItemAsync(T item)
        {
            await table.UpdateAsync(item);
            return item;
        }
        #endregion
    }
}
```

This is the `AzureCloudTable` class that our task list has been using thus far.  It's actually got a few bugs in it.  Let's go
over them.

Probably the most egregious bug is that the `ReadAllItemsAsync()` method does not handle paging.  If you have more than 50 items,
then the `ToListAsync()` method will do a single GET operation and then return the results.  The Azure Mobile Apps Server SDK implements
enforced paging.  This protects two things.  Firstly, the client cannot tie up the UI thread and cause a significant delay in the
responsiveness of the app.  More importantly, a rogue client cannot tie up the server for a long period thus helping with dealing
with denial of service attacks.  Paging is a good thing.

To test this:

* Insert over 50 records into the `TodoItems` table in your database using a SQL client.
* Put a break point at the `Items.ReplaceRange(list);` (line 78 approximately) in `ViewModels\TaskListViewModel.cs`.
* Run the UWP project.

![][not-paging]

Note that even though there are more than 50 records, you will only see 50 records in the list.  There are multiple ways to fix
this and it depends on your final expectation.  In the class of "probably not what we want", we can keep on reading records until
there are no more records to read. This is the simplest to implement.  In the `Services\AzureCloudTable.cs` file, replace the
`ReadAllItemsAsync()` method with the following:

```csharp
```



## An Offline Client

## Query Management

<!-- Images -->
[not-paging]: img/not-paging.PNG
