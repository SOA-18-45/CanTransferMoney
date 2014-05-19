using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Web;
using System.Timers;

namespace CanTransferMoney
{
    class Config
    {
        public const string ConfigSrc = @"..\..\..\..\config.txt";
        public const string ServiceRepositoryURI = @"net.tcp://localhost:11900/IServiceRepository";
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
            ServiceHost sh = new ServiceHost(transferMoney, new Uri[] { new Uri("net.tcp://localhost:11910/ICanTrasferMoney") });

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
            Logger.log(serviceParameters[5]);
            ChannelFactory<IServiceRepository> cf = new ChannelFactory<IServiceRepository>(new NetTcpBinding(SecurityMode.None), Config.ServiceRepositoryURI);
            IServiceRepository serviceRepository = cf.CreateChannel();
            
            //register service in IServiceRepository service
            serviceRepository.registerService(serviceParameters[1], serviceParameters[3]);
            Logger.log("Zarejestrowano w ServiceRepository");

            //enable imAlive method every 5 seconds
            var timer = new System.Threading.Timer(e => imAlive(serviceRepository, serviceParameters), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));


            //transferMoney.TransferMoney("123", "234", 150.00);
            //transferMoney.TransferMoney("234", "123", 123.49);
            Console.ReadLine();
        }

        private static void imAlive(IServiceRepository serviceRepository, string[] serviceParameters)
        {
            serviceRepository.isAlive(serviceParameters[1]);
            Logger.log("Wysyłanie sygnału IamAlive");
        }
    }

    [ServiceContract]
    public interface ICanTransferMoney
    {
        [OperationContract]
        int TransferMoney(string AccountNumber1, string AccountNumber2, double value); //pozwala na wykonanie przelewu na podstawie numeru kont
        
        [OperationContract]
        int TransferMoneyGuid(Guid AccountGuid1, Guid AccountGuid2, decimal value); // pozwala na wykonanie przelewu na podstawie numeru Guid kont

        [OperationContract]
        int TransferHistoryAccountString(DateTime DateFrom, DateTime DateTo, String AccountNumber); // pozwala na zwrócenie historii przelewów dla danego numeru konta w podanych zakresie czasowym

        [OperationContract]
        int TransferHistoryAccountGuid(DateTime DateFrom, DateTime DateTo, Guid AccountGuid); // pozwala na zwrócenie historii przelewów dla danego numeru Guid konta w podanych zakresie czasowym

        [OperationContract]
        int TransferHistory(DateTime DateFrom, DateTime DateTo); // pozwala na zwrócenie historii przelewów dla WSZYSTKICH kont w podanym zakresie czasowym
        
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
        public static void log(string Message)
        {
            Console.WriteLine(Message);        
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
                historyList.Add(new historyItem(DateTime.Now, AccountNumber1, AccountNumber2, value));
                return 0;
            }
        }

        public int TransferMoneyGuid(Guid AccountGuid1, Guid AccountGuid2, decimal value)
        {
            // pozwala na wykonanie przelewu na podstawie numeru Guid kont
            return 1;
        }

        public int TransferHistoryAccountString(DateTime DateFrom, DateTime DateTo, String AccountNumber)
        {
            // pozwala na zwrócenie historii przelewów dla danego numeru konta w podanych zakresie czasowym
            return 1;
        }

        public int TransferHistoryAccountGuid(DateTime DateFrom, DateTime DateTo, Guid AccountGuid)
        {
            // pozwala na zwrócenie historii przelewów dla danego numeru Guid konta w podanych zakresie czasowym
            return 1;
        }
            
        
        public int TransferHistory(DateTime DateFrom, DateTime DateTo)
        {
            // pozwala na zwrócenie historii przelewów dla WSZYSTKICH kont w podanych zakresie czasowym
            return 1;   
        }   
    }

    [ServiceContract]
    public interface IServiceRepository
    {
        [OperationContract]
        void registerService(string serviceName, string serviceAddress);
        [OperationContract]
        void unregisterService(string serviceName);
        [OperationContract]
        string getServiceAddress(string serviceName);
        [OperationContract]
        void isAlive(string serviceName);
    }

    [ServiceContract]
    public interface IAccountRepository
    {
        [OperationContract]
        string CreateAccount(Guid clientId, AccountDetails details);
        [OperationContract]
        AccountDetails GetAccountInformation(string accountNumber);
        [OperationContract]
        void UpdateAccountInformation(AccountDetails details);
        [OperationContract]
        int test();
       
    }

    [DataContract(Namespace = "wybraneslowo")]
    public class AccountDetails
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public Guid ClientId { get; set; }
        [DataMember]
        public string AccountNumber { get; set; }
        [DataMember]
        public double Money { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public double Percentage { get; set; }
        [DataMember]
        public DateTime EndDate { get; set; }
        [DataMember]
        public DateTime StartDate { get; set; }
    }
}
