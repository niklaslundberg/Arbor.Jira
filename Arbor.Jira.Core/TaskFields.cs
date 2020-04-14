using System;

namespace Arbor.Jira.Core
{
    public class TaskFields
    {
        public DateTime Created { get; set; }

        public JiraTaskStatus? Status { get; set; }

        public string Summary { get; set; } = "";
    }
}