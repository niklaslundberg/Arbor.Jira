using System;

namespace Arbor.Jira.Core
{
    public static class NameHelper
    {
        public static string GetUrl(this JiraIssue issue)
        {
            if (string.IsNullOrWhiteSpace(issue.Self))
            {
                return string.Empty;
            }

            var uri = new UriBuilder(issue.Self) { Path = $"/browse/{issue.Key}", Query = string.Empty, Fragment = string.Empty };

            return uri.ToString();
        }

        public static string GitBranchName(this JiraIssue issue)
        {
            string trimmedName = issue.Fields.Summary
                .Replace(" ", "-")
                .Replace(":", "-")
                .Replace(".", string.Empty)
                .Replace("\"", string.Empty)
                .Replace("\\", string.Empty)
                .Replace("/", string.Empty)
                .Replace("(", string.Empty)
                .Replace(")", string.Empty)
                .ToLower();

            while (trimmedName.Contains("--"))
            {
                trimmedName = trimmedName.Replace("--", "-");
            }

            return $"{issue.Key}-{trimmedName}";
        }
    }
}