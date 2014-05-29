using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Web;
using Contracts;


namespace CanTransferMoneyTest
{
    class Program
    {
        static ICanTransferMoney transferMoney;

        static void Main(string[] args)
        {
            connectToTransferMoney();

            string account1 = "1111", account2 = "2222";
            double transferValue = 10.0;
            Console.WriteLine("Wykonanie przelewu z konta {0} na konto {1} o wartości {2}", account1, account2, transferValue);
            transferMoney.TransferMoney(account1, account2, transferValue);
            Console.WriteLine();

            Console.WriteLine("Pobranie historii przelewów dla konta {0}", account1);
            List<HistoryItem> historyItems1 = transferMoney.TransferHistoryForAccount(account1);
            Console.WriteLine("Pobrano rekordy z history dotyczące {0}");
            Console.WriteLine(historyItems1.Count);

            for (int i = 0; i < historyItems1.Count; i++)
            {
                HistoryItem temp = historyItems1.ElementAt(i);
                Console.WriteLine("ID: {0}, AccountFrom: {1}, AccountTo: {2}, Value: {3}", temp.ID, temp.AccountFrom, temp.AccountTo, temp.Value);
            }

            Console.ReadLine();

        }

        static void connectToTransferMoney()
        {
            string Uri = Config.getCanTransferMoneyURI();
            ChannelFactory<ICanTransferMoney> cf = new ChannelFactory<ICanTransferMoney>(new NetTcpBinding(SecurityMode.None), Uri);
            transferMoney = cf.CreateChannel();
        }
    }
}
