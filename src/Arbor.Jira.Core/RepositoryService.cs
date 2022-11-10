using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Arbor.Jira.Core;

public class RepositoryService
{
    private readonly GitConfiguration _gitConfiguration;

    public RepositoryService(GitConfiguration gitConfiguration) => _gitConfiguration = gitConfiguration;

    public async Task<ImmutableArray<GitRepository>> GetRepositories()
    {
        if (string.IsNullOrWhiteSpace(_gitConfiguration.BasePath))
        {
            return ImmutableArray<GitRepository>.Empty;
        }

        var basePath = new DirectoryInfo(_gitConfiguration.BasePath);

        if (!basePath.Exists)
        {
            return ImmutableArray<GitRepository>.Empty;
        }

        RepositoryConfig GetConfig(string fullPath)
        {
            return _gitConfiguration.Repositories?.SingleOrDefault(config =>
                       string.Equals(config.FullPath, fullPath, StringComparison.OrdinalIgnoreCase)) ??
                   new RepositoryConfig { FullPath = fullPath };
        }

        return basePath.GetDirectories().Where(dir => dir.GetDirectories(".git").Length == 1)
            .Select(directory => new GitRepository(GetConfig(directory.FullName)))
            .OrderBy(repo => repo.SortOrder)
            .ThenBy(repo => repo.DisplayName)
            .ToImmutableArray();
    }
}