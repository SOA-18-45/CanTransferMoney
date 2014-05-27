using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceRepository
{
    class Config
    {
        public const int MaxBufferSize = 10000000;
        public const int MaxBufferPoolSize = 10000000;
        public const int MaxReceivedMessageSize = 10000000;
        public const int ReceiveTimeout = 10000000;
        public const string ServiceURI = "net.tcp://localhost:11900/IServiceRepository";
    }
}
