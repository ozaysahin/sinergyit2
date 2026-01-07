using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(AppContext.BaseDirectory, "logs", "serviceb.txt"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("ServiceB başlatılıyo");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    builder.Services.AddControllers();

    builder.Services.AddSingleton(new sinergyit.ServiceB.API.ApiHelper.RabbitMQLogger("serviceb"));

    var app = builder.Build();
    app.MapControllers();

    Log.Information("ServiceB başlatıldı");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ServiceB başlatılamadı");
}
finally
{
    Log.CloseAndFlush();
}