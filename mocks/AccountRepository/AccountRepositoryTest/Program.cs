using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contracts;

namespace AccountRepositoryTest
{
    class Program
    {
        static void Main(string[] args)
        {
            AccountRepository.AccountRepository accountRepository = new AccountRepository.AccountRepository();

            Console.WriteLine("Pobieranie informacji o koncie 1111");
            AccountDetails account1 = accountRepository.GetAccountInformation("1111");
            Console.WriteLine("Pobrano informacje:");
            Console.WriteLine(String.Format("Id: {0}", account1.Id));
            Console.WriteLine(String.Format("Account number: {0}", account1.AccountNumber));
            Console.WriteLine(String.Format("Money: {0}", account1.Money));
            Console.WriteLine();

            Console.WriteLine("Aktualizacja informacji o koncie 1111 (aktualizacja Money oraz Id)");
            double Money = 500.0;
            Guid Id = new Guid();
            account1.Money = Money;
            account1.Id = Id;
            accountRepository.UpdateAccountInformation(account1);
            Console.WriteLine("Zaaktualizowano konto.");
            Console.WriteLine();

            Console.WriteLine("Pobieranie informacji o koncie 1111");
            account1 = accountRepository.GetAccountInformation("1111");
            Console.WriteLine("Pobrano informacje:");
            Console.WriteLine(String.Format("Money == {0} ? {1}", Money, account1.Money == Money));
            Console.WriteLine(String.Format("Id == {0} ? {1}", Id, account1.Id == Id));
            Console.WriteLine();

            Console.WriteLine("Pobieranie informacji o koncie 0000 (nie istnieje)");
            AccountDetails account2 = accountRepository.GetAccountInformation("0000");
            Console.WriteLine(String.Format("account == null ? {0}", account2 == null));
            
            Console.ReadLine();
        }
    }
}
