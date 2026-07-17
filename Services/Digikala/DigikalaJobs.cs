namespace Core.API.Services.Digikala;
using System.Text.Json;
using Core.API.Models;
public class DigikalaJobs
{
    private readonly DigikalaAuthService _auth;
    private readonly DigikalaSyncService _sync;
    private readonly ILogger<DigikalaJobs> _logger;

    public DigikalaJobs(
        DigikalaAuthService auth,
        ILogger<DigikalaJobs> logger,
        DigikalaSyncService sync)
    {
        _auth = auth;
        _logger = logger;
        _sync = sync;
    }

    public async Task RefreshTokenKrabo()
    {
        try
        {
            await _auth.RefreshTokenAsync(1);

            _logger.LogInformation("Digikala krabo-token refreshed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "krabo-Token refresh failed");
            throw;
        }
    }
    public async Task RefreshTokenFereshte()
    {
        try
        {
            await _auth.RefreshTokenAsync(2);

            _logger.LogInformation("Digikala fereshte-token refreshed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "fereshte-Token refresh failed");
            throw;
        }
    }


    public async Task AddInvoicesKrabo()
    {
        try
        {
            await _sync.InvoiceSync(1);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Add invoices failed");
            throw;
        }
    }


    public async Task AddPackagesKrabo()
    {
        try
        {
            await _sync.PackagesSync(1);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Add PACKAGES failed");
            throw;
        }
    }

    public async Task AddInvoicesFereshte()
    {
        try
        {
            await _sync.InvoiceSync(2);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Add invoices failed");
            throw;
        }
    }


    public async Task AddPackagesFereshte()
    {
        try
        {
            await _sync.PackagesSync(2);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Add PACKAGES failed");
            throw;
        }
    }
}