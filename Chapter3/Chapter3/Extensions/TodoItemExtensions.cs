using System.Linq;
using Chapter3.DataObjects;

namespace Chapter3.Extensions
{
    public static class TodoItemExtensions
    {
        public static IQueryable<TodoItem> PerUserFilter(this IQueryable<TodoItem> query, string userid)
        {
            return query.Where(item => item.UserId.Equals(userid));
        }
    }
}