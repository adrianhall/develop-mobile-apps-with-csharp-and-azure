# Complex Types

A complex type has many meanings in development.  For our purposes, a complex type is any type that cannot be represented
be a JSON primitive.  The JSON primitives are string, number and boolean.  Azure Mobile Apps already provides a mapping
for a complex type.  The `DateTimeOffset` class is a complex type.  However, Azure Mobile Apps transforms this into a
string for transport.  It is represented as a UTC time stamp. 

Without special treatment, complex types are serialized by the JSON.Net serializer into an object format.  For example, let
us say we have the following complex type:

```csharp
namespace ComplexTypes.Types
{
    public class Position
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}
```

Let us further say that we use this complex type as part of a Data Transfer Object (DTO):

```csharp
using ComplexTypes.Types;
using Microsoft.Azure.Mobile.Server;

namespace ComplexTypes.DataObjects
{
    public class Track : EntityData
    {
        public Position Location { get; set; }
    }
}
```

We can use the normal mechanism to create an associated `TableController`.  Finally, let's produce 
some seed data for this (in `App_Start\Startup.MobileApp.cs`):

```csharp
protected override void Seed(MobileServiceContext context)
{
    List<Track> tracks = new List<Track>
    {
        new Track {
            Id = Guid.NewGuid().ToString(),
            Location = new Position { Longitude = 1.0, Latitude = 1.0 }
        },
        new Track {
            Id = Guid.NewGuid().ToString(),
            Location = new Position { Longitude = 89.6, Latitude = 77.4 }
        }
    };
    context.Set<Track>().AddRange(tracks);

    string json = JsonConvert.SerializeObject(tracks);
    Debug.WriteLine($"JSON Serialization: {json}");

    base.Seed(context);
}
```

The `Debug.WriteLine()` statement in the Seed method will tell us what the serialization of the complex
type is:

```
JSON Serialization: [
    {
        "Location":{
            "Longitude":1.0,
            "Latitude":1.0
        },
        "Id":"1dccb623-d777-407e-8a44-779a283e11ca",
        "Version":null,
        "CreatedAt":null,
        "UpdatedAt":null,
        "Deleted":false
    },
    {
        "Location":{
            "Longitude":89.6,
            "Latitude":77.4
        },
        "Id":"194c9f4e-4134-486d-bfa5-3e8d5c2cfd53",
        "Version":null,
        "CreatedAt":null,
        "UpdatedAt":null,
        "Deleted":false
    }
]
```

I've "pretty-printed" the JSON output for readability.  Note that the complex type is an object, so we have an
object embedded in an object at this point.  If you perform a `GET /tables/tracks` against this server with a
HTTP client like Postman, you will see the following:

![][complex-1]

Note that the Location field is not mentioned at all in the output.  The Azure Mobile Apps Server SDK squashes
the complex types before transmitting them on the wire.  They are treated as not even there.  

> The reason that dates are handled already is because the JSON.NET serializer converts them to ISO-8601 format,
and that is a string.

To properly represent a complex type on the wire, we have to store it as something that can be handled - a string,
number or boolean.  Our complex type could be represented, for example, by the string `POSITION:{long=1.0,lat=1.0}`.  
This conversion provides all the necessary data to re-constitute the object on the other end.

<!-- Images -->
[complex-1]: img/complex-1.PNG


