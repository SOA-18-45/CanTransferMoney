using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using Contracts;

namespace AccountRepository
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AccountRepository : IAccountRepository
    {
        List<AccountDetails> accountList = new List<AccountDetails>();

        public AccountRepository()
        {
            accountList.Add(new MockAccountDetails(new System.Guid(), "1111", 1000.0));
        }

        public string CreateAccount(Guid clientId, AccountDetails details)
        {
            return "";
        }

        public AccountDetails GetAccountInformation(string accountNumber)
        {
            Console.WriteLine("Żądanie informacji o koncie " + accountNumber);
            for (int i = 0; i < accountList.Count; i++)
            {
                if (accountList.ElementAt(i).AccountNumber.Equals(accountNumber))
                {
                    Console.WriteLine("Konto istnieje na pozycji " + i + " i ma na koncie " + accountList.ElementAt(i).Money);
                    return accountList.ElementAt(i);
                }
            }

            // konto nie istnieje
            return null;
        }

        public void UpdateAccountInformation(AccountDetails details)
        {
            Console.WriteLine("Żądanie aktualizacji konta " + details.AccountNumber);
            for (int i = 0; i < accountList.Count; i++)
            {
                if (accountList.ElementAt(i).AccountNumber.Equals(details.AccountNumber))
                {
                    double oldMoney = accountList.ElementAt(i).Money;
                    accountList.ElementAt(i).Money = details.Money;
                    Console.WriteLine("Zaaktualizowano " + accountList.ElementAt(i).AccountNumber + ". Stara wartość " + oldMoney + ".  Nowa wartość " + accountList.ElementAt(i).Money);
                    break;
                }
            }
        }
    }
}
