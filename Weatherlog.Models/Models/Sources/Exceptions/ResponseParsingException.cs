using System;
using System.Runtime.Serialization;

namespace Weatherlog.Models
{
    [Serializable]
    public class ResponseParsingException : Exception, ISerializable
    {
        public ResponseParsingException()
        {
        }

        public ResponseParsingException(string message)
            : base(message)
        {
        }

        public ResponseParsingException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ResponseParsingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
