using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .Build();

            var connectionString = configuration.GetConnectionString("ServiceBus");
            var topicName = "testtopic";

            var manager = new ManagementClient(connectionString);
            if (!await manager.TopicExistsAsync(topicName))
            {
                await manager.CreateTopicAsync(topicName);
            }

            Console.Write("Enter your username: ");
            var username = Console.ReadLine();

            var subscriptionName = $"{username}-{Guid.NewGuid()}";

            await manager.CreateSubscriptionAsync(new SubscriptionDescription(topicName, subscriptionName)
            {
                AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
            });

            var topicClient = new TopicClient(connectionString, topicName);
            var subscriptionClient = new SubscriptionClient(connectionString, topicName, subscriptionName);

            subscriptionClient.RegisterMessageHandler(
                (message, token) =>
                {
                    var content = Encoding.UTF8.GetString(message.Body);
                    Console.WriteLine($"{message.Label}: {content}");
                    return Task.CompletedTask;
                },
                args =>
                {
                    return Task.CompletedTask;
                }
            );

            var helloMessage = new Message(Encoding.UTF8.GetBytes("has entered the room"))
            {
                Label = username
            };
            await topicClient.SendAsync(helloMessage);

            while (true)
            {
                var message = Console.ReadLine();
                if (message.Equals("exit"))
                {
                    break;
                }

                var chatMessage = new Message(Encoding.UTF8.GetBytes(message))
                {
                    Label = username
                };
                await topicClient.SendAsync(chatMessage);
            }

            var byeMessage = new Message(Encoding.UTF8.GetBytes("has left the room"))
            {
                Label = username
            };
            await topicClient.SendAsync(byeMessage);

            await topicClient.CloseAsync();
            await subscriptionClient.CloseAsync();
        }
    }
}
