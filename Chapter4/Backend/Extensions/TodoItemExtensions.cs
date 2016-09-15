using System.Linq;
using Backend.DataObjects;

namespace Backend.Extensions
{
    public static class TodoItemExtensions
    {
        public static IQueryable<TodoItem> PerUserFilter(this IQueryable<TodoItem> query, string userId)
        {
            return query.Where(item => item.UserId.Equals(userId));
        }
    }
}