using NEU.IPGateway.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace NEU.IPGateway.Driver
{
    public class IPGWException : ConnectionException
    {
        public IPGWException(ConnectionError type) : base(type)
        {
        }

        public IPGWException(ConnectionError type, string message) : base(type, message)
        {
        }

        public IPGWException(string message) : base(ConnectionError.Unclear, message)
        {
        }
    }
}
