using System;

namespace Arbor.Jira.Core;

public class GitBranch : IEquatable<GitBranch>
{
    public static readonly GitBranch Develop = new("develop");
    public static readonly GitBranch Main = new("main");
    public static readonly GitBranch Master = new("master");

    public bool Equals(GitBranch? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((GitBranch) obj);
    }

    public override int GetHashCode() => Name.GetHashCode();

    public static bool operator ==(GitBranch? left, GitBranch? right) => Equals(left, right);

    public static bool operator !=(GitBranch? left, GitBranch? right) => !Equals(left, right);

    public GitBranch(string name, string? message = default)
    {
        Name = name;
        Message = message;
    }

    public string Name { get; }

    public string? Message { get; }
    public override string ToString() => $"{nameof(Name)}: {Name}";

    public static GitBranch Unknown(string result) => new("Unknown", result);
}