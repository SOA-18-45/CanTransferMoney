﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AccountRepository
{
    class Config
    {
        public const int MaxBufferSize = 10000000;
        public const int MaxBufferPoolSize = 10000000;
        public const int MaxReceivedMessageSize = 10000000;
        public static TimeSpan ReceiveTimeout = new TimeSpan(0, 5, 0);
        public static TimeSpan SendTimeout = new TimeSpan(0, 5, 0);
        public const string ServiceRepositoryURI = "net.tcp://localhost:11902/IServiceRepository";
        public const string AccountRepositoryURI = "net.tcp://localhost:11900/IAccountRepository";
    }
}
