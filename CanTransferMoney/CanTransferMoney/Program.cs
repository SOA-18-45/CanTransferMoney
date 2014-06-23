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
using log4net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using ZMQ;

namespace CanTransferMoney
{
    class Program
    {
        static private bool _stop = false;
        static private object _locker = new object();
        static private Thread _workerThread;
        static System.Timers.Timer registerServiceTimer;
        static IServiceRepository serviceRepository;
        static CanTransferMoney canTransferMoney;
        static ServiceHost sh;
        static ServiceHost shHttp;

        static void Main(string[] args)
        {
            Logger.log("Program started");
            if (Config.getStorage() == "database")
            {
                CanTransferMoney.LoadHibernateCfg();
                Logger.log("Hibernate conf loaded");
            }
            // creation of transferMoney object
            canTransferMoney = new CanTransferMoney();

            //create service host of transferMoney object
            configureService();            
            sh.Open();
            Logger.log("Service host opened");
            

            if (Config.getMode() == "prod")
            {
                //connect to IServiceRepository service
                connectToServiceRepository();

                //register service in IServiceRepository service
                registerService();
                
               

            }

            if (Config.getMode() == "dev")
            {
                /*
                IAccountRepository accountRepository;
                string IAccountRepositoryAddress = Config.getAccountRepositoryURI();

                string AccountNumber1 = "1111";
                Console.WriteLine(IAccountRepositoryAddress);

                NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);

                binding.MaxBufferSize = Config.MaxBufferSize;
                binding.MaxBufferPoolSize = Config.MaxBufferPoolSize;
                binding.MaxReceivedMessageSize = Config.MaxReceivedMessageSize;
                binding.ReceiveTimeout = Config.ReceiveTimeout;
                binding.SendTimeout = Config.SendTimeout;

                ChannelFactory<IAccountRepository> cf2 = new ChannelFactory<IAccountRepository>(new NetTcpBinding(SecurityMode.None), IAccountRepositoryAddress);
                accountRepository = cf2.CreateChannel();
                AccountDetails account1 = new AccountDetails();
                account1 = accountRepository.GetAccountInformation(AccountNumber1);

                Console.WriteLine(account1.Money);
                
                 */
            }

            Start();
            
            Console.ReadLine();
        }

        static public void Start()
        {
            _workerThread = new Thread(RunZeroMqServer);
            _workerThread.Start();
        }

        //metoda Stop zatrzymuje watek sluchajacy na message ZeroMQ
        static public void Stop()
        {
            lock (_locker)
            {
                _stop = true;
            }

            _workerThread.Join();
        }

        static void registerService(object sender = null, System.Timers.ElapsedEventArgs e = null)
        {
            try
            {
                Logger.log("Próba rejestracji");


                serviceRepository.registerService(Config.getServiceName(), Config.getCanTransferMoneyURI(), "net.tcp");
                serviceRepository.registerService(Config.getServiceName(), Config.getCanTransferMoneyHttpURI(), "http");

                
                Logger.log("Serwis zarejestrowano");

                /*   
                IAccountRepository accountRepository;
                string IAccountRepositoryAddress = Config.getAccountRepositoryURI();

                string AccountNumber1 = "1111";
                Console.WriteLine(IAccountRepositoryAddress);

                NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);

                binding.MaxBufferSize = Config.MaxBufferSize;
                binding.MaxBufferPoolSize = Config.MaxBufferPoolSize;
                binding.MaxReceivedMessageSize = Config.MaxReceivedMessageSize;
                binding.ReceiveTimeout = Config.ReceiveTimeout;
                binding.SendTimeout = Config.SendTimeout;

                ChannelFactory<IAccountRepository> cf2 = new ChannelFactory<IAccountRepository>(new NetTcpBinding(SecurityMode.None), IAccountRepositoryAddress);
                accountRepository = cf2.CreateChannel();
                AccountDetails account1 = new AccountDetails();
                account1 = accountRepository.GetAccountInformation(AccountNumber1);

                Console.WriteLine(account1.Money);
                */

                sendAlive();
            }
            catch (System.Exception ex)
            {
                Logger.log("Nie powiodła się rejestracja w serwisie, następna próba za 5 sekund...");
                connectToServiceRepository();
                registerServiceTimer = new System.Timers.Timer();
                registerServiceTimer.AutoReset = false;
                registerServiceTimer.Elapsed += new System.Timers.ElapsedEventHandler(registerService);
                registerServiceTimer.Interval = Config.getTimeout();
                registerServiceTimer.Start();
            }

        }

        static void connectToServiceRepository()
        {
            string Uri = Config.getServiceRepositoryURI();
            ChannelFactory<IServiceRepository> cf = new ChannelFactory<IServiceRepository>(new NetTcpBinding(SecurityMode.None), Uri);
            serviceRepository = cf.CreateChannel();

        }

        static void configureService()
        {
            string Uri = Config.getCanTransferMoneyURI();
            sh = new ServiceHost(canTransferMoney, new Uri[] { new Uri(Uri) });
            ServiceMetadataBehavior metadata = sh.Description.Behaviors.Find<ServiceMetadataBehavior>();

            if (metadata == null)
            {
                metadata = new ServiceMetadataBehavior();
                sh.Description.Behaviors.Add(metadata);
            }

            metadata.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;

            // create endpoint of transferMoney service
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            sh.AddServiceEndpoint(typeof(ICanTransferMoney), binding, Uri);

            string HttpUri = Config.getCanTransferMoneyHttpURI();
            shHttp = new ServiceHost(canTransferMoney, new Uri[] { new Uri(HttpUri) });

            ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            shHttp.Description.Behaviors.Add(smb);
            


        }

        static private void RunZeroMqServer()
        {
            // ZMQ Context, server socket
            using (var context = new Context())
            using (var server = context.Socket(SocketType.REP))
            {
                var bindingAddress = new StringBuilder("tcp://");
                bindingAddress.Append("127.0.0.1");
                bindingAddress.Append(":");
                bindingAddress.Append("11901");

                server.Bind(bindingAddress.ToString());

                while (!_stop)
                {
                    string message = server.Recv(Encoding.Unicode, 100);
                    if (message == null)
                    {
                        continue;//nic nie odebral przez 100 ms, wiec nic nie przetwarzamy
                    }
                    var response = ProcessMessage(message);

                    server.Send(response, Encoding.Unicode);

                    Thread.Sleep(100);
                }
            }
        }

        static private string ProcessMessage(string message)
        {
            var TransferMoneyMessage = JsonConvert.DeserializeObject<ICanTransferMoneyMessage>(message); //deserializacja wiadomosci ogolenej dla AccountRepository Service
            if (TransferMoneyMessage.Action != "TransferMoney")//Sprawdzenie czy akcja to CreateAccount - jesli nie to rzuca exception bo innych nie obsluguje ten kod
            {
                throw new InvalidOperationException(string.Format("Cannot process unknown action {0}",
                    TransferMoneyMessage.Action));
            }

            if (TransferMoneyMessage.Action != "TransferMoney")
            {//Sprawdzenie czy akcja to CreateAccount - jesli nie to rzuca exception bo innych nie obsluguje ten kod

                var createAccountMessage =
                    JsonConvert.DeserializeObject<TransferMoneyMessage>(TransferMoneyMessage.Arguments); //deserialiazacja argumentow CreateAccountMessage
                var response = canTransferMoney.TransferMoney(createAccountMessage.AccountNumber1, createAccountMessage.AccountNumber2, createAccountMessage.value); //wywowalnie metody serwisu ktora wczesniej byla dostepna przez WebService.
                return response.ToString();
            }

            
            return "";

        }


        static void sendAlive(object sender = null, System.Timers.ElapsedEventArgs e = null)
        {
            try
            {
                Logger.log("Wysłanie sygnału I-AM-ALIVE");
                serviceRepository.isAlive(Config.getServiceName());
                
                registerServiceTimer = new System.Timers.Timer();
                registerServiceTimer.AutoReset = false;
                registerServiceTimer.Elapsed += new System.Timers.ElapsedEventHandler(sendAlive);
                registerServiceTimer.Interval = Config.getTimeout();
                registerServiceTimer.Start();
            }
            catch (System.Exception ex)
            {
                Logger.log("Nie powiodło się wysłanie I-AM-ALIVE");
                registerService();
            }
        }

        //metoda start tworzy nowy watek na ktorym sluchamy na nowe message pojawiajace sie na naszej kolejce ZeroMQ
        
    
    }
    
    public static class Logger
    {
        private static readonly log4net.ILog Logger4net = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public static void log(string message)
        {
            
         //   System.IO.StreamWriter file = new System.IO.StreamWriter("log.txt", true);
        //    file.WriteLine(message);
         //   file.Close();
            Console.WriteLine(message);
            Logger4net.Debug(message);
        }
    }

    public class ICanTransferMoneyMessage
    {
        public string Action { get; set; }
        public string Arguments { get; set; }
    }

    public class TransferMoneyMessage
    {
        public string AccountNumber1 { get; set; }
        public string AccountNumber2 { get; set; }
        public double value { get; set; }
    }


    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CanTransferMoney : ICanTransferMoney
    {
        IServiceRepository serviceRepository;
        IAccountRepository accountRepository;
        List<HistoryItem> historyList;

        public CanTransferMoney()
        {
            if (Config.getStorage() == "memory")
            {
                historyList = new List<HistoryItem>();
            }
            ChannelFactory<IServiceRepository> cf = new ChannelFactory<IServiceRepository>(new NetTcpBinding(SecurityMode.None), Config.ServiceRepositoryURI);
            serviceRepository = cf.CreateChannel();
        }


        public int TransferMoney(string AccountNumber1, string AccountNumber2, double value)
        {
            //pozwala na wykonanie przelewu na podstawie numeru kont


            string IAccountRepositoryAddress = "";

            if (Config.getMode() == "dev")
            {
                IAccountRepositoryAddress = Config.getAccountRepositoryURI();
            }
            
            if (Config.getMode() == "prod") {
                IAccountRepositoryAddress = serviceRepository.getServiceAddress("IAccountRepository", "net.tcp");
            }

            ChannelFactory<IAccountRepository> cf = new ChannelFactory<IAccountRepository>(new NetTcpBinding(SecurityMode.None), IAccountRepositoryAddress);
            accountRepository = cf.CreateChannel();

            AccountDetails account1 = accountRepository.GetAccountInformation(AccountNumber1);
            Logger.log("Stan konta " + AccountNumber1 + " przed operacją: " + account1.Money);

            AccountDetails account2 = accountRepository.GetAccountInformation(AccountNumber2);
            Logger.log("Stan konta " + AccountNumber2 + " przed operacją: " + account2.Money);

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


                if (Config.getStorage() == "database")
                {
                    HistoryRepository repo = new HistoryRepository();
                    var Transaction = new History
                    {
                        AccountFrom = AccountNumber1,
                        AccountTo = AccountNumber2,
                        Value = value,
                        TransactionDate = DateTime.Now
                    };

                    repo.Add(Transaction);
                }

                if (Config.getStorage() == "memory")
                {
                    historyList.Add(new HistoryItem
                    {
                        AccountFrom = AccountNumber1,
                        AccountTo = AccountNumber2,
                        Value = value,
                        TransactionDate = DateTime.Now
                    });
                }

                return 0;
            }
        }

        public int TransferMoneyGuid(Guid AccountGuid1, Guid AccountGuid2, double value)
        {
            // AccountRepository nie wystawia metody pozwalającej na pboranie informacji o koncie na podstawie Guid
            //pozwala na wykonanie przelewu na podstawie numeru kont
            return 1;
        }

        public List<HistoryItem> TransferHistoryForAccountBeetweenDates(DateTime DateFrom, DateTime DateTo, String AccountNumber)
        {

            HistoryRepository repo = new HistoryRepository();

            List<HistoryItem> transactions = new List<HistoryItem>();

            transactions = repo.GetHistoryBetweenDatesForAccount(DateFrom, DateTo, AccountNumber);
            return transactions;
        }

        public List<HistoryItem> TransferHistoryForAccount(String AccountNumber)
        {
            List<HistoryItem> transactions = new List<HistoryItem>();

            if (Config.getStorage() == "database")
            {
                HistoryRepository repo = new HistoryRepository();
                transactions = repo.GetHistoryByAccountNumber(AccountNumber);
            }

            if (Config.getStorage() == "memory")
            {
                for (int i = 0; i < historyList.Count; i++)
                {
                    if (historyList.ElementAt(i).AccountFrom.Equals(AccountNumber) || historyList.ElementAt(i).AccountTo.Equals(AccountNumber))
                    {
                        transactions.Add(historyList.ElementAt(i));
                    }
                }
            }


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
            HistoryRepository repo = new HistoryRepository();

            List<HistoryItem> transactions = new List<HistoryItem>();

            transactions = repo.GetHistoryBetweenDates(DateFrom, DateTo);
            return transactions;
        }
        
        public static void LoadHibernateCfg()
        {
            var cfg = new Configuration();
            cfg.Configure(Config.getHibernatePath());

            try
            {
                cfg.AddAssembly(typeof(History).Assembly);
            }
            catch (System.Exception e) { Console.WriteLine(e); }
            try
            {
                new SchemaExport(cfg).Execute(true, true, false);
            }
            catch (System.Exception e) { Console.WriteLine(e); }
        }

   

    }
}
