using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CanTransferMoneyTest
{
    class Config
    {
        public const string CanTransferMoneyURI = "net.tcp://localhost:11901/ICanTransferMoney";

        public static string getCanTransferMoneyURI()
        {
            return CanTransferMoneyURI;
        }
    }
}
