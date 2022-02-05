using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Wellbeing.API;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Wellbeing.API;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddHttpClient();
    }
}