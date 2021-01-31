using Microsoft.Extensions.DependencyInjection;

namespace Arbor.Jira.Core
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.AddSingleton<JiraService>();
            services.AddSingleton<RepositoryService>();
            services.AddHttpClient();
            services.AddSingleton<JiraConfiguration>();
            services.AddSingleton<GitConfiguration>();
            services.AddSingleton<JiraApp>();

            return services;
        }
    }
}