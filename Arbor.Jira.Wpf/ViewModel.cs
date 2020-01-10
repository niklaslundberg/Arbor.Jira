using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
                for (int i = 0; i < 20; i++)
                {
                    var jiraIssue = new JiraIssue { Key = "JIRA-1" + i, Fields = new TaskFields { Summary = "Summary 1" + i } };

                    Issues.Add(jiraIssue);
                }
            }
        }

        public ObservableCollection<JiraIssue> Issues { get; }

        public JiraIssue FirstOrDefault => Issues.FirstOrDefault();
    }
}