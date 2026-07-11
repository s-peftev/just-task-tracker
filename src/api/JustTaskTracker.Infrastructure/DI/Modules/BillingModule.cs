using JustTaskTracker.Infrastructure.Common.Options;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace JustTaskTracker.Infrastructure.DI.Modules;

internal static class BillingModule
{
    public static IServiceCollection AddBillingModule(this IServiceCollection services)
    {
        services.AddSingleton<IStripeClient>(sp =>
        { 
            var stripeOptions = sp.GetRequiredService<StripeOptions>();

            return new StripeClient(stripeOptions.SecretKey);
        });

        return services;
    }
}
