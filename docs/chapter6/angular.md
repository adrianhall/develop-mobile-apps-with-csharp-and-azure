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

## Pre-requisites

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


<!-- Links -->
[1]: https://docs.angularjs.org
[2]: https://docs.angularjs.org/tutorial
[3]: https://www.youtube.com/user/angularjs
[4]: https://vslive.com/Blogs/News-and-Tips/2016/02/4-Must-Read-Angular-Blogs.aspx
[5]: https://github.com/tastejs/todomvc/tree/gh-pages/examples/angularjs
[6]: http://todomvc.com/
[7]: https://github.com/adrianhall/develop-mobile-apps-with-csharp-and-azure/tree/master/Chapter6
[8]: https://nodejs.org/en/
