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

* **ReadAsync()** performs reads against the table.
* **LookupAsync()** reads a single record in the table, identified by its id.
* **InsertAsync()** inserts a new record into the table.
* **UpdateAsync()** updates an existing record in the table.
* **DeleteAsync()** deletes a record in the table.
* **UndeleteAsync()** un-deletes a deleted record (if soft-delete is turned on).

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
public async Task<ICollection<T>> ReadAllItemsAsync()
{
    List<T> allItems = new List<T>();

    var pageSize = 50;
    var hasMore = true;
    while (hasMore)
    {
        var pageOfItems = await table.Skip(allItems.Count).Take(pageSize).ToListAsync();
        if (pageOfItems.Count > 0)
        {
            allItems.AddRange(pageOfItems);
        }
        else
        {
            hasMore = false;
        }
    }
    return allItems;
}
```

This code could be simplified quite a bit.  The reason I am not doing so is that this is not how you would want to do the transfer
of items in a real application.  Doing this will tie up the UI thread of your application for quite a while as the `AzureCloudTable`
downloads all the data.  Consider if there were thousands of entries?  This method would be problematic very quickly.

The alternative is to incrementally load the data as it is needed.  This means that your UI thread will pause as the data is loaded,
but the resulting UI will be less memory hungry and overall more responsive.  We start by adjusting our `Abstractions\ICloudTable.cs`
to add a method signature for returning paged data:

```csharp
public interface ICloudTable<T> where T : TableData
{
    Task<T> CreateItemAsync(T item);
    Task<T> ReadItemAsync(string id);
    Task<T> UpdateItemAsync(T item);
    Task<T> UpsertItemAsync(T item);
    Task DeleteItemAsync(T item);
    Task<ICollection<T>> ReadAllItemsAsync();
    Task<ICollection<T>> ReadItemsAsync(int start, int count);
}
```

The `ReadItemsAsync()` method is our new method here.  The concrete implementation usese `.Skip()` and `.Take()` to return just the
data that is required:

```csharp
public async Task<ICollection<T>> ReadItemsAsync(int start, int count)
{
    return await table.Skip(start).Take(count).ToListAsync();
}
```

Now that we have a method for paging through the contents of our table, we need to be able to wire that up to our `ListView`.  Xamarin Forms
has a concept called [Behaviors][1] that lets us add functionality to user interface controls without having to completely re-write them or
sub-class them.  We can use a behavior to implement a reusable paging control for a ListView.  Xamarin provides a sample for this called
[EventToCommandBehavior][2] (along with an [explanation][3]). We are going to be using the [ItemAppearing][4] event and that event uses the
[ItemVisibilityEventArgs][5] as a parameter.  We need a converter for the EventToCommandBehavior class (in `Converters\ItemVisibilityConverter.cs`):

```csharp
using System;
using System.Globalization;
using Xamarin.Forms;

namespace TaskList.Converters
{
    public class ItemVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var eventArgs = value as ItemVisibilityEventArgs;
            return eventArgs.Item;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
```

This is wired up with some XAML code in `Pages\TaskList.xaml.cs`.  There are two pieces.  Firstly, we must define the ItemVisibilityConverter
that we just wrote.  This is done at the top of the file:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage x:Class="TaskList.Pages.TaskList"
             xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:behaviors="clr-namespace:TaskList.Behaviors;assembly=TaskList"
             xmlns:converters="clr-namespace:TaskList.Converters;assembly=TaskList"
             Title="{Binding Title}">

    <ContentPage.Resources>
        <ResourceDictionary>
            <converters:ItemVisibilityConverter x:Key="ItemVisibilityConverter" />
        </ResourceDictionary>
    </ContentPage.Resources>
```

Next, we must define the behavior for the ListView:

```xml
<ListView CachingStrategy="RecycleElement"
            IsPullToRefreshEnabled="True"
            IsRefreshing="{Binding IsBusy,
                                    Mode=OneWay}"
            ItemsSource="{Binding Items}"
            RefreshCommand="{Binding RefreshCommand}"
            RowHeight="50"
            SelectedItem="{Binding SelectedItem,
                                    Mode=TwoWay}">
    <ListView.Behaviors>
        <behaviors:EventToCommandBehavior Command="{Binding LoadMoreCommand}"
                                                Converter="{StaticResource ItemVisibilityConverter"
                                                EventName="ItemAppearing" />
    </ListView.Behaviors>
```

Finally, we need to add a new command to our `TaskListViewModel` to load more items.  This involves
firstly defining the new command:

```csharp
public TaskListViewModel()
{
    CloudTable = CloudService.GetTable<TodoItem>();

    Title = "Task List";

    RefreshCommand = new Command(async () => await Refresh());
    AddNewItemCommand = new Command(async () => await AddNewItem());
    LogoutCommand = new Command(async () => await Logout());
    LoadMoreCommand = new Command<TodoItem> (async (TodoItem item) => await LoadMore(item));

    // Subscribe to events from the Task Detail Page
    MessagingCenter.Subscribe<TaskDetailViewModel>(this, "ItemsChanged", async (sender) =>
    {
        await Refresh();
    });

    // Execute the refresh command
    RefreshCommand.Execute(null);
}

public ICommand LoadMoreCommand { get; }
```

We also need to define the actual command code:

```csharp
bool hasMoreItems = true;

async Task LoadMore(TodoItem item)
{
    if (IsBusy)
    {
        Debug.WriteLine($"LoadMore: bailing because IsBusy = true");
        return;
    }

    // If we are not displaying the last one in the list, then return.
    if (!Items.Last().Id.Equals(item.Id))
    {
        Debug.WriteLine($"LoadMore: bailing because this id is not the last id in the list");
        return;
    }

    // If we don't have more items, return
    if (!hasMoreItems)
    {
        Debug.WriteLine($"LoadMore: bailing because we don't have any more items");
        return;
    }

    IsBusy = true;
    try
    {
        var list = await CloudTable.ReadItemsAsync(Items.Count, 20);
        if (list.Count > 0)
        {
            Debug.WriteLine($"LoadMore: got {list.Count} more items");
            Items.AddRange(list);
        }
        else
        {
            Debug.WriteLine($"LoadMore: no more items: setting hasMoreItems= false");
            hasMoreItems = false;
        }
    }
    catch (Exception ex)
    {
        await Application.Current.MainPage.DisplayAlert("LoadMore Failed", ex.Message, "OK");
    }
    finally
    {
        IsBusy = false;
    }
}
```

I've added a whole bunch of debug output because this command is called a lot, so I can scroll back through the
output window instead of setting a breakpoint and clicking Continue a lot.

As the UI displays each cell, it calls our command.  The command figures out if the record being displayed is the
last one in the list.  If it is, it asks for more records.  Once no more records are available, it sets the
flag `hasMoreItems` to false so it can short-circuit the network request.

Finally, our current implementation of the `Refresh()` method loads all the items.  We need to adjust it
to only load the first page:

```csharp
async Task Refresh()
{
    if (IsBusy)
        return;
    IsBusy = true;

    try
    {
        var identity = await CloudService.GetIdentityAsync();
        if (identity != null)
        {
            var name = identity.UserClaims.FirstOrDefault(c => c.Type.Equals("name")).Value;
            Title = $"Tasks for {name}";
        }
        var list = await CloudTable.ReadItemsAsync(0, 20);
        Items.ReplaceRange(list);
        hasMoreItems = true;
    }
    catch (Exception ex)
    {
        await Application.Current.MainPage.DisplayAlert("Items Not Loaded", ex.Message, "OK");
    }
    finally
    {
        IsBusy = false;
    }
}
```

We've done two things here.

* We have altered the first request so that only the first 20 records are retrieved.
* We have set `hasMoreItems` to true so that the `LoadMore()` command will do network requests again.

### Query Support in Online Clients

When using an online client, you can also use an OData query to look for records.  The following code
snippet, for example, will only return records that are incomplete:

```csharp
return await table
    .Where(item => item.Complete == false)
    .ToListAsync()
```

This is a standard LINQ query.  Just as the LINQ query was used to adjust the SQL that is generated in
the server-side code, the LINQ query here is used to adjust the OData query that is generated to call
the server.  This particular query will generate the follow HTTP call:

```
GET /tables/todoitem?$filter=(complete+eq+false) HTTP/1.1
```

LINQ queries are very useful in dealing with online data.  In general they should take a specific forms

```csharp
table                           // start with the table reference
    .Where(filter)              // filter the results
    .Skip(start).Take(count)    // paging support
    .ToListAsync()              // convert to something we can use
```

## An Offline Client



## Query Management

<!-- Images -->
[not-paging]: img/not-paging.PNG

<!-- Links -->
[1]: https://developer.xamarin.com/guides/xamarin-forms/behaviors/introduction/
[2]: https://developer.xamarin.com/samples/xamarin-forms/behaviors/eventtocommandbehavior/
[3]: https://developer.xamarin.com/guides/xamarin-forms/behaviors/reusable/event-to-command-behavior/
[4]: https://developer.xamarin.com/api/event/Xamarin.Forms.ListView.ItemAppearing/
[5]: https://developer.xamarin.com/api/type/Xamarin.Forms.ItemVisibilityEventArgs/
