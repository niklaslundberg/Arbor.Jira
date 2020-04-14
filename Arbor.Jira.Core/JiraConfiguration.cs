using JetBrains.Annotations;

namespace Arbor.Jira.Core
{
    [PublicAPI]
    public class JiraConfiguration
    {
        public string? Password { get; set; }

        public string? Url { get; set; }

        public string? Username { get; set; }
    }
}