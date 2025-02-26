using System.Text.Json;
using Aviator.Acars;
using Aviator.Main.DependencyInjection;
using Serilog;
using Serilog.Events;

const string logFormat = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}";
var logPath = Path.Combine(Environment.CurrentDirectory, "logs");
if (!Directory.Exists(logPath))
{
    Directory.CreateDirectory(logPath);
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: logFormat)
    .WriteTo.File(Path.Combine(logPath, "aviator-log.txt"), rollingInterval: RollingInterval.Month, outputTemplate: logFormat)
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
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

    builder.AddNetworkUtilities();
    builder.AddAcarsService();

    var app = builder.Build();

    app.UseCors(s =>
    {
        s.AllowAnyHeader();
        s.AllowAnyMethod();
        s.SetIsOriginAllowed(_ => true);
        s.AllowCredentials();
    });

    app.UseResponseCompression();
    
    app.MapHub<AcarsHub>("/Acars");
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