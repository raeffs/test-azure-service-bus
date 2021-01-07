using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Receiver
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
            client.RegisterMessageHandler((message, token) =>
            {
                var content = Encoding.UTF8.GetString(message.Body);
                Console.WriteLine($"Received {content}");
                return Task.CompletedTask;
            }, args =>
            {
                return Task.CompletedTask;
            });

            Console.ReadLine();
            await client.CloseAsync();
        }
    }
}
