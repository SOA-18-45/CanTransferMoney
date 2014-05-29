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
    class Program
    {
        static void Main(string[] args)
        {
            AccountRepository accountRepository = new AccountRepository();
            ServiceHost sh = configureServiceHost(accountRepository, Config.AccountRepositoryURI);
            
            sh.Open();
            Console.WriteLine("Serwis uruchomiony...");
            Console.ReadLine();            
        }

        static ServiceHost configureServiceHost(Object repository, string uri)
        {
            ServiceHost sh = new ServiceHost(repository, new Uri[] { new Uri(uri) });

            ServiceMetadataBehavior metadata = sh.Description.Behaviors.Find<ServiceMetadataBehavior>();

            if (metadata == null)
            {
                metadata = new ServiceMetadataBehavior();
                sh.Description.Behaviors.Add(metadata);
            }

            metadata.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            sh.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexTcpBinding(), "mex");

            NetTcpBinding accountRepositoryBinding = new NetTcpBinding(SecurityMode.None);
            sh.AddServiceEndpoint(typeof(IAccountRepository), accountRepositoryBinding, Config.AccountRepositoryURI);
        
            return sh;
        }   
    }
}

   

   


   


