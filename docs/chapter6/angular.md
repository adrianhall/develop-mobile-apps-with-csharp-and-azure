# Angular Applications

For much of the last couple of years, [Angular][1] has been the JavaScript framework of choice For
front-end developers.  Developed by Google, it is fully-featured, albeit complex, but with a
solid community of developers willing to help, and the support for learning through [tutorials][2],
[videos][3], and [blogs][4].  The most recent iteration, Angular 2, is taking off as a great framework
as well.

Learning a new framework is time consuming, but the outcomes can be remarkable.  In this section, I'm
going to adjust a single class in the [Angular version][5] of [ToDoMVC][6] so that it works with the Azure
Mobile Apps JavaScript SDK.  You can find the full source code in the [Chapter6][7] project on the books
GitHub page.

## Angular in ASP.NET MVC

Before we get started, let's get the default ToDoMVC application running in our ASP.NET MVC framework.

### Add a Controller Method and a View

Edit the `Controllers\SPAController.cs` and add the following method:

```csharp
    public ActionResult Angular()
    {
        return View();
    }
```

Also, add the following in `Views\SPA\Angular.cshtml`:

```html
@{
    Layout = null;
}

<!doctype html>
<html lang="en" data-framework="angularjs">
<head>
    <meta charset="utf-8">
    <title>AngularJS • TodoMVC</title>
    <link rel="stylesheet" href="~/Content/spa/todomvc/base.css">
    <link rel="stylesheet" href="~/Content/spa/todomvc/index.css">
    <style>
        [ng-cloak] {
            display: none;
        }
    </style>
</head>
<body ng-app="todomvc">
    <ng-view />

    <script type="text/ng-template" id="todomvc-index.html">
        <section id="todoapp">
            <header id="header">
                <h1>todos</h1>
                <form id="todo-form" ng-submit="addTodo()">
                    <input id="new-todo" placeholder="What needs to be done?" ng-model="newTodo" ng-disabled="saving" autofocus>
                </form>
            </header>
            <section id="main" ng-show="todos.length" ng-cloak>
                <input id="toggle-all" type="checkbox" ng-model="allChecked" ng-click="markAll(allChecked)">
                <label for="toggle-all">Mark all as complete</label>
                <ul id="todo-list">
                    <li ng-repeat="todo in todos | filter:statusFilter track by $index" ng-class="{completed: todo.completed, editing: todo == editedTodo}">
                        <div class="view">
                            <input class="toggle" type="checkbox" ng-model="todo.completed" ng-change="toggleCompleted(todo)">
                            <label ng-dblclick="editTodo(todo)">{{todo.title}}</label>
                            <button class="destroy" ng-click="removeTodo(todo)"></button>
                        </div>
                        <form ng-submit="saveEdits(todo, 'submit')">
                            <input class="edit" ng-trim="false" ng-model="todo.title" todo-escape="revertEdits(todo)" ng-blur="saveEdits(todo, 'blur')" todo-focus="todo == editedTodo">
                        </form>
                    </li>
                </ul>
            </section>
            <footer id="footer" ng-show="todos.length" ng-cloak>
                <span id="todo-count">
                    <strong>{{remainingCount}}</strong>
                    <ng-pluralize count="remainingCount" when="{ one: 'item left', other: 'items left' }"></ng-pluralize>
                </span>
                <ul id="filters">
                    <li>
                        <a ng-class="{selected: status == ''} " href="#/">All</a>
                    </li>
                    <li>
                        <a ng-class="{selected: status == 'active'}" href="#/active">Active</a>
                    </li>
                    <li>
                        <a ng-class="{selected: status == 'completed'}" href="#/completed">Completed</a>
                    </li>
                </ul>
                <button id="clear-completed" ng-click="clearCompletedTodos()" ng-show="completedCount">Clear completed</button>
            </footer>
        </section>
        <footer id="info">
            <p>Double-click to edit a todo</p>
            <p>
                Credits:
                <a href="http://twitter.com/cburgdorf">Christoph Burgdorf</a>,
                <a href="http://ericbidelman.com">Eric Bidelman</a>,
                <a href="http://jacobmumm.com">Jacob Mumm</a> and
                <a href="http://blog.igorminar.com">Igor Minar</a>
            </p>
            <p>Adjusted for Azure Mobile Apps by <a href="https://github.com/adrianhall">Adrian Hall</a>.</p>
            <p>Part of <a href="http://todomvc.com">TodoMVC</a></p>
        </footer>
    </script>
    <script src="~/Content/spa/todomvc/base.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/angular.js/1.4.14/angular.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/angular.js/1.4.14/angular-route.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/angular.js/1.4.14/angular-resource.min.js"></script>
    <script src="~/Content/spa/angular/app.js"></script>
    <script src="~/Content/spa/angular/controllers/todoCtrl.js"></script>
    <script src="~/Content/spa/angular/services/todoStorage.js"></script>
    <script src="~/Content/spa/angular/directives/todoFocus.js"></script>
    <script src="~/Content/spa/angular/directives/todoEscape.js"></script>
</body>
</html>
```

I haven't done much to these except adjust the script links to resolve to the JavaScript CDN.  In addition, there
are some basic CSS/JS libraries that all the ToDoMVC applications use.  I've copied those from the ToDoMVC
site into my project in `~/Content/spa/todomvc`. 

### Copy the ToDoMVC application into Content

I've created a directory `~/Content/spa/angular` with a direct copy of the [AngularJS][5] application.  Everything
under the `js` directory has been copied, preserving the directory structure. 

At this point, you can publish your application and you will see the original ToDoMVC application prior to our
making it work with Azure Mobile Apps.

## Cloud Connectivity

The logic for the storage of the data behind the task list all happens in `services\todoStorage.js`, and our
changes are all limited to that file.  In this case, we will have a local cache of the data.  This local
cache is read at the beginning of the application.  When the user wants to make a change to the data, we 
modify the data locally and remotely at the same time.

We have a small complexity - the model used by ToDoMVC does not match the model on the backend.  As a result,
we need to do conversions between the two models when we perform backend operations.  This is surprisingly common,
especially when using backend databases that you do not control.

Let's start with the basics.  Here is the recipe for the promise-based Angular factory, with our 
Azure Mobile Apps initializer embedded:

```javascript
/*global angular */

/**
 * Services that persists and retrieves todos from localStorage or a backend API
 * if available.
 *
 * They both follow the same API, returning promises for all changes to the
 * model.
 */
angular.module('todomvc')
    .factory('todoStorage', function ($q) {
        var store = {
            todos: [],
            client: null,
            table: null,

            // Additional methods Here
            get: function() {

            },

            delete: function(todo) {

            },

            insert: function(todo) {

            },

            put: function(todo, index) {

            },

            clearCompleted: function() {

            }
        };

        var deferred = $q.defer();
        
        store.client = new WindowsAzure.MobileServiceClient(location.origin);
        store.table = store.client.getTable('todoitem');

        deferred.resolve(store);
        return deferred.promise;
    });
```

Since our application is doing all the filtering client-side, we are going to cache the
table data in the store.todos variable.  I've created the API that the ToDoMVC application
expects, but with empty contents.  Each method is expected to return a promise that resolves
to the new list of todo items.

Getting the data is easier than the jQuery version as we don't have to deal with filtering:

```javascript
    get: function () {
        var deferred = $q.defer();

        store.table.read().then(function (items) {
            // Convert the items into todos for this application
            var todoList = items.map(function (item) {
                return {
                    id: item.id,
                    completed: item.complete,
                    title: item.text
                };
            });

            angular.copy(todoList, store.todos);
            deferred.resolve(store.todos);
        });

        return deferred.promise;
    },
```

Angular comes with a A+/Promise library that is referenced in a similar way to either the regular
Promise API or like the jQuery deferred API.  Here, I am creating a promise, then doing the work,
resolving the promise when the work is complete.  The `Array.map()` method in JavaScript is great
for doing the work of converting one model to a new shape.

Deleting, Updating and Inserting are all very similar to the jQuery version.  Since we are maintaing
a cache, we don't resolve the promise we need until the server comes back with the new data:

```javascript
    delete: function (todo) {
        var deferred = $q.defer();

        store.table.del({ id: todo.id }).done(function () {
            store.todos.splice(store.todos.indexOf(todo), 1);
            deferred.resolve(store.todos);
        });

        return deferred.promise;
    },

    insert: function (todo) {
        var deferred = $q.defer();

        store.table.insert({ text: todo.title }).then(function (newItem) {
            todo.id = newItem.id;
            todo.title = newItem.text;
            todo.completed = newItem.complete;

            store.todos.push(todo);
            deferred.resolve(store.todos);
        });

        return deferred.promise;
    },

    put: function (todo, index) {
        var deferred = $q.defer();

        store.table.update({ id: todo.id, text: todo.title, complete: todo.completed })
            .then(function (item) {
                todo.title = item.text;
                todo.completed = item.complete;
                store.todos[index] = todo;
                deferred.resolve(store.todos);
            });

        return deferred.promise;
    }
```

The major reason for not updating the cache directly is that we need the ID of the new record.
That ID is created on the server for us.  We could, as an improvement, include the `uuid` package
and generate a GUID on the client, storing that instead.

Finally, there is a method for clearing (aka deleting) the completed records.  This is difficult
primarily because the server only handles one record at a time.  The Angular promise library has
an API for that called `.all()`.  This method is given an array of promises and waits for all of 
them to be resolved.  We can use this as follows:

```javascript
    clearCompleted: function () {
        var deferred = $q.defer();

        var promises = [];

        var completeTodos = store.todos.filter(function (todo) { return todo.completed; });
        completeTodo.forEach(function (todo) {
            promises.push(store.table.del({ id: todo.id }));
        });

        $q.all(promises).then(function () {
            var incompleteTodos = store.todos.filter(function (todo) {
                return !todo.completed;
            });

            angular.copy(incompleteTodos, store.todos);
            deferred.resolve(store.todos);
        });

        return deferred.promise;
    },
```

We spend our initial time creating a promise for each record to be deleted.  That promise resolves
when the record is deleted.  Once all the records have been deleted, we filter the cache similarly.

## Angular Gotchas

The main problem I see over and over is that the `WindowsAzure.MobileServiceClient` class is not
available when it is used.  By default, Angular waits for the DOMContentLoaded event, which signals
that all the scripts have been loaded.  Inevitably, when I look at the failing code, the call to
initialize the MobileServiceClient is called outside of a factory. 

If you place the `new WindowsAzure.MobileServiceClient()` call inside of a service or factory, then
you gain the two major problems.  Firstly, the MobileServiceClient class will be available.  Secondly,
you instantiate a singleton copy of the MobileServiceClient.  

## Authentication

The Azure Mobile Apps JavaScript SDK includes a call `client.login('provider')` for server-flow
authentication and a similar functionality for client-flow authentication.  If you have configured
your authentication service in the Azure Portal properly, then calling the `.login()` method will 
pop up a small window to complete the normal authentication flow.  The token is then stored inside 
the MobileServiceClient object.

When using authentication this way, it is vital that you have a singleton model for your MobileServiceClient.
In this case, I would break down the backend connectivity into three or more distinct services - one for the
client connection, one for authenticating users, and one for each table controller you wish to expose.


<!-- Links -->
[1]: https://docs.angularjs.org
[2]: https://docs.angularjs.org/tutorial
[3]: https://www.youtube.com/user/angularjs
[4]: https://vslive.com/Blogs/News-and-Tips/2016/02/4-Must-Read-Angular-Blogs.aspx
[5]: https://github.com/tastejs/todomvc/tree/gh-pages/examples/angularjs
[6]: http://todomvc.com/
[7]: https://github.com/adrianhall/develop-mobile-apps-with-csharp-and-azure/tree/master/Chapter6
[8]: https://nodejs.org/en/
