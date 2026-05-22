using Aviator.Adsb.DependencyInjection;
using Aviator.Airframe.DependencyInjection;
using Aviator.Airframe.SignalR;
using Aviator.Global.Extensions.TimeSeries;
using Aviator.Main.Components;
using Aviator.Main.Frontend;
using Aviator.Network.DependencyInjection;
using Serilog;
using Serilog.Events;

const string logFormat = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}{Scope}] {Message:lj}{NewLine}{Exception}";
var logPath = Path.Combine(Environment.CurrentDirectory, "logs");
if (!Directory.Exists(logPath))
{
    Directory.CreateDirectory(logPath);
}

Log.Logger = new LoggerConfiguration()
    //.MinimumLevel.Debug()
    .Enrich.WithDemystifiedStackTraces()
    .WriteTo.Console(outputTemplate: logFormat)
    .WriteTo.File(
        Path.Combine(logPath, "aviator.txt"),
        restrictedToMinimumLevel: LogEventLevel.Verbose,
        rollingInterval: RollingInterval.Month,
        outputTemplate: logFormat
    )
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
    builder.Services.AddRazorComponents().AddInteractiveServerComponents();
    builder.Services.AddSingleton<PlanespottersService>();
    builder.Services.Configure<FrontendConfig>(builder.Configuration.GetSection("Frontend"));

    builder.AddAviatorInfluxDb();
    builder.AddQuestDb();
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
    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapHub<AirframeHub>("/hub/acars");
    app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

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