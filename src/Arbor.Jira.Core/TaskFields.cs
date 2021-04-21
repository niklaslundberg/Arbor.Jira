using System;

namespace Arbor.Jira.Core
{
    public class TaskFields
    {
        public DateTime Created { get; set; }

        public JiraTaskStatus? Status { get; set; }

        public string Summary { get; set; } = "";

        public string Description { get; set; } = "";

        public string ShortDescription
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Description))
                {
                    return "";
                }

                return (Description.Length > 30
                    ? Description.Substring(0, 30) + "..."
                    : Description).Trim();
            }
        }
    }
}