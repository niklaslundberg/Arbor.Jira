using System;

namespace Arbor.Jira.Core;

public class JiraIssueData
{
    public JiraIssue[] Issues { get; set; } = Array.Empty<JiraIssue>();
}