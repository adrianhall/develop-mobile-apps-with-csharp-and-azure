using Microsoft.Azure.Mobile.Server;
using System.Collections.Generic;

namespace Backend.DataObjects
{
    public class Track : EntityData
    {
        public Track() { }

        public string Title { get; set; }
        public int Length { get; set; }

        public virtual Album Album { get; set; }
    }

    public class Album : EntityData
    {
        public Album()
        {
            Tracks = new List<Track>();
        }

        public string Title { get; set; }

        public virtual ICollection<Track> Tracks { get; set; }
    }
}