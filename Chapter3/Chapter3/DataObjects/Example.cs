using System;
using Microsoft.Azure.Mobile.Server;

namespace Chapter3.DataObjects
{
    public class Example : EntityData
    {
        public string StringField { get; set; }
        public int IntField { get; set; }
        public double DoubleField { get; set; }
        public DateTimeOffset DateTimeField { get; set; }
    }
}