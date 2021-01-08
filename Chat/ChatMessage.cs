using Microsoft.Azure.ServiceBus;
using System.Text;
using System.Text.Json;

namespace Chat
{
    public record ChatMessage
    {
        public string Username { get; init; }

        public string Message { get; init; }

        public bool IsControlMessage { get; init; }

        public static explicit operator Message(ChatMessage source) => new Message
        {
            Body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(source))
        };

        public static explicit operator ChatMessage(Message source) => JsonSerializer.Deserialize<ChatMessage>(Encoding.UTF8.GetString(source.Body));
    }
}
