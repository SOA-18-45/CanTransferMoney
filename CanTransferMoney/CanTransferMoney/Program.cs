using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Web;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System.Timers;
using CanTransferMoney.Domain;
using Contracts;

namespace CanTransferMoney
{
    class Config
    {
        public const string ConfigSrc = @"..\..\..\..\config.txt";
        public const string ServiceRepositoryURI = "net.tcp://192.168.0.109:50000/IServiceRepository";

        public const string CanTransferMoneyURI = "net.tcp://192.168.0.95:50005/ICanTrasferMoney";
    }

    class Program
    {

        static void Main(string[] args)
        {
            //read configuration from file
            string Delimiter = "=";
            string configFile = System.IO.File.ReadAllText(Config.ConfigSrc);
            string[] serviceParameters = configFile.Split(new[] { Delimiter }, StringSplitOptions.None);


            // creation of transferMoney object
            MoneyTransfer transferMoney = new MoneyTransfer();

            //create service host of transferMoney object
            ServiceHost sh = new ServiceHost(transferMoney, new Uri[] { new Uri(Config.CanTransferMoneyURI) });

            ServiceMetadataBehavior metadata = sh.Description.Behaviors.Find<ServiceMetadataBehavior>();

            if (metadata == null)
            {
                metadata = new ServiceMetadataBehavior();
                sh.Description.Behaviors.Add(metadata);
            }

            metadata.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;

            // create endpoint of transferMoney service
            sh.AddServiceEndpoint(typeof(ICanTransferMoney), new NetTcpBinding(SecurityMode.None), serviceParameters[3]);
            sh.Open();
            Logger.log("Serwis uruchomiony...");

            //connect to IServiceRepository service
            Logger.log("Próba rejestracji w ServiceRepository");
           // Logger.log(serviceParameters[5]);
            ChannelFactory<IServiceRepository> cf = new ChannelFactory<IServiceRepository>(new NetTcpBinding(SecurityMode.None), Config.ServiceRepositoryURI);
            IServiceRepository serviceRepository = cf.CreateChannel();
            
            //register service in IServiceRepository service
            serviceRepository.registerService(serviceParameters[1], serviceParameters[3]);
            Logger.log("Zarejestrowano w ServiceRepository");

            //enable imAlive method every 5 seconds
            var timer = new System.Threading.Timer(e => imAlive(serviceRepository), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            /*
            IAccountRepository accountRepository;
            string IAccountRepositoryAddress = serviceRepository.getServiceAddress("IAccountRepository");

            string AccountNumber1 = "1111";
            Console.WriteLine(IAccountRepositoryAddress);
            ChannelFactory<IAccountRepository> cf2 = new ChannelFactory<IAccountRepository>(new NetTcpBinding(SecurityMode.None), IAccountRepositoryAddress);
            accountRepository = cf2.CreateChannel();

            AccountDetails account1 = accountRepository.GetAccountInformation(AccountNumber1);*/

            //Console.WriteLine(account1.Money);
            transferMoney.TransferMoney("1111", "2222", 150.00);
            //transferMoney.TransferMoney("234", "123", 123.49);
            Console.ReadLine();
        }



        private static void imAlive(IServiceRepository serviceRepository)
        {
            serviceRepository.isAlive(Config.ServiceRepositoryURI);
            Logger.log("Wysyłanie sygnału IamAlive");
        }
    }

    public class historyItem
    {
        public DateTime date;
        public string accountNumber1;
        public string accountNumber2;
        public double value;

        public historyItem(DateTime date, string accountNumber1, string accountNumber2, double value)
        {
            this.date = date;
            this.accountNumber1 = accountNumber1;
            this.accountNumber2 = accountNumber2;
            this.value = value;
        }
    }

    public static class Logger
    {
        public static void log(string message)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter("log.txt", true);
            file.WriteLine(message);
            file.Close();
            Console.WriteLine(message);
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class MoneyTransfer : ICanTransferMoney
    {
        IServiceRepository serviceRepository;
        IAccountRepository accountRepository;
        List<historyItem> historyList;
        public MoneyTransfer()
        {
            historyList = new List<historyItem>();
            ChannelFactory<IServiceRepository> cf = new ChannelFactory<IServiceRepository>(new NetTcpBinding(SecurityMode.None), Config.ServiceRepositoryURI);
            serviceRepository = cf.CreateChannel();
        }

        public int TransferMoney(string AccountNumber1, string AccountNumber2, double value)
        {
            LoadHibernateCfg();
            //pozwala na wykonanie przelewu na podstawie numeru kont

            string IAccountRepositoryAddress = serviceRepository.getServiceAddress("IAccountRepository");
            ChannelFactory<IAccountRepository> cf = new ChannelFactory<IAccountRepository>(new NetTcpBinding(SecurityMode.None), IAccountRepositoryAddress);
            accountRepository = cf.CreateChannel();

            AccountDetails account1 = accountRepository.GetAccountInformation(AccountNumber1);
            Logger.log("Stan konta " + AccountNumber1 + " przed operacją: " + account1.Money);

            AccountDetails account2 = accountRepository.GetAccountInformation(AccountNumber2);
            Logger.log("Stan konta " + AccountNumber1 + " przed operacją: " + account2.Money);

            Logger.log("Kwota przelewu: " + value);
            if (account1.Money < value)
            {
                Logger.log("Brak wystarczających środków na koncie.");
                return 1;
            }
            else
            {
                account1.Money -= value;
                account2.Money += value;
                accountRepository.UpdateAccountInformation(account1);
                accountRepository.UpdateAccountInformation(account2);

                HistoryRepository repo = new HistoryRepository();

                var Transaction = new History
                {
                    AccountFrom = AccountNumber1,
                    AccountTo = AccountNumber2,
                    Value = value,
                    TransactionDate = DateTime.Now
                };

                repo.Add(Transaction);

                return 0;
            }
        }

        public int TransferMoneyGuid(Guid AccountGuid1, Guid AccountGuid2, double value)
        {
            // AccountRepository nie wystawia metody pozwalającej na pboranie informacji o koncie na podstawie Guid
            //pozwala na wykonanie przelewu na podstawie numeru kont
            return 1;
        }

        public List<HistoryItem> TransferHistoryForAccount(DateTime DateFrom, DateTime DateTo, String AccountNumber)
        {
            LoadHibernateCfg();

            HistoryRepository repo = new HistoryRepository();

            List<HistoryItem> transactions = new List<HistoryItem>();

            transactions = repo.GetHistoryBetweenDatesForAccount(DateFrom, DateTo, AccountNumber);
            return transactions;
        }

        public List<HistoryItem> TransferHistoryAccountGuid(DateTime DateFrom, DateTime DateTo, Guid AccountGuid)
        {
            // AccountRepository nie wystawia metody pozwalającej na pboranie informacji o koncie na podstawie Guid
            // pozwala na zwrócenie historii przelewów dla danego numeru Guid konta w podanych zakresie czasowym
            return new List<HistoryItem>();
        }
            
        
        public List<HistoryItem> TransferHistory(DateTime DateFrom, DateTime DateTo)
        {
            LoadHibernateCfg();

            HistoryRepository repo = new HistoryRepository();

            List<HistoryItem> transactions = new List<HistoryItem>();

            transactions = repo.GetHistoryBetweenDates(DateFrom, DateTo);
            return transactions;
        }
        
        public static void LoadHibernateCfg()
        {
            var cfg = new Configuration();
            cfg.Configure(@"D:\SOA_lab\CanTransferMoney\CanTransferMoney\CanTransferMoney\hibernate.cfg.xml");

            try
            {
                cfg.AddAssembly(typeof(History).Assembly);
            }
            catch (Exception e) { Console.WriteLine(e); }
            try
            {
                new SchemaExport(cfg).Execute(true, true, false);
            }
            catch (Exception e) { Console.WriteLine(e); }
        }


    }
}
