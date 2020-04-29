using Microsoft.Extensions.DependencyInjection;

namespace Arbor.Jira.Core
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.AddSingleton<JiraService>();
            services.AddHttpClient();
            services.AddSingleton<JiraConfiguration>();
            services.AddSingleton<JiraApp>();

            return services;
        }
    }
}