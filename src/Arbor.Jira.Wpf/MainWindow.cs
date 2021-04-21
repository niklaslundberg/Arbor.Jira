using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using Arbor.Jira.Core;

namespace Arbor.Jira.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private JiraApp? _app;

        private bool _isLoadingRepositories;
        private bool _isLoadingIssues;

        public MainWindow()
        {
            DataContext = CreateViewModel();

            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void Browse(object sender, RequestNavigateEventArgs e)
        {
            if (e.Uri is { })
            {
                NavigateUrl(e.Uri);
            }
        }

        private static void NavigateUrl(Uri uri) =>
            Process.Start(new ProcessStartInfo("cmd", $"/c start {uri}") {CreateNoWindow = true});

        private async void OpenIssues_OnClick(object sender, RoutedEventArgs e) => await GetData();

        private void CopyFullName_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FullNameBlock.Text))
            {
                Clipboard.SetText(FullNameBlock.Text);
            }
        }

        private void CopyGitBranch_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(GitBranchBlock.Text))
            {
                Clipboard.SetText(GitBranchBlock.Text);
            }
        }

        private static ViewModel CreateViewModel() => new ();

        private async Task GetRepositories()
        {
            if (_isLoadingRepositories)
            {
                return;
            }

            _isLoadingRepositories = true;

            if (DataContext is ViewModel viewModel && _app is {})
            {
                var gitRepositories = await _app.RepositoryService.GetRepositories().ConfigureAwait(false);

                Dispatcher.Invoke(() =>
                {
                    viewModel.Repositories.Clear();

                    foreach (var repository in gitRepositories)
                    {
                        viewModel.Repositories.Add(repository);
                    }
                });
            }

            _isLoadingRepositories = false;
        }

        private async Task GetData()
        {
            if (_app is null)
            {
                return;
            }

            if (_isLoadingIssues)
            {
                return;
            }

            MessageTextBox.Text = "Loading data...";

            _isLoadingIssues = true;

            bool open = OpenFilterCheckBox.IsChecked ?? true;
            var result = await _app.Service.GetIssues(open).ConfigureAwait(false);

            Dispatcher.Invoke(() =>
                {
                    if (result.Exception is { })
                    {
                        MessageTextBox.Text = result.Exception.ToString();
                        _isLoadingIssues = false;

                        return;
                    }

                    MessageTextBox.Text = $"Found {result.IssueList.Length} Jira issues";

                    if (DataContext is ViewModel viewModel)
                    {
                        viewModel.Issues.Clear();
                        viewModel.AllIssues.Clear();

                        foreach (JiraIssue jiraIssue in result.IssueList.OrderByDescending(issue => issue.SortOrder))
                        {
                            var jiraTaskStatus = jiraIssue.Fields.Status;

                            if (jiraTaskStatus is null)
                            {
                                continue;
                            }

                            if (open && jiraTaskStatus.Name.Equals(
                                "done",
                                StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            viewModel.Issues.Add(jiraIssue);
                            viewModel.AllIssues.Add(jiraIssue);
                        }

                        Filter();

                        if (viewModel.Issues.Any())
                        {
                            IssuesGrid.SelectedItem = viewModel.Issues.First();
                        }
                    }

                    _isLoadingIssues = false;
                });
        }

        private async void OnLoaded(object sender, RoutedEventArgs e) => await Initialize();

        private async Task Initialize()
        {
            _app = await JiraApp.CreateAsync();

            var repositoriesTask = GetRepositories();

            var dataTask = GetData();

            await Task.WhenAll(dataTask, repositoriesTask);
        }

        private async void OpenFilterCheckBox_Toggled(object sender, RoutedEventArgs e) => await GetData();

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control
                               && IssuesGrid.SelectedItem is JiraIssue issue
                               && !string.IsNullOrWhiteSpace(issue.GitBranch))
            {
                Clipboard.SetText(issue.GitBranch);
            }
        }

        private void ListViewScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scv)
            {
                scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private void CopyLink_Click(object sender, RoutedEventArgs e)
        {
            if (IssuesGrid.SelectedItem is JiraIssue {Url: { }} issue)
            {
                Clipboard.SetText(issue.Url);
            }
        }

        private void IssuesGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ViewModel viewModel
                && sender is DataGrid {SelectedItem: JiraIssue issue})
            {
                viewModel.SelectedIssue = issue;
                viewModel.CommitMessage = "";

                CommitTextBox.Focus();
            }
        }

        private void OnHyperlinkClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Hyperlink hyperlink
                && hyperlink.NavigateUri is {})
            {
                NavigateUrl(hyperlink.NavigateUri);
            }
        }

        private void FilterTextBox_OnTextChanged(object sender, TextChangedEventArgs e) => Filter();

        private void Filter()
        {
            if (DataContext is not ViewModel viewModel)
            {
                return;
            }

            if (string.IsNullOrEmpty(FilterTextBox.Text))
            {
                ResetFilter();
                return;
            }

            var filtered = viewModel.AllIssues
                .Where(
                    issue =>
                    {
                        var stringComparison = CaseSensitiveCheckBox.IsChecked == true
                            ? StringComparison.Ordinal
                            : StringComparison.OrdinalIgnoreCase;

                        return issue.Match(FilterTextBox.Text, stringComparison);
                    })
                .ToArray();

            viewModel.Issues.Clear();

            foreach (var jiraIssue in filtered)
            {
                viewModel.Issues.Add(jiraIssue);
            }
        }

        private void ResetFilter()
        {
            FilterTextBox.Text = "";

            if (DataContext is not ViewModel viewModel)
            {
                return;
            }

            if (viewModel.Issues.Count == viewModel.AllIssues.Count)
            {
                return;
            }

            viewModel.Issues.Clear();

            foreach (var jiraIssue in viewModel.AllIssues)
            {
                viewModel.Issues.Add(jiraIssue);
            }
        }

        private void Clear_OnClick(object sender, RoutedEventArgs e) => ResetFilter();

        private void FilterCaseSensitive_Click(object sender, RoutedEventArgs e) => Filter();

        private void CommitBlock_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is not ViewModel viewModel)
            {
                return;
            }

            viewModel.CommitMessage = CommitTextBox.Text;
        }

        private void CopyCommitMessage_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(CommitFullText.Text))
            {
                Clipboard.SetText(CommitFullText.Text);
            }
        }
    }
}