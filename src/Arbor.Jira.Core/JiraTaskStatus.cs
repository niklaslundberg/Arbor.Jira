﻿namespace Arbor.Jira.Core
{
    public class JiraTaskStatus
    {
        public string Name { get; set; } = "";

        public override string ToString() => Name;
    }
}