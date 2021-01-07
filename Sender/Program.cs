using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sender
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .Build();

            var connectionString = configuration.GetConnectionString("ServiceBus");

            var client = new QueueClient(connectionString, "testqueue");

            for (var i = 0; i < 10; i++)
            {
                var payload = $"Message {i}";
                var message = new Message(Encoding.UTF8.GetBytes(payload));
                await client.SendAsync(message);
                Console.WriteLine($"Sent {payload}");
            }

            Console.ReadLine();
            await client.CloseAsync();
        }
    }
}
