using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace Sentinel.Worker;

public class Worker : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<Worker> _logger;
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;
    
    // Basit bir iç durum (State) takibi
    private int _consecutiveFailures = 0;

    public Worker(IHttpClientFactory httpClientFactory, ILogger<Worker> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        // Polly v8 Gelişmiş Resilience Pipeline (Timeout + Retry + Circuit Breaker)
        _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            // Her bir istek için 2 saniye zaman aşımı sınırı
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(2)
            })
            // 2 kez tekrar deneme
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(500)
            })
            // Circuit Breaker
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(10),
                MinimumThroughput = 3,
                BreakDuration = TimeSpan.FromSeconds(10)
            })
            .Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(3000, stoppingToken);

            var client = _httpClientFactory.CreateClient();
            string targetUrl = "http://dummy-service:8080/health-check";
            string status = "Healthy";
            string latencyStr = "0ms";

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var response = await _pipeline.ExecuteAsync(
                    async token => await client.GetAsync(targetUrl, token), 
                    stoppingToken
                );
                sw.Stop();

                if (response.IsSuccessStatusCode)
                {
                    status = "Healthy";
                    latencyStr = $"{sw.ElapsedMilliseconds}ms";
                    _consecutiveFailures = 0; // Başarılı olursa sayacı sıfırla
                }
                else
                {
                    status = "Unhealthy (Error)";
                    latencyStr = $"{sw.ElapsedMilliseconds}ms";
                    _consecutiveFailures++;
                }
            }
            catch (BrokenCircuitException)
            {
                status = "Unhealthy (Circuit Open)";
                latencyStr = "N/A";
                _consecutiveFailures++;
            }
            catch (TimeoutRejectedException)
            {
                status = "Unhealthy (Timeout)";
                latencyStr = ">2000ms";
                _consecutiveFailures++;
            }
            catch (Exception)
            {
                status = "Unhealthy (Down)";
                latencyStr = "Timeout";
                _consecutiveFailures++;
            }

            // Sonucu Merkezi API'ye bildir (Arka arkaya hata sayısını da ekleyerek)
            try
            {
                await client.PostAsJsonAsync("http://sentinel-api:8080/api/report", new
                {
                    ServiceName = "Dummy-Microservice-X",
                    Status = status,
                    Latency = latencyStr,
                    ConsecutiveFailures = _consecutiveFailures,
                    Timestamp = DateTime.Now.ToString("HH:mm:ss")
                }, stoppingToken);

                _logger.LogInformation("Rapor gönderildi: {Status} ({Latency}) - Art arda hata: {Failures}", status, latencyStr, _consecutiveFailures);
            }
            catch (Exception ex)
            {
                _logger.LogError("Merkezi API'ye rapor gönderilemedi: {Message}", ex.Message);
            }
        }
    }
}