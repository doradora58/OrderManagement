
using Microsoft.AspNetCore.Connections;
using OrderManagement.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace OrderManagement.Messaging
{
    public class RabbitMqConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<RabbitMqConsumer> logger) : BackgroundService
    {
        private static readonly ActivitySource _activitySource = new("OrderManagement.Messaging");

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // appsettings.json または環境変数 (RabbitMQ__HostName) からホスト名を取得（デフォルトは "rabbitmq"）
            var hostname = configuration["RabbitMQ:HostName"] ?? "rabbitmq";
            var factory = new ConnectionFactory { HostName = hostname };

            logger.LogInformation("Attempting to connect to RabbitMQ at host: {Hostname}", hostname);

            // 接続リトライのロジック（起動直後のRabbitMQの立ち上がり待ち対策）
            IConnection? connection = null;
            IChannel? channel = null;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    connection = await factory.CreateConnectionAsync(stoppingToken);
                    channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
                    logger.LogInformation("Successfully connected to RabbitMQ.");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to connect to RabbitMQ. Retrying in 5 seconds...");
                    await Task.Delay(5000, stoppingToken);
                }
            }

            if (connection == null || channel == null)
            {
                return;
            }

            using (connection)
            using (channel)
            {
                // キューの宣言（FMS側と名前を一致させる）
                await channel.QueueDeclareAsync(
                    queue: "job-status-queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: stoppingToken
                );

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    // 1. W3C トレースコンテキスト（traceparent）の抽出
                    ActivityContext extractedContext = default;
                    if (ea.BasicProperties.Headers != null &&
                        ea.BasicProperties.Headers.TryGetValue("traceparent", out var traceParentObj) &&
                        traceParentObj is byte[] traceParentBytes)
                    {
                        var traceParentStr = Encoding.UTF8.GetString(traceParentBytes);
                        if (ActivityContext.TryParse(traceParentStr, null, out var parsedContext))
                        {
                            extractedContext = parsedContext;
                        }
                    }

                    // 2. コンシューマ側のスパンを開始し、FMSからのトレースを結合
                    using var activity = _activitySource.StartActivity(
                        "RabbitMQ.Consume",
                        ActivityKind.Consumer,
                        extractedContext
                    );

                    activity?.SetTag("messaging.system", "rabbitmq");
                    activity?.SetTag("messaging.destination.name", "job-status-queue");

                    try
                    {
                        var body = ea.Body.ToArray();
                        var messageJson = Encoding.UTF8.GetString(body);

                        // 3. DIコンテナからスコープを作成してハンドラーを解決し、処理を実行
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<JobStatusEvent>>();

                            var jobEvent = JsonSerializer.Deserialize<JobStatusEvent>(messageJson);
                            if (jobEvent != null)
                            {
                                await handler.HandleAsync(jobEvent, ea.BasicProperties.Headers);
                                logger.LogInformation("Successfully processed JobId: {JobId}", jobEvent.JobId);
                            }
                            else
                            {
                                logger.LogWarning("Failed to deserialize message into JobStatusEvent.");
                            }
                        }

                        // 4. 処理成功の確認応答 (Ack)
                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing RabbitMQ message.");

                        // 5. 処理失敗時の確認応答 (Nack - 再キューイングしない)
                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                    }
                };

                // キューの購読開始
                await channel.BasicConsumeAsync(
                    queue: "job-status-queue",
                    autoAck: false, // 手動Ackにするため false
                    consumer: consumer,
                    cancellationToken: stoppingToken
                );

                // アプリケーションが終了するまでバックグラウンドで待機
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }
    }
}
