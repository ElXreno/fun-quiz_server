namespace API.Data;

public class DataSeederService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    public DataSeederService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();

        await seeder.SeedData();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}