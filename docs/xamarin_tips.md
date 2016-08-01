# Xamarin Forms Tips

Over the many months I have spent coding Xamarin Forms, a number of people
from the community and the Xamarin team have given me little tips to improve
my Xamarin Forms code.  I hope you find them as useful as I did.

## Improve your ListView performance

It should be no surprise that performance matters.  Little things like smooth
scrolling and fast load times are a must.  A lot of improvement can be gained
by the techniques we have shown in this book since most of the perceived delays
are caused by the back end loading times.

Eventually, a lot of apps generate a list.  The normal way for implementing
this is with a `ListView` that has an `ObservableCollection`  You load your
data into the ObservableCollection and then update that whenever the data
changes.  That, in turn, updates the ListView.

There are two problems that are inherent here.  The first is in the ObservableCollection
and the second is in the ListView.  Let's tackle the ObservableCollection first.

The problem with the ObservableCollection is that it's very hard to update.
What normally ends up happening is code like this:

```csharp
// Earlier in the code
var listContents = new ObservableCollection<Model>();

// When updating the code
var items = await table.ReadAllItemsAsync();
listContents.Clear();
for (var item in items) {
    listContents.Add(item);
}
```

The point of the ObservableCollection is that it emits an event whenever the
list changes.  In the case where the table has thousands of entries, thousands
of events will cause thousands of redraws, causing a major slow down in your
code that you probably won't know until you have a large enough data set to
note the problem.

Fortunately, one of the top Xamarin Evangelists, [James Montemagno][1], has
created a set of helper classes that assist with this sort of problem.  The
solution to this problem is to use the [ObservableRangeCollection][2], like
this:

```csharp
// Earlier in the code
var listContents = new ObservableRangeCollection<Model>();

// When updating the code
var items = await table.ReadAllItemsAsync();
listContents.ReplaceRange(items);
```

With this code, Xamarin Forms gets notified once instead of thousands of times.

As to the second problem.  A `ListView` with thousands of items will not be
showing all the items at once.  A ListView will update all the items that have
been updated, irrespective of whether they are visible or not.  The answer is
to use a caching strategy.  There are two potential caching strategies.  with
`RetainElement`, the ListView will generate a cell for each item in the list.
This is the default, but it's really only good for certain situations (most
notably when the cell has a large number of bindings).  For almost all situations,
the `RecycleElement` caching strategy should be used.  In this caching strategy,
the ListView will minimize the memory foot print and execution speed by only
updating cells when they are in the viewable area.  This is good pretty much
all the time, but explicitly when the cell has a small number of bindings or
when each cell in the list has the same template.  All data about the cell must
come from the binding context when using the `RecycleElement` caching strategy.

You can set the caching strategy right in the XAML:

```xml
<ListView CachingStrategy="RecycleElement" ...>
```

Alternatively, if you are creating a ListView in code, you can specify the
caching strategy in the constructor:

```csharp
var listView = new ListView(ListViewCachingStrategy.RecycleElement);
```

There are more techniques for improving ListView performance in the
[Xamarin documentation][3]

[1]: https://github.com/jamesmontemagno
[2]: https://github.com/jamesmontemagno/mvvm-helpers/blob/35f0ddd7e739eb5daed3c90cae1334d3e674229b/MvvmHelpers/ObservableRangeCollection.cs
[3]: https://developer.xamarin.com/guides/xamarin-forms/user-interface/listview/performance/
