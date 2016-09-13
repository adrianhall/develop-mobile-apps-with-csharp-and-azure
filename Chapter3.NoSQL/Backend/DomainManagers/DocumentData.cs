using System;
using Microsoft.Azure.Mobile.Server.Tables;

namespace Backend.DomainManagers
{
    public class DocumentData : ITableData
    {
        public DateTimeOffset? CreatedAt { get; set; }

        public bool Deleted { get; set; }

        public string Id { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public byte[] Version { get; set; }
    }
}