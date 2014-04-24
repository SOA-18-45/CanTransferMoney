using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;


namespace ICanTransferMoney
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }

    [ServiceContract]
    public interface ICanTransferMoney
    {
        [OperationContract]
        int transferMoney(long accountNumber1, long accountNumber2, decimal value);
    }

    private class TransferMoney : ICanTransferMoney
    {
        int transferMoney(long accountNumber1, long accountNumber2, decimal value)
        {
            //zgloszenie sie do servicu IAccountRepository po struktury AccountDetails dla accountNumber1 i 2.
            //sprawdzenie istnienia obydwu kont i dostępnych środków na 1.
            //skorzystanie z kontraktu zmieniającego stan kont - wywołanie go na obydwu accountach
            //zapis do bazy historii transakcji -- account1, account2, value, date
            
            return 1;
        }
    }


}
