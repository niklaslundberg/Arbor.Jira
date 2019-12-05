using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

using Arbor.Jira.Core;

namespace Arbor.Jira.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private JiraApp _app;

        public MainWindow()
        {
            DataContext = CreateViewModel();

            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void Browse(object sender, RequestNavigateEventArgs e)
        {
            if (e.Uri == null)
            {
                return;
            }

            Process.Start(new ProcessStartInfo("cmd", $"/c start {e.Uri}") { CreateNoWindow = true });
        }

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

        private async Task GetData()
        {
            bool? open = OpenFilterCheckBox.IsChecked;
            ImmutableArray<JiraIssue> immutableArray = await _app.Service.GetIssues();

            if (DataContext is ViewModel viewModel)
            {
                viewModel.Issues.Clear();

                foreach (JiraIssue jiraIssue in immutableArray)
                {
                    if (open == true && jiraIssue.Fields.Status.Name.Equals(
                            "done",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    viewModel.Issues.Add(jiraIssue);
                }
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            _app = await JiraApp.CreateAsync();

            await GetData();
        }

        private async void OpenFilterCheckBox_Toggled(object sender, RoutedEventArgs e) => await GetData();

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control
                               && IssueList.SelectedItem is JiraIssue issue)
            {
                Clipboard.SetText(issue.GitBranch);
            }
        }
    }
}