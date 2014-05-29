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
            AccountDetails a1 = new AccountDetails();
            AccountDetails a2 = new AccountDetails();

            a1.Id = new System.Guid();
            a1.AccountNumber = "1111";
            a1.Money = 1000.0;

            a2.Id = new System.Guid();
            a2.AccountNumber = "2222";
            a2.Money = 1000.0;

            accountList.Add(a1);
            accountList.Add(a2);
        }

        public string CreateAccount(Guid clientId, AccountDetails details)
        {
            return "";
        }

        public List<AccountDetails> GetAllAccounts()
        {
            List<AccountDetails> list = new List<AccountDetails>();

            return list;
        }

        public List<AccountDetails> GetAccountsById(System.Guid clientId) {    
            List<AccountDetails> list = new List<AccountDetails>();

            return list;
        }
        
        public AccountDetails GetAccountInformation(string accountNumber)
        {
            Console.WriteLine("Żądanie informacji o koncie " + accountNumber);
            for (int i = 0; i < accountList.Count; i++)
            {
                if (accountList.ElementAt(i).AccountNumber.Equals(accountNumber))
                {
                    Console.WriteLine("Konto istnieje na pozycji " + i + " i ma na koncie " + accountList.ElementAt(i).Money);
                    return (AccountDetails) accountList.ElementAt(i);
                }
            }

            Console.WriteLine("Test");

            // konto nie istnieje
            return null;
        }

        public bool UpdateAccountInformation(AccountDetails details)
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

            return true;
        }
    }
}
