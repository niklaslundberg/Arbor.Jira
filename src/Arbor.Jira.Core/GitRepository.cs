using System;
using System.IO;

namespace Arbor.Jira.Core
{
    public class GitRepository
    {
        public string DisplayName { get; }
        public string FullPath { get; }
        public GitBranch DefaultGitBranch { get; }
        public int SortOrder { get; }

        public GitRepository(RepositoryConfig repositoryConfig, string? displayName = null)
        {
            if (string.IsNullOrWhiteSpace(repositoryConfig.FullPath))
            {
                throw new ArgumentException(nameof(FullPath));
            }

            DisplayName = displayName ?? repositoryConfig.DisplayName ?? new DirectoryInfo(repositoryConfig.FullPath).Name;
            FullPath = repositoryConfig.FullPath;

            SortOrder = repositoryConfig.SortOrder ?? int.MaxValue;
            DefaultGitBranch = string.IsNullOrWhiteSpace(repositoryConfig.DefaultGitBranch) ? GitBranch.Develop : new GitBranch(repositoryConfig.DefaultGitBranch);
        }
    }
}