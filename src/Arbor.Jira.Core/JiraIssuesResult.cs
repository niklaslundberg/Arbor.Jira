using System;
using System.Collections.Immutable;

namespace Arbor.Jira.Core;

public class JiraIssuesResult
{
    private JiraIssuesResult(ImmutableArray<JiraIssue> issues, Exception? exception = default)
    {
        IssueList = issues;
        Exception = exception;
    }

    public ImmutableArray<JiraIssue> IssueList { get; }

    public Exception? Exception { get; }

    public static JiraIssuesResult Issues(ImmutableArray<JiraIssue> issues) => new JiraIssuesResult(issues);

    public static JiraIssuesResult Error(Exception ex) => new JiraIssuesResult(ImmutableArray<JiraIssue>.Empty, ex);
}