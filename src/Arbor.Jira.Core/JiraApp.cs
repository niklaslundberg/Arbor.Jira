using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arbor.Jira.Core;

public class JiraApp
{
    public JiraService Service => ServiceProvider.GetRequiredService<JiraService>();
    public RepositoryService RepositoryService => ServiceProvider.GetRequiredService<RepositoryService>();

    public IServiceProvider ServiceProvider { get; set; } = null!;

    public static Task<JiraApp> CreateAsync()
    {
        IServiceCollection services = new ServiceCollection().AddDomain();
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        IConfigurationRoot configurationRoot = ConfigurationHelper.CreateConfiguration();

        var jiraConfiguration = serviceProvider.GetRequiredService<JiraConfiguration>();
        IConfigurationSection jiraSection = configurationRoot.GetSection("jira");
        jiraSection.Bind(jiraConfiguration);

        var gitConfiguration = serviceProvider.GetRequiredService<GitConfiguration>();
        IConfigurationSection gitSection = configurationRoot.GetSection("git");
        gitSection.Bind(gitConfiguration);

        var app = serviceProvider.GetRequiredService<JiraApp>();

        app.ServiceProvider = serviceProvider;

        return Task.FromResult(app);
    }
}