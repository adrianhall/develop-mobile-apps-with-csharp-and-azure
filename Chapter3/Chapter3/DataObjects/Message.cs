using System.Collections.Generic;
using Microsoft.Azure.Mobile.Server;

namespace Chapter3.DataObjects
{
    public class Message : EntityData
    {
        public string UserId { get; set; }
        public string Text { get; set; }
        public ICollection<Tag> Tags { get; set; }
    }
}