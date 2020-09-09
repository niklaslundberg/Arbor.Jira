using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
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

        public MainWindow()
        {
            var viewModel = CreateViewModel();
            DataContext = viewModel;

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

        private static void NavigateUrl(Uri uri) => Process.Start(new ProcessStartInfo("cmd", $"/c start {uri}") { CreateNoWindow = true });

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e) => await GetData();

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

        private ViewModel CreateViewModel() => new ViewModel();

        private bool _isLoadingData;

        private async Task GetData()
        {
            if (_app is null)
            {
                return;
            }

            if (_isLoadingData)
            {
                return;
            }

            MessageTextBox.Text = "Loading data...";

            _isLoadingData = true;

            bool? open = OpenFilterCheckBox.IsChecked;
            var result = await _app.Service.GetIssues();

            if (result.Exception is { })
            {
                MessageTextBox.Text = result.Exception.ToString();
                _isLoadingData = false;
                return;
            }

            MessageTextBox.Text = $"Found {result.IssueList.Length} Jira issues";

            if (DataContext is ViewModel viewModel)
            {
                viewModel.Issues.Clear();

                foreach (JiraIssue jiraIssue in result.IssueList)
                {
                    var jiraTaskStatus = jiraIssue.Fields.Status;

                    if (jiraTaskStatus is null)
                    {
                        continue;
                    }

                    if (open == true && jiraTaskStatus.Name.Equals(
                            "done",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    viewModel.Issues.Add(jiraIssue);
                }

                if (viewModel.Issues.Any())
                {
                    IssuesGrid.SelectedItem = viewModel.Issues.First();
                }
            }

            _isLoadingData = false;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e) => await Initialize();

        private async Task Initialize()
        {
            _app = await JiraApp.CreateAsync();

            await GetData();
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
            if (IssuesGrid.SelectedItem is JiraIssue issue && issue.Url is { })
            {
                Clipboard.SetText(issue.Url);
            }
        }

        private void IssuesGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ViewModel viewModel
                && sender is DataGrid grid
                && grid.SelectedItem is JiraIssue issue)
            {
                viewModel.SelectedIssue = issue;
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
    }
}