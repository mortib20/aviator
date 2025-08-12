using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestDB.Utils;

namespace Aviator.Global.DependencyInjection;

public static class QuestDbExtension
{
    public static WebApplicationBuilder AddQuestDb(this WebApplicationBuilder builder)
    {
        var options = builder.Configuration.GetSection("Metrics").GetSection("QuestDB").Get<SenderOptions>();

        ArgumentNullException.ThrowIfNull(options);
        
        builder.Services.AddSingleton(options);
        
        return builder;
    }
}