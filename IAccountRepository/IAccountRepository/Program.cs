using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.ServiceModel.Description;

namespace IAccountRepository
{
    class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            AccountRepository accountRepository = new AccountRepository();
            //ServiceRepository serviceRepository = new ServiceRepository();
            //accountRepository.DisplayAccountDetails(1);
            ServiceHost sh = new ServiceHost(accountRepository, new Uri[] { new Uri("net.tcp://localhost:11905/IAccountRepository") });

            ServiceMetadataBehavior metadata = sh.Description.Behaviors.Find<ServiceMetadataBehavior>();

            if (metadata == null)
            {
                metadata = new ServiceMetadataBehavior();
                sh.Description.Behaviors.Add(metadata);
            }

            metadata.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;

            sh.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexTcpBinding(), "mex");

            NetTcpBinding accountRepositoryBinding = new NetTcpBinding(SecurityMode.None);
            accountRepositoryBinding.MaxBufferSize = 10000000;
            accountRepositoryBinding.MaxBufferPoolSize = 10000000;
            accountRepositoryBinding.MaxReceivedMessageSize = 10000000;
            sh.AddServiceEndpoint(typeof(IAccountRepository), accountRepositoryBinding, "net.tcp://localhost:11905/IAccountRepository");
            sh.Open();

            ChannelFactory<IServiceRepository> cf = new ChannelFactory<IServiceRepository>(new NetTcpBinding(SecurityMode.None), "net.tcp://localhost:11900/IServiceRepository");
            IServiceRepository serviceRepository = cf.CreateChannel();

            serviceRepository.registerService("IAccountRepository", "net.tcp://localhost:11905/IAccountRepository");


            AccountDetails nowy = new AccountDetails(new Guid(), "123", 0);
            nowy = accountRepository.GetAccountInformation("123");
            Console.WriteLine("account 123 value" +  nowy.Money);

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

        [OperationContract]
        int test();
       
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AccountRepository : IAccountRepository
    {
        List<AccountDetails> accountList = new List<AccountDetails>();

        public int test()
        {
            return 1000;
        }
        public string CreateAccount(Guid clientId, AccountDetails details)
        {
            return "lala";
        }

        public AccountDetails GetAccountInformation(string accountNumber)
        {
            Console.WriteLine("Got AccountInformation request for " + accountNumber);
            int i = 0;
            for (i = 0; i < accountList.Count; i++)
            {
                if (accountList.ElementAt(i).AccountNumber.Equals(accountNumber))
                {
                    Console.WriteLine("Return element at " + i + " with value " + accountList.ElementAt(i).Money);
                    AccountDetails returnAccountDetails = new AccountDetails(new Guid(), "00", 0);
                    returnAccountDetails = accountList.ElementAt(i);
                    return accountList.ElementAt(i);
                    
                }
            }

            return new AccountDetails(new Guid(), "-1", -1);

        }

        public AccountRepository()
        {
            accountList.Add(new AccountDetails(new System.Guid(), "123", 453.25));
            accountList.Add(new AccountDetails(new System.Guid(), "234", 453.25));
            accountList.Add(new AccountDetails(new System.Guid(), "3234", 453.25));
            accountList.Add(new AccountDetails(new System.Guid(), "4234", 453.25));
            accountList.Add(new AccountDetails(new System.Guid(), "5234", 453.25));
            accountList.Add(new AccountDetails(new System.Guid(), "6234", 453.25));
        }

        public void UpdateAccountInformation(AccountDetails details)
        {
            Console.WriteLine("Got UpdateAccountInformation request for " + details.AccountNumber);
            for (int i = 0; i < accountList.Count; i++)
            {
                if (accountList.ElementAt(i).AccountNumber.Equals(details.AccountNumber))
                {
                    double oldMoney = accountList.ElementAt(i).Money;
                    accountList.ElementAt(i).Money = details.Money;
                    Console.WriteLine("Updated " + accountList.ElementAt(i).AccountNumber + " old value " + oldMoney + " present value " + accountList.ElementAt(i).Money);
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

   

   


   


