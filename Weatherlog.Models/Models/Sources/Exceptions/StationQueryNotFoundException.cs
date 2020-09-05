using System;
using System.Runtime.Serialization;

namespace Weatherlog.Models
{
    [Serializable]
    public class StationQueryNotFoundException : Exception, ISerializable
    {
        public StationQueryNotFoundException()
        {
        }

        public StationQueryNotFoundException(string message)
            : base(message)
        {
        }

        public StationQueryNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected StationQueryNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
