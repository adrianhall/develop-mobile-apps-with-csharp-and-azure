using System.Collections.Generic;
using TaskList.Abstractions;

namespace TaskList.Services
{
    public class MockCloudService : ICloudService
    {
        public Dictionary<string, object> tables = new Dictionary<string, object>();

        public ICloudTable<T> GetTable<T>() where T : TableData
        {
            var tableName = typeof(T).Name;
            if (!tables.ContainsKey(tableName))
            {
                var table = new MockCloudTable<T>();
                tables[tableName] = table;
            }
            return (ICloudTable<T>)tables[tableName];
        }
    }
}
