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

            get: function () {
                var deferred = $q.defer();

                store.table.read().then(function (items) {
                    // Convert the items into todos for this application
                    var todoList = items.

                    angular.copy(todoList, store.todos);
                    deferred.resolve(store.todos);
                });

                return deferred.promise;
            },

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

        };

        var deferred = $q.defer();
        
        store.client = new WindowsAzure.MobileServiceClient(location.origin);
        store.table = store.client.getTable('todoitem');

        deferred.resolve(store);
        return deferred.promise;
    });
