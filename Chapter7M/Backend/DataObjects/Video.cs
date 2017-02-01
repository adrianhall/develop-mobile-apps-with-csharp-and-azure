using Microsoft.Azure.Mobile.Server;

namespace Backend.DataObjects
{
    public class Video : EntityData
    {
        public string Title { get; set; }

        public string VideoUri { get; set; }

        public string ImageUri { get; set; }
    }
}