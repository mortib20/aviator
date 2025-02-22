using System.Net;
using System.Text.Json;
using Aviator.Acars;
using Aviator.Main.DependencyInjection;
using Microsoft.Extensions.Logging.Console;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(c => c.Listen(IPAddress.Any, 21001));

builder.Services.AddCors();
builder.Services.AddSignalR();

builder.AddNetworkUtilities();
builder.AddAcarsService();

builder.Logging.AddSimpleConsole(s =>
{
    s.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    s.ColorBehavior = LoggerColorBehavior.Enabled;
});

var app = builder.Build();

app.UseCors(s =>
{
    s.AllowAnyHeader();
    s.AllowAnyMethod();
    s.SetIsOriginAllowed(_ => true);
    s.AllowCredentials();
});

app.MapHub<AcarsHub>("/Acars");
app.MapGet("/", () => JsonSerializer.Serialize("Hello World!"));

await app.RunAsync().ConfigureAwait(false);