using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SoEx.Protection;

public static class ProtectionExtensions
{
    public static IServiceCollection AddMessageProtection(this IServiceCollection services, MessageSecurityConfig securityConfig)
    {
        services.AddSingleton(new MessageSecurity(securityConfig));
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IMessageReplayCache,InMemoryReplayCache>();
        return services;
    }
}
