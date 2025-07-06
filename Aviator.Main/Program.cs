using System.Text.Json;
using Aviator.Adsb.DependencyInjection;
using Aviator.Airframe.DependencyInjection;
using Aviator.Airframe.SignalR;
using Aviator.Global.DependencyInjection;
using Aviator.Network.DependencyInjection;
using Serilog;

const string logFormat = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}{Scope}] {Message:lj}{NewLine}{Exception}";
var logPath = Path.Combine(Environment.CurrentDirectory, "logs");
if (!Directory.Exists(logPath))
{
    Directory.CreateDirectory(logPath);
}

var configuration = new ConfigurationBuilder()
    .SetBasePath(Environment.CurrentDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog();

    builder.WebHost.UseKestrel(k =>
    {
        k.ListenAnyIP(21001);
        k.AllowResponseHeaderCompression = true;
    });

    builder.Services.AddResponseCompression();
    builder.Services.AddCors();
    builder.Services.AddSignalR();

    builder.AddAviatorInfluxDb();
    builder.AddNetworkUtilities();
    
    builder.AddAirframeExtension();
    builder.AddAdsbService();

    var app = builder.Build();

    app.UseCors(s =>
    {
        s.AllowAnyHeader();
        s.AllowAnyMethod();
        s.SetIsOriginAllowed(_ => true);
        s.AllowCredentials();
    });

    app.UseResponseCompression();

    app.MapHub<AirframeHub>("/Acars");
    app.MapGet("/", () => JsonSerializer.Serialize("Hello World!"));

    await app.RunAsync().ConfigureAwait(false);
}
catch (Exception e)
{
    Log.Fatal(e, "Aviator terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}
