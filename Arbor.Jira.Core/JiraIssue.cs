namespace Arbor.Jira.Core
{
    public class JiraIssue
    {
        public TaskFields Fields { get; set; }

        public string FullName => $"{Key} {Fields.Summary}";

        public string GitBranch => this.GitBranchName();

        public string Key { get; set; }

        public string Self { get; set; }

        public string Url => this.GetUrl();

        public override string ToString() => FullName;
    }
}