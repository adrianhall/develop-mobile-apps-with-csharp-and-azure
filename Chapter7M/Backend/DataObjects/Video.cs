using Microsoft.Azure.Mobile.Server;

namespace Backend.DataObjects
{
    public class Video : EntityData
    {
        public string Filename { get; set; }

        public string ImageUri { get; set; }
    }
}
