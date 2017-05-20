using Consul;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var urls = new List<Uri>();

            var consulClient = new ConsulClient(c => c.Address = new Uri("http://127.0.0.1:8500"));
            var services = consulClient.Agent.Services().Result.Response;

            foreach (var service in services)
            {
                if(service.Value.Tags.Any(t => t == "Values")) {
                    var serviceUri = new Uri($"{service.Value.Address}:{service.Value.Port}");
                    urls.Add(serviceUri);
                }
            }

            Console.WriteLine("Available services:");
            urls.ForEach(Console.WriteLine);

            // from here we can re-try requests in case of failing and use other available urls
            
            var url = urls.FirstOrDefault();

            if(url != null)
            {
                var serviceClient = new HttpClient();
                serviceClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = serviceClient.GetAsync(new Uri(url, "api/values/10")).Result;
                var content = response.Content.ReadAsStringAsync().Result;

                Console.WriteLine($"Service response is: {content}");
            }
        }
    }
}