using ComplexTypes.Types;
using Microsoft.Azure.Mobile.Server;

namespace ComplexTypes.DataObjects
{
    public class Track : EntityData
    {
        public Position Location { get; set; }
    }
}