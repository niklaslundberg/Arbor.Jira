using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Arbor.Jira.Core
{
    public class RepositoryService
    {
        private readonly GitConfiguration _gitConfiguration;

        public RepositoryService(GitConfiguration gitConfiguration)
        {
            _gitConfiguration = gitConfiguration;
        }

        public async Task<ImmutableArray<GitRepository>> GetRepositories()
        {
            return new List<GitRepository>()
            {
                new GitRepository("Test 1"),
                new GitRepository("Test 2"),
            }.ToImmutableArray();
        }
    }
}