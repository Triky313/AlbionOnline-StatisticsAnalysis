using Microsoft.Extensions.DependencyInjection;
using System;

namespace StatisticsAnalysisTool.Common;

public static class ServiceRegistration
{
    public static IServiceProvider RegisterServices()
    {
        var services = new ServiceCollection();

        //services.AddTransient<ICraftingResources, CraftingResources>();
        //services.AddTransient<ICraftingJournal, CraftingJournal>();
        //services.AddTransient<ICraftingBaseValues, CraftingBaseValues>();
        //services.AddTransient<ICraftingCalculations, CraftingCalculations>();

        return services.BuildServiceProvider();
    }
}