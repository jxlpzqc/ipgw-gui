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

        private string _message;

        public override string Message
        {
            get => _message;
        }

        public ConnectionException(ConnectionError type)
        {
            ErrorType = type;
            if(type == ConnectionError.InvalidCredient)
            {
                _message = "用户名或密码错误";
            }
        }
        public ConnectionException(ConnectionError type, string message) : base(message)
        {

            ErrorType = type;
            _message = message;
        }
        public ConnectionException(ConnectionError type, string message, Exception inner) : base(message, inner)
        {
            ErrorType = type;
            _message = message;
        }

        protected ConnectionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
