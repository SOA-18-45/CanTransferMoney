using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using Contracts;

namespace ServiceRepository
{
    class Program
    {

        static void Main(string[] args)
        {
            ServiceRepository serviceRepository = new ServiceRepository();
            ServiceHost sh = configureServiceHost(serviceRepository, Config.ServiceURI);

            int state = 0;
            int input = 0;
            bool breakLoop = false;

            while(true) {
                Console.WriteLine("Co chcesz zrobic?");
                if (state == 0) Console.WriteLine("(1) Uruchom serwis");
                if (state == 1 || state == 3) Console.WriteLine("(2) Zatrzymaj serwis");
                if (state == 1) Console.WriteLine("(3) Wyłącz obsługę isAlive");
                if (state == 3) Console.WriteLine("(4) Włącz obsługę isAlive");
                Console.WriteLine("(0) Zakończ");

                input = Convert.ToInt32(Console.ReadLine());
                switch (input)
                {
                    case 1:
                        sh = configureServiceHost(serviceRepository, Config.ServiceURI);
                        sh.Open();
                        Console.WriteLine("Serwis uruchomiony.");
                        state = input;
                        break;
                    case 2:
                        sh.Close();
                        Console.WriteLine("Serwis zatrzymany");
                        state = 0;
                        break;
                    case 3:
                        serviceRepository.disableIsAlive();
                        Console.WriteLine("isAlive wyłączony");
                        state = input;
                        break;
                    case 4:
                        serviceRepository.enableIsAlive();
                        Console.WriteLine("isAlive włłączony");
                        state = 1;
                        break;
                    case 0:
                        breakLoop = true;
                        break;
                    default:
                        Console.WriteLine("Nie rozpoznano polecenia");
                    break;
                }
                Console.WriteLine();

                if (breakLoop) break;
            };
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

            NetTcpBinding serviceRepositoryBinding = new NetTcpBinding(SecurityMode.None);
            serviceRepositoryBinding.MaxBufferSize = Config.MaxBufferSize;
            serviceRepositoryBinding.MaxBufferPoolSize = Config.MaxBufferPoolSize;
            serviceRepositoryBinding.MaxReceivedMessageSize = Config.MaxReceivedMessageSize;

            sh.AddServiceEndpoint(typeof(IServiceRepository), serviceRepositoryBinding, uri);

            return sh;
        }
    }
}

