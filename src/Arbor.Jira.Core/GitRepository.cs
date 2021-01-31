namespace Arbor.Jira.Core
{
    public class GitRepository
    {
        public string DisplayName { get; }

        public GitRepository(string displayName)
        {
            DisplayName = displayName;
        }
    }
}