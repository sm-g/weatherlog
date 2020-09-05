using System;
using System.Runtime.Serialization;

namespace Weatherlog.Models
{
    [Serializable]
    public class NoStationOffsetException : Exception, ISerializable
    {
        public NoStationOffsetException()
        {
        }

        public NoStationOffsetException(string message)
            : base(message)
        {
        }

        public NoStationOffsetException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected NoStationOffsetException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
