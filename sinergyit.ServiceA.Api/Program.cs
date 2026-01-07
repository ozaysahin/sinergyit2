using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(AppContext.BaseDirectory, "logs", "servicea.txt"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("ServiceA başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    builder.Services.AddControllers();

    builder.Services.AddSingleton(new sinergyit.ServiceA.API.ApiHelper.RabbitMQLogger("servicea"));

    var app = builder.Build();
    app.MapControllers();

    Log.Information("ServiceA başlatıldı!");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ServiceA başlatılamadı!");
}
finally
{
    Log.CloseAndFlush();
}