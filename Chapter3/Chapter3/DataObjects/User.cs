using Microsoft.Azure.Mobile.Server;

namespace Chapter3.DataObjects
{
    public class User : EntityData
    {
        public string EmailAddress { get; set; }
        public string Name { get; set; }
    }
}