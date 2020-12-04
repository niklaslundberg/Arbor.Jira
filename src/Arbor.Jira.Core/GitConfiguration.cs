using JetBrains.Annotations;

namespace Arbor.Jira.Core
{
    [PublicAPI]
    public class GitConfiguration
    {
        public string? BasePath { get; set; }
    }
}