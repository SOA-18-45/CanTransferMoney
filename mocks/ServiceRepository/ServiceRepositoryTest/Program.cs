using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceRepository;

namespace ServiceRepositoryTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceRepository.ServiceRepository serviceRepository = new ServiceRepository.ServiceRepository();

            string serviceName = "test_serwis";
            string serviceAddress = "net.tcp://test.com";
            Console.WriteLine(String.Format("Próba rejestracji serwisu {0}({1})", serviceName, serviceAddress));
            serviceRepository.registerService(serviceName, serviceAddress);

            Console.WriteLine();

            Console.WriteLine("Pobranie adresu serwisu {0}", serviceName);
            string address = serviceRepository.getServiceAddress(serviceName);
            Console.WriteLine(String.Format("address == {0} ? {1}", serviceAddress, serviceAddress == address));
            Console.WriteLine();

            Console.WriteLine("Pobranie adresu nieistniejącego serwisu");
            try
            {
                address = serviceRepository.getServiceAddress("nieistniejacy");
            }
            catch (Exception e)
            {
                Console.WriteLine("ServiceRepository zgłosił wyjątek, żądany serwis nie istnieje");
            }
            Console.WriteLine();

            Console.WriteLine("Wyrejestrowanie się serwisu {0}", serviceName);
            serviceRepository.unregisterService(serviceName);
            Console.WriteLine("Pobranie adresu serwisu {0}", serviceName);
            try
            {
                address = serviceRepository.getServiceAddress(serviceName);
            }
            catch (Exception e)
            {
                Console.WriteLine("ServiceRepository zgłosił wyjątek, żądany serwis nie istnieje");
            }

            Console.ReadLine();

        }
    }
}
