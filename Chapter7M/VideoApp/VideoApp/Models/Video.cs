using VideoApp.Abstractions;

namespace VideoApp.Models
{
    public class Video : TableData
    {
        public string Filename { get; set; }

        public string VideoUri { get; set; }
    }
}
