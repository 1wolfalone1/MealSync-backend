using Confluent.Kafka;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Chat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MealSync.Infrastructure.Services;

public class KafkaOpenChatRoomService : BaseService, IChatService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaOpenChatRoomService> _logger;

    public KafkaOpenChatRoomService(IConfiguration configuration, ILogger<KafkaOpenChatRoomService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task OpenOrCloseRoom(AddChat addChat)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = _configuration["KAFKA_BOOSTRAP_SERVER"]
        };

        var messageValue = JsonConvert.SerializeObject(addChat);
        using (var producer = new ProducerBuilder<string, string>(config).Build())
        {
            try
            {
                // Send the message asynchronously
                var result = await producer.ProduceAsync("joinRoom", new Message<string, string>
                {
                    Key = "JoinRoom",
                    Value = messageValue,
                });

                // Log the result
                _logger.LogInformation($"Message sent to partition {result.Partition} with offset {result.Offset} to open room chat");
            }
            catch (ProduceException<string, string> e)
            {
                _logger.LogError($"Error: {e.Error.Reason}");
                Console.WriteLine($"Error: {e.Error.Reason}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
    }
}