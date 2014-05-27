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
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single)]
    public class ServiceRepository : IServiceRepository
    {
        Dictionary<string, string> services = new Dictionary<string, string>();
        bool isAliveActive = true;

        public void registerService(string serviceName, string serviceAddress)
        {
            services.Add(serviceName, serviceAddress);
            Console.WriteLine("Dodano serwis: {0} {1}", serviceName, serviceAddress);

            Console.WriteLine("Aktualna lista serwisów:");
            foreach (KeyValuePair<string, string> service in services)
            {
                Console.WriteLine("Key: {0}, Value: {1}",
                service.Key, service.Value);
            }
        }

        public void disableIsAlive()
        {
            isAliveActive = false;
        }

        public void enableIsAlive()
        {
            isAliveActive = true;
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
            if (isAliveActive)
            {
                Console.WriteLine(serviceName + " zgłasza obecność.");
            }
            else
            {
                throw new Exception("isAlive nie odpowiada");
            }
        }
    }    
}
