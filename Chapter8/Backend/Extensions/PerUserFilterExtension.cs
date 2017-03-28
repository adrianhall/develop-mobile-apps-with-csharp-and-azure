using Backend.DataObjects;
using System.Linq;

namespace Backend.Extensions
{
    public static class PerUserFilterExtension
    {
        public static IQueryable<TodoItem> PerUserFilter(this IQueryable<TodoItem> query, string userid)
        {
            return query.Where(item => item.UserId.Equals(userid));
        }
    }
}