using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.ServiceModel.Description;

namespace BankService
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceRepository serviceRepository = new ServiceRepository();
           
            ServiceHost sh = new ServiceHost(serviceRepository, new Uri[] {new Uri("net.tcp://localhost:11900/IServiceRepository")});

            ServiceMetadataBehavior metadata = sh.Description.Behaviors.Find<ServiceMetadataBehavior>();
           
            if (metadata == null)
            {
                metadata = new ServiceMetadataBehavior();
                sh.Description.Behaviors.Add(metadata);
            }

            metadata.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;

            sh.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexTcpBinding(), "mex");

            NetTcpBinding serviceRepositoryBinding = new NetTcpBinding(SecurityMode.None);
            serviceRepositoryBinding.MaxBufferSize = 10000000;
            serviceRepositoryBinding.MaxBufferPoolSize = 10000000;
            serviceRepositoryBinding.MaxReceivedMessageSize = 10000000;
           // serviceRepositoryBinding.ReceiveTimeout = 100000000;
           // serviceRepositoryBinding.SendTimeout = new System.TimeSpan(1, 0, 0);
            sh.AddServiceEndpoint(typeof(IServiceRepository), serviceRepositoryBinding, "net.tcp://localhost:11900/IServiceRepository");
            sh.Open();

            Console.ReadLine();
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

    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single)]
    public class ServiceRepository : IServiceRepository
    {
        Dictionary<string, string> services = new Dictionary<string, string>();

        public void registerService(string serviceName, string serviceAddress)
        {
            services.Add(serviceName, serviceAddress);
            Console.WriteLine("Dodałem takie cuś: {0} {1}", serviceName, serviceAddress);
            
            foreach (KeyValuePair<string, string> service in services)
            {
                Console.WriteLine("Key: {0}, Value: {1}",
                service.Key, service.Value);
            }


        }

        public void unregisterService(string serviceName)
        {
            services.Remove(serviceName);
        }

        public string getServiceAddress(string serviceName)
        {
            return services[serviceName];
        }

        public void isAlive(string serviceName)
        {
            Console.WriteLine(serviceName + " is alive");
        }

    }
       
    [DataContract(Namespace="wybraneslowo")]
    public class AccountInfo
    {
        [DataMember]
        public string ServiceName { get; set; }
        [DataMember]
        public string ServiceAddress { get; set; }
       
    }
}

