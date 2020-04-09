using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

using Arbor.Jira.Core;
using Arbor.Jira.Wpf.Annotations;

namespace Arbor.Jira.Wpf
{
    public class ViewModel : INotifyPropertyChanged
    {
        private bool _showDetails;

        public bool ShowDetails
        {
            get => _showDetails;

            set
            {
                _showDetails = value;
                OnPropertyChanged();
            }
        }

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

            Issues.CollectionChanged += IssuesOnCollectionChanged;
        }

        private void IssuesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ShowDetails = Issues.Count > 0;
        }

        public ObservableCollection<JiraIssue> Issues { get; }

        public JiraIssue FirstOrDefault => Issues.FirstOrDefault();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}