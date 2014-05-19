using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.ServiceModel.Description;

namespace AccountRepository
{
    class Program
    {
        private const int MaxBufferSize = 10000000;
        private const int MaxBufferPoolSize = 10000000;
        private const int MaxReceivedMessageSize = 10000000;
        private const string ServiceRepositoryURI = "net.tcp://localhost:11900/IServiceRepository";
        private const string AccountRepositoryURI = "net.tcp://localhost:11905/IAccountRepository";

        static void Main(string[] args)
        {
            AccountRepository accountRepository = new AccountRepository();
            ServiceHost sh = new ServiceHost(accountRepository, new Uri[] { new Uri(AccountRepositoryURI) });

            ServiceMetadataBehavior metadata = sh.Description.Behaviors.Find<ServiceMetadataBehavior>();

            if (metadata == null)
            {
                metadata = new ServiceMetadataBehavior();
                sh.Description.Behaviors.Add(metadata);
            }

            metadata.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            sh.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexTcpBinding(), "mex");

            NetTcpBinding accountRepositoryBinding = new NetTcpBinding(SecurityMode.None);
            accountRepositoryBinding.MaxBufferSize = MaxBufferSize;
            accountRepositoryBinding.MaxBufferPoolSize = MaxBufferPoolSize;
            accountRepositoryBinding.MaxReceivedMessageSize = MaxReceivedMessageSize;
            sh.AddServiceEndpoint(typeof(IAccountRepository), accountRepositoryBinding, AccountRepositoryURI);


            sh.Open();
            Console.WriteLine("Serwis uruchomiony...");

            Console.WriteLine("Próba zarejestrowania serwisu w ServiceRepository...");
            ChannelFactory<IServiceRepository> cf = new ChannelFactory<IServiceRepository>(new NetTcpBinding(SecurityMode.None), ServiceRepositoryURI);
            IServiceRepository serviceRepository = cf.CreateChannel();

            serviceRepository.registerService("AccountRepository", AccountRepositoryURI);
            Console.WriteLine("Serwis zarejestrowany w ServiceRepository.");

            Console.ReadLine();            
        }
        
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

        public AccountDetails(Guid id, string number, double money)
        {
            this.Id = id;
            this.AccountNumber = number;
            this.Money = money;
        }
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
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AccountRepository : IAccountRepository
    {
        List<AccountDetails> accountList = new List<AccountDetails>();

        public AccountRepository()
        {
            accountList.Add(new AccountDetails(new System.Guid(), "123", 453.25));
            accountList.Add(new AccountDetails(new System.Guid(), "234", 453.25));
            accountList.Add(new AccountDetails(new System.Guid(), "3234", 453.25));
            accountList.Add(new AccountDetails(new System.Guid(), "4234", 453.25));
            accountList.Add(new AccountDetails(new System.Guid(), "5234", 453.25));
            accountList.Add(new AccountDetails(new System.Guid(), "6234", 453.25));
        }

        public string CreateAccount(Guid clientId, AccountDetails details)
        {
            return "Konto utworzone.";
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
            return new AccountDetails(new Guid(), "-1", -1);
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
}

   

   


   


