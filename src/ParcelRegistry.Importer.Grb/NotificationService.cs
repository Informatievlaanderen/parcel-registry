namespace ParcelRegistry.Importer.Grb
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Amazon.SimpleNotificationService;
    using Amazon.SimpleNotificationService.Model;

    public interface INotificationService
    {
        public Task PublishToTopicAsync(NotificationMessage message);
    }

    public class NotificationService : INotificationService, IDisposable
    {
        private readonly IAmazonSimpleNotificationService _amazonSimpleNotificationService;
        private readonly string _topicArn;

        public NotificationService(IAmazonSimpleNotificationService amazonSimpleNotificationService,string topicArn)
        {
            _amazonSimpleNotificationService = amazonSimpleNotificationService;
            _topicArn = topicArn;
        }

        public async Task PublishToTopicAsync(NotificationMessage message)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var request = new PublishRequest
            {
                TopicArn = _topicArn,
                Message = JsonSerializer.Serialize(message, options),
                MessageAttributes =
                {
                    { "MessageType", new MessageAttributeValue { DataType = "String", StringValue = message.MessageType } },
                    { "service", new MessageAttributeValue { DataType = "String", StringValue = message.Service } },
                    { "warning", new MessageAttributeValue { DataType = "String", StringValue = message.Warning } }
                }
            };

            await _amazonSimpleNotificationService.PublishAsync(request);
        }

        public void Dispose()
        {
            _amazonSimpleNotificationService.Dispose();
        }
    }
}
