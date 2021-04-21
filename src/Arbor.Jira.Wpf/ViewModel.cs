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
        private JiraIssue? _selectedIssue;
        private bool _showDetails;
        private GitRepository? _selectedRepository;
        private string _commitMessage;

        public ViewModel()
        {
            Issues = new ObservableCollection<JiraIssue>();
            AllIssues = new ObservableCollection<JiraIssue>();
            Repositories = new ObservableCollection<GitRepository>();

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var currentCulture = new CultureInfo("sv-SE");
                CultureInfo.CurrentCulture = currentCulture;
                CultureInfo.CurrentUICulture = currentCulture;

                for (int i = 0; i < 20; i++)
                {
                    var jiraIssue = new JiraIssue
                    {
                        Key = $"JIRA-1{i}",
                        Fields = new TaskFields
                        {
                            Summary = $"Summary 1{i}",
                            Status = new JiraTaskStatus {Name = "Open"},
                            Created = new DateTime(year: 2020, month: 1, day: 2, hour: 9, minute: 0, second: 1),
                            Description = "As as user I would like to make http requests"
                        },
                        Self = "http://jira.localhost"
                    };

                    Issues.Add(jiraIssue);
                    AllIssues.Add(jiraIssue);
                }

                Repositories.Add(new GitRepository("Test 1"));
                Repositories.Add(new GitRepository("Test 2"));

                CommitMessage = "Add visual bug";
            }

            Issues.CollectionChanged += IssuesOnCollectionChanged;

            SelectedIssue = Issues.FirstOrDefault();
            SelectedRepository = Repositories.FirstOrDefault();
        }

        public string CommitMessage
        {
            get => _commitMessage;
            set
            {
                _commitMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FullCommitMessage));
            }
        }

        public string FullCommitMessage => $"{(SelectedIssue?.Key + " " + CommitMessage).Trim().Trim('.')}.";

        public JiraIssue? SelectedIssue
        {
            get => _selectedIssue;
            set
            {
                _selectedIssue = value;
                OnPropertyChanged();
            }
        }
        public GitRepository? SelectedRepository
        {
            get => _selectedRepository;
            set
            {
                _selectedRepository = value;
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
        public ObservableCollection<JiraIssue> AllIssues { get; }

        public ObservableCollection<GitRepository> Repositories { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void IssuesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
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