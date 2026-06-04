using Core.API.Services.Digikala;

namespace Core.API.Services.Background;

public class DigikalaTokenWorker
    : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<
        DigikalaTokenWorker> _logger;

    public DigikalaTokenWorker(
        IServiceProvider provider,
        ILogger<
            DigikalaTokenWorker
        > logger
    )
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task
        ExecuteAsync(
            CancellationToken stoppingToken
        )
    {
        while (!stoppingToken
               .IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _provider.CreateScope();

                var auth =
                    scope.ServiceProvider
                        .GetRequiredService
                        <DigikalaAuthService>();

                await auth.RefreshTokenAsync();

                _logger.LogInformation(
                    "Digikala token refreshed"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Token refresh failed"
                );
            }

            await Task.Delay(
                TimeSpan.FromMinutes(55),
                stoppingToken
            );
        }
    }
}