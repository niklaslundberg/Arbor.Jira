using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

using Arbor.Jira.Core;

namespace Arbor.Jira.Wpf
{
    public class ViewModel
    {
        public ViewModel()
        {
            Issues = new ObservableCollection<JiraIssue>();

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var jiraIssue = new JiraIssue { Key = "JIRA-123", Fields = new TaskFields { Summary = "Summary 1" } };

                Issues.Add(jiraIssue);
            }
        }

        public ObservableCollection<JiraIssue> Issues { get; }
    }
}