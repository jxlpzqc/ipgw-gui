using System;
using System.Collections.Generic;
using System.Text;

namespace NEU.IPGateway.Core
{
    public enum ConnectionError
    {
        LostPin,
        InvalidPin,
        InvalidCredient,
        NotConnected,
        Unclear,
    }


    [Serializable]
    public class ConnectionException : Exception
    {
        public ConnectionError ErrorType { get; set; }
        
        public ConnectionException(ConnectionError type)
        {
            ErrorType = type;
        }
        public ConnectionException(ConnectionError type, string message) : base(message)
        {

            ErrorType = type;
        }
        public ConnectionException(ConnectionError type, string message, Exception inner) : base(message, inner)
        {
            ErrorType = type;
        }

        protected ConnectionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
