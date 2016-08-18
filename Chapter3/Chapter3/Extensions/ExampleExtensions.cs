using System.Collections.Generic;
using System.Linq;
using Chapter3.DataObjects;

namespace Chapter3.Extensions
{
    public static class ExampleExtensions
    {
        public static IQueryable<Example> PerGroupFilter(this IQueryable<Example> query, List<string> groups)
        {
            return query.Where(item => groups.Contains(item.GroupId));
        }
    }
}