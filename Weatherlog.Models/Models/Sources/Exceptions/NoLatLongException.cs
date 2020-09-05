using System;
using System.Runtime.Serialization;

namespace Weatherlog.Models
{
    [Serializable]
    public class NoLatLongException : Exception, ISerializable
    {
        public NoLatLongException()
        {
        }

        public NoLatLongException(string message)
            : base(message)
        {
        }

        public NoLatLongException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected NoLatLongException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
