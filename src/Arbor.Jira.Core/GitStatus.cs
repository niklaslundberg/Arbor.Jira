namespace Arbor.Jira.Core;

public class GitStatus
{
    public override string ToString() => string.IsNullOrWhiteSpace(Message) ? "Clean" : $"{nameof(Message)}: {Message}";

    public string Message { get; }

    public GitStatus(string message) => Message = message;

    public static GitStatus WithMessage(string message)
    {
        return new GitStatus(message);
    }

    public static readonly GitStatus Ok = new("");
}