# Single Page Web Applications

One of the major changes that has happened within web applications in the past few years is the single page application, or SPA,
coupled with the rise of JavaScript frameworks.  No-one can directly support all the JavaScript frameworks out there, but this
section will cover a couple of the main ones - [jQuery][1], [React][2] and [Angular2][3].  Azure Mobile Apps has a [JavaScript SDK][4]
that can be used for accessing table controllers and identity services within your mobile backend.

## Basic JavaScript Usage

I refer to jQuery as a "plain JavaScript framework" because it relies on standard `<script>` tag loading within your main HTML
page.  The important thing to note here is that you cannot instantiate the Azure Mobile Apps client SDK until all the scripts
are loaded.  Fortunately, JavaScript (and jQuery) provide events when this happens.  I tend to add SPA applications to ASP.NET
MVC apps by making the main HTML page a View.  First, add a controller called "Controllers\SPAController" with the following contents:

```csharp
using System.Web.Mvc;

namespace Backend.Controllers
{
    public class SPAController : Controller
    {
        public ActionResult JQuery()
        {
            return View();
        }
    }
}
```

Also, create a directory `Views\SPA` and add a `JQuery.cshtml` file:

```html
@{
    Layout = null;
}

<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width">
    <title>JQuery</title>
    <link rel="stylesheet" href="~/Content/spa/jquery/application.css"/>
</head>
<body>
    <div id="wrapper">
        <article>
            <header>
                <h2>Azure</h2>
                <h1>Mobile Apps</h1>
                <form id="add-item">
                    <button type="submit" id="refresh">Refresh</button>
                    <button type="submit">Add</button>
                    <div>
                        <input type="text" id="new-item-text" placeholder="Enter new task"/>
                    </div>
                </form>
            </header>
            <ul id="todo-items"></ul>
            <p id="summary">Initializing...</p>
        </article>
        <footer>
            <ul id="errorlog"></ul>
        </footer>
    </div>
    <script src="https://code.jquery.com/jquery-2.2.1.min.js"></script>
    <script src="https://zumo.blob.core.windows.net/sdk/azure-mobile-apps-client.min.js"></script>
    <script src="~/Content/spa/jquery/application.js"></script>
</body>
</html>
```

This will be accessed via the `/SPA/JQuery` path.  Unlike other (mostly component based) applications,
jQuery uses mark-up extensively, so we are really setting up the basics of the application.  You can
find the CSS on the [GitHub repository][5] as its contents are not germane to the discussion here.

The main item to note in the scripts section is the path of the Azure Mobile Apps client.  This is loaded
from the ZUMO CDN.  You can also download the JavaScript file (both minified and non-minified versions exist),
and include it in your `Scripts` directory if you prefer a local copy.

The `~/Content/spa/jquery/application.js` file contains our JavaScript code.  It starts with a standard [IIFE][6]
(immediately invoked function expression) - a common method of wrapping code such that variables don't leak into
the global namespace:

```javascript
(function () {
    "use strict";

    $(onBrowserReady);

    /* Rest of the application */
})();
```

The important thing here is that we cannot invoke a client definition until the "DOMContentLoaded" event has fired,
which ensures that all libraries (including those slower libraries loaded from a CDN) have been loaded and executed.
This, in turn, ensures that the `WindowsAzure.MobileServiceClient` object is available.  The `onBrowserReady()` function
will be called when this happens:

```javascript
    var client, table;

    /**
     * Event handler, called when the browser has loaded all scripts
     * @event
     */
    function onBrowserReady() {
        // Create a connection reference to our Azure Mobile Apps backend
        client = new WindowsAzure.MobileServiceClient(location.origin);

        // Create a table reference
        table = client.getTable('todoitem');

        // Refresh the todoItems
        refreshDisplay();

        // Wire up the UI event handler for the add item
        $('#add-item').submit(addItemHandler);
        $('#refresh').on('click', refreshDisplay);
    }
```

Once the browser is ready for action, we create a `MobileServiceClient`, get a reference to the table, then wire up the rest
of the UI that we need to handle.  You may notice that the API that the JavaScript SDK for Azure Mobile Apps exposes is very
similar to the .NET SDK.  This is intentional across all the Azure Mobile Apps SDK.  If you know the API surface of one
version, it's likely you can take that knowledge to the other SDKs.  You only have to learn the new programming language.

## Reading Table Data

We can see this more clearly when working with the `refreshDisplay()` function:

```javascript
    /**
     * Refresh the items within a page
     */
    function refreshDisplay() {
        updateSummaryMessage('Loading data from Azure');

        // Execute a query for uncompleted items and process
        table
            .where({ complete: false })     // Set up the query
            .read()                         // Send query and read results
            .then(createTodoItemList, handleError);
    }

    /**
     * Create the DOM for a single todo item
     * @param {Object} item the Todo item
     * @param {string} item.id the ID of the item
     * @param {bool} item.complete true if the item is complete
     * @param {string} item.text the text value
     * @returns {jQuery} jQuery DOM object
     */
    function createTodoItem(item) {
        return $('<li>')
            .attr('data-todoitem-id', item.id)
            .append($('<button class="item-delete">Delete</button>'))
            .append($('<input type="checkbox" class="item-complete">')
                .prop('checked', item.complete))
            .append($('<div>')
                .append($('<input class="item-text">').val(item.text)));
    }

    /**
     * Create a list of Todo Items
     * @param {TodoItem[]} items an array of items
     * @returns {void}
     */
    function createTodoItemList(items) {
        // Cycle through each item received from Azure and add items to the item list
        var listItems = $.map(items, createTodoItem);
        $('#todo-items').empty().append(listItems).toggle(listItems.length > 0);
        updateSummaryMessage('<strong>' + items.length + '</strong> item(s)');

        // Wire up event handlers
        $('.item-delete').on('click', deleteItemHandler);
        $('.item-text').on('change', updateItemTextHandler);
        $('.item-complete').on('change', updateItemCompleteHandler);
    }
```

In the `refreshDisplay()` function, we can see the parallel with the LINQ language within the .NET world.  JavaScript
does not have a LINQ library, so Azure Mobile Apps provides a simplified version of LINQ (called [QueryJS][7]) for
use with Azure Mobile Apps.  Once you have set up the appropriate query, calling `.read()` will actually execute the
HTTP GET to obtain the results.  The other two functions fill in the list of tasks with the appropriate HTML trimmings.

> There is no concept of "offline-sync" in web applications as there is not an equivalent of the SQLite database available.

You can perform paging with `.skip(n)` and `.take(n)`, include the total number of records that would be returned without
paging with `.includeTotalCount()`, order the returned results and filter by function instead of a specific value.  For
example, let's say you had an orders table and you wanted to produce a paged list of results where the state was OPEN (a
constant) that belonged to a specific user, ordered by their completion date?:

```javascript
function filter (userId, state) {
    return this.owner === userId && this.state == state;
}

function processResults (results) {
    totalCount = results.totalCount;
    // Handle results here
}

table
    .where(filter, filteredUser, constants.OPEN)
    .orderBy('completionDate')
    .skip(startRecord)
    .take(pageSize)
    .includeTotalCount()
    .read()
    .then(processResults, handleError);
```

You can get just the total count using `.take(0).includeTotalCount()`.

QueryJS is extremely effective at (and optimized for) selecting the exact data you need to do live displays without extra
data being transferred.

## Modifying Data

In a similar way to the `.InsertAsync()`, `.UpdateAsync()` and `.DeleteAsync()` methods in the .NET SDK, there are methods
for inserting, modifying and deleting data:

```javascript
    /**
     * Given a sub-element of an LI, find the TodoItem ID associated with the list member
     *
     * @param {DOMElement} el the form element
     * @returns {string} the ID of the TodoItem
     */
    function getTodoItemId(el) {
        return $(el).closest('li').attr('data-todoitem-id');
    }

    /**
     * Event handler for when the user enters some text and clicks on Add
     * @param {Event} event the event that caused the request
     * @returns {void}
     */
    function addItemHandler(event) {
        var textbox = $('#new-item-text'),
            itemText = textbox.val();

        updateSummaryMessage('Adding New Item');
        if (itemText !== '') {
            table.insert({
                text: itemText,
                complete: false
            }).then(refreshDisplay, handleError);
        }

        textbox.val('').focus();
        event.preventDefault();
    }

    /**
     * Event handler for when the user clicks on Delete next to a todo item
     * @param {Event} event the event that caused the request
     * @returns {void}
     */
    function deleteItemHandler(event) {
        var itemId = getTodoItemId(event.currentTarget);

        updateSummaryMessage('Deleting Item in Azure');
        table
            .del({ id: itemId })   // Async send the deletion to backend
            .then(refreshDisplay, handleError); // Update the UI
        event.preventDefault();
    }

    /**
     * Event handler for when the user updates the text of a todo item
     * @param {Event} event the event that caused the request
     * @returns {void}
     */
    function updateItemTextHandler(event) {
        var itemId = getTodoItemId(event.currentTarget),
            newText = $(event.currentTarget).val();

        updateSummaryMessage('Updating Item in Azure');
        table
            .update({ id: itemId, text: newText })  // Async send the update to backend
            .then(refreshDisplay, handleError); // Update the UI
        event.preventDefault();
    }

    /**
     * Event handler for when the user updates the completed checkbox of a todo item
     * @param {Event} event the event that caused the request
     * @returns {void}
     */
    function updateItemCompleteHandler(event) {
        var itemId = getTodoItemId(event.currentTarget),
            isComplete = $(event.currentTarget).prop('checked');

        updateSummaryMessage('Updating Item in Azure');
        table
            .update({ id: itemId, complete: isComplete })  // Async send the update to backend
            .then(refreshDisplay, handleError);        // Update the UI
    }
```

We insert data by passing the object to insert to `table.insert()`, modify with `table.update()`
and delete with `table.del()`.  These functions all operate using promises.  Once the promise
returns, we call the `refreshDisplay()` method to refresh the data.  In the case of the `.insert()`
and `.update()` functions, the promise is called with the updated item (containing the extra fields
that the server adds), allowing the code to update a cache if necessary and avoiding a round-trip for
the search.

<!-- Links -->
[1]: http://jquery.com/
[2]: https://facebook.github.io/react/
[3]: https://angularjs.org/
[4]: https://www.npmjs.com/package/azure-mobile-apps-client
[5]: https://github.com/adrianhall/develop-mobile-apps-with-csharp-and-azure/blob/master/Chapter6/Backend/Content/spa/jquery/application.css
[6]: https://en.wikipedia.org/wiki/Immediately-invoked_function_expression
[7]: https://msdn.microsoft.com/en-us/library/azure/jj613353.aspx