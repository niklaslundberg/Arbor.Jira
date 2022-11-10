namespace Arbor.Jira.Core;

public class RepositoryConfig
{
    public int? SortOrder { get; init; }
    public string? FullPath { get; init; }
    public string? DisplayName { get; init; }
    public string? DefaultGitBranch { get; init; }
}