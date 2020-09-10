using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arbor.Jira.Core
{
    public class JiraApp
    {
        public JiraService Service => ServiceProvider.GetRequiredService<JiraService>();

        public IServiceProvider ServiceProvider { get; set; } = null!;

        public static Task<JiraApp> CreateAsync()
        {
            IServiceCollection services = new ServiceCollection().AddDomain();
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            var config = serviceProvider.GetRequiredService<JiraConfiguration>();

            IConfigurationRoot configurationRoot = ConfigurationHelper.CreateConfiguration();
            IConfigurationSection jiraSection = configurationRoot.GetSection("jira");
            jiraSection.Bind(config);

            var app = serviceProvider.GetRequiredService<JiraApp>();

            app.ServiceProvider = serviceProvider;

            return Task.FromResult(app);
        }
    }
}