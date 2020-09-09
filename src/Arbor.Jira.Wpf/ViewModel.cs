using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Arbor.Jira.Core;
using JetBrains.Annotations;

namespace Arbor.Jira.Wpf
{
    public class ViewModel : INotifyPropertyChanged
    {
        private bool _showDetails;
        private JiraIssue? _selectedIssue;

        public ViewModel()
        {
            Issues = new ObservableCollection<JiraIssue>();

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var currentCulture = new CultureInfo("sv-SE");
                CultureInfo.CurrentCulture = currentCulture;
                CultureInfo.CurrentUICulture = currentCulture;

                for (int i = 0; i < 20; i++)
                {
                    var jiraIssue = new JiraIssue
                    {
                        Key = "JIRA-1" + i,
                        Fields = new TaskFields
                        {
                            Summary = "Summary 1" + i, Status = new JiraTaskStatus {Name = "Open"},
                            Created = new DateTime(2020,1,2,9,0,1)
                        },
                        Self = "http://jira.localhost"
                    };

                    Issues.Add(jiraIssue);
                }
            }

            Issues.CollectionChanged += IssuesOnCollectionChanged;

            SelectedIssue = Issues.FirstOrDefault();
        }

        public JiraIssue? SelectedIssue
        {
            get => _selectedIssue;
            set
            {
                _selectedIssue = value;
                OnPropertyChanged();
            }
        }

        public bool ShowDetails
        {
            get
            {
                if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                {
                    return true;
                }

                return _showDetails;
            }
            set
            {
                _showDetails = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<JiraIssue> Issues { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void IssuesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) =>
            ShowDetails = Issues.Count > 0;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}