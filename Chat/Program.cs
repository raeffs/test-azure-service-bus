using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
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
                (rawMessage, token) =>
                {
                    var message = (ChatMessage)rawMessage;
                    Console.WriteLine($"{message.Username}{(message.IsControlMessage ? string.Empty : ":")} {message.Message}");
                    return Task.CompletedTask;
                },
                args =>
                {
                    return Task.CompletedTask;
                }
            );

            var helloMessage = new ChatMessage
            {
                Username = username,
                Message = "has entered the room",
                IsControlMessage = true
            };
            await topicClient.SendAsync((Message)helloMessage);

            while (true)
            {
                var message = Console.ReadLine();
                if (message.Equals("exit"))
                {
                    break;
                }

                var chatMessage = new ChatMessage
                {
                    Username = username,
                    Message = message
                };
                await topicClient.SendAsync((Message)chatMessage);
            }

            var byeMessage = new ChatMessage
            {
                Username = username,
                Message = "has left the room",
                IsControlMessage = true
            };
            await topicClient.SendAsync((Message)byeMessage);

            await topicClient.CloseAsync();
            await subscriptionClient.CloseAsync();
        }
    }
}
