using System;

namespace TaskList.Abstractions
{
    public class TableData
    {
        public string Id { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public byte[] Version { get; set; }
    }
}
