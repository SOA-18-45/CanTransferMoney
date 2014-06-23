using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CanTransferMoney
{
    class Config
    {
        public const int MaxBufferSize = 10000000;
        public const int MaxBufferPoolSize = 10000000;
        public const int MaxReceivedMessageSize = 10000000;
        public static TimeSpan ReceiveTimeout = new TimeSpan(0, 5, 0);
        public static TimeSpan SendTimeout = new TimeSpan(0, 5, 0);

        public const string ServiceName = "CanTransferMoney";
        public const string ServiceRepositoryURI = "net.tcp://192.168.0.102:50000/IServiceRepository";

        public const string CanTransferMoneyURI = "net.tcp://localhost:11901/ICanTransferMoney";
        public const string CanTransferMoneyHttpURI = "http://localhost:11902/ICanTransferMoney";


        public const string AccountRepositoryURI = "net.tcp://localhost:11900/IAccountRepository";
        public const string hibernatePath = "..\\..\\hibernate.cfg.xml";
        public const double Timeout = 5000;
        public const string mode = "dev";

        /* mode:d
         *  dev:     bez rejestracji w ServiceRepository
         *  prod:    rejestracja w ServiceRepository 
         */

        public const string storage = "database"; // database, memory

        public static string getStorage()
        {
            return storage;
        }

        public static string getServiceRepositoryURI()
        {
            return ServiceRepositoryURI;
        }

        public static string getCanTransferMoneyURI()
        {
            return CanTransferMoneyURI;
        }

        public static string getCanTransferMoneyHttpURI()
        {
            return CanTransferMoneyHttpURI;
        }

        public static string getAccountRepositoryURI()
        {
            return AccountRepositoryURI;
        }

        public static string getServiceName()
        {
            return ServiceName;
        }

        public static double getTimeout()
        {
            return Timeout;
        }

        public static string getMode()
        {
            return mode;
        }

        public static string getHibernatePath()
        {
            return hibernatePath;
        }
    }
}
