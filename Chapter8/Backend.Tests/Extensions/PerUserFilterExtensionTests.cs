using Backend.DataObjects;
using Backend.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Backend.Tests.Extensions
{
    public class PerUserFilterExtensionTests
    {
        [Fact]
        public void UserId_Is_Valid()
        {
            List<TodoItem> items = new List<TodoItem>
            {
                new TodoItem { UserId = "test", Text = "Task 1", Complete = false },
                new TodoItem { UserId = "test2", Text = "Task 2", Complete = true },
                new TodoItem { UserId = "test", Text = "Task 3", Complete = false }
            };

            var result = items.AsQueryable<TodoItem>().PerUserFilter("test");

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void UserId_Is_Empty()
        {
            List<TodoItem> items = new List<TodoItem>
            {
                new TodoItem { UserId = "test", Text = "Task 1", Complete = false },
                new TodoItem { UserId = "test2", Text = "Task 2", Complete = true },
                new TodoItem { UserId = "test", Text = "Task 3", Complete = false }
            };

            var result = items.AsQueryable<TodoItem>().PerUserFilter(String.Empty);

            Assert.NotNull(result);
            Assert.Equal(0, result.Count());
        }

        [Fact]
        public void UserId_Is_Null()
        {
            List<TodoItem> items = new List<TodoItem>
            {
                new TodoItem { UserId = "test", Text = "Task 1", Complete = false },
                new TodoItem { UserId = "test2", Text = "Task 2", Complete = true },
                new TodoItem { UserId = "test", Text = "Task 3", Complete = false }
            };

            var result = items.AsQueryable<TodoItem>().PerUserFilter(null);

            Assert.NotNull(result);
            Assert.Equal(0, result.Count());
        }
    }
}
