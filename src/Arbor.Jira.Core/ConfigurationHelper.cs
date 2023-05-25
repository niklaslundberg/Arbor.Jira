using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Arbor.Jira.Core;

public static class ConfigurationHelper
{
    public static IConfigurationRoot CreateConfiguration()
    {
        var configurationBuilder = new ConfigurationBuilder();

        configurationBuilder
            .Add(new JsonConfigurationSource {Path = "appsettings.json", Optional = true})
            .Add(new JsonConfigurationSource {Path = "appsettings.user.json", Optional = true});

        return configurationBuilder.Build();
    }
}