﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using Arbor.Jira.Core;
using CliWrap;
using Newtonsoft.Json;

namespace Arbor.Jira.Wpf;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private const string GitExePath = @"C:\Program Files\Git\bin\git.exe";
    private JiraApp? _app;
    private bool _isLoadingIssues;

    private bool _isLoadingRepositories;
    private readonly string _cacheFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Arbor.Jira", "cache.json");

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
        Process.Start(new ProcessStartInfo("cmd", $"/c start {uri}") { CreateNoWindow = true });

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

    private static ViewModel CreateViewModel() => new();

    private Task GetRepositories()
    {
        if (_isLoadingRepositories)
        {
            return Task.CompletedTask;
        }

        _isLoadingRepositories = true;

        if (DataContext is ViewModel viewModel && _app is { })
        {
            var gitRepositories = _app.RepositoryService.GetRepositories();

            Dispatcher.Invoke(() =>
            {
                viewModel.Repositories.Clear();

                foreach (var repository in gitRepositories)
                {
                    viewModel.Repositories.Add(repository);
                }

                if (viewModel.Repositories.Any())
                {
                    RepositoriesComboBox.SelectedItem = viewModel.Repositories.First();
                }
            });
        }

        _isLoadingRepositories = false;
        return Task.CompletedTask;
    }

    private async Task<JiraIssuesResult?> GetCachedData()
    {
        if (File.Exists(_cacheFile))
        {
            try
            {
                string json = await File.ReadAllTextAsync(_cacheFile);

                if (json.Length == 0)
                {
                    return null;
                }

                var jiraIssuesResult = JsonConvert.DeserializeObject<JiraIssue[]>(json);

                if (jiraIssuesResult is { Length: > 0 })
                {
                    return JiraIssuesResult.Issues(jiraIssuesResult.ToImmutableArray());
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        return null;
    }

    private async Task GetData()
    {
        if (_app is null)
        {
            return;
        }

        if (DataContext is not ViewModel viewModel)
        {
            return;
        }

        viewModel.StatusMessage = "Loading data...";

        bool open = OpenFilterCheckBox.IsChecked ?? true;

        var dataSourceTask = _app.Service.GetIssues(open).ConfigureAwait(false);

        if (viewModel.AllIssues.Count == 0)
        {
            var data = await GetCachedData();

            if (data is { })
            {
                Dispatcher.Invoke(() => InitializeViewModel(data, viewModel, open, "cache"));
            }
        }

        if (_isLoadingIssues)
        {
            return;
        }

        _isLoadingIssues = true;

        JiraIssuesResult result = await dataSourceTask;

        try
        {
            if (result.IssueList.Length > 0)
            {
                var cacheFileInfo = new FileInfo(_cacheFile);

                if (cacheFileInfo.Directory?.Exists != true)
                {
                    cacheFileInfo.Directory!.Create();
                }

                await File.WriteAllTextAsync(_cacheFile, JsonConvert.SerializeObject(result.IssueList));
            }
        }
        catch (Exception)
        {
            // ignore
        }

        Dispatcher.Invoke(() =>
        {
            InitializeViewModel(result, viewModel, open, "server");
        });
    }

    private void InitializeViewModel(JiraIssuesResult result, ViewModel viewModel, bool open, string source)
    {
        if (DataContext is not ViewModel asViewModel)
        {
            return;
        }

        if (result.Exception is { })
        {
            asViewModel.StatusMessage = result.Exception.ToString();
            _isLoadingIssues = false;

            return;
        }

        var currentSelected = viewModel.SelectedIssue;

        asViewModel.StatusMessage = $"Found {result.IssueList.Length} Jira issues from {source}";

        viewModel.Issues.Clear();
        viewModel.AllIssues.Clear();

        foreach (var jiraIssue in result.IssueList.OrderByDescending(issue => issue.SortOrder))
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

        if (viewModel.Issues.Any() && (currentSelected is null || !viewModel.Issues.Any(issue => issue.IssueNumber == currentSelected.IssueNumber)))
        {
            var selected = viewModel.Issues.First();
            viewModel.CustomBranchName = selected.GitBranch;
            BranchNameCustom.Text = viewModel.CustomBranchName ?? "";
            IssuesGrid.SelectedItem = selected;
        }
        else if (currentSelected is { } && viewModel.Issues.SingleOrDefault(issue => issue.IssueNumber == currentSelected.IssueNumber) is { } newSelected)
        {
            viewModel.SelectedIssue = newSelected;
        }

        if (viewModel.Issues.Any())
        {
            BranchNameCustom.Text = viewModel.CustomBranchName ?? "";
        }

        _isLoadingIssues = false;
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
        if (e.Key == Key.C
            && Keyboard.Modifiers == ModifierKeys.Control
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
        if (IssuesGrid.SelectedItem is JiraIssue { Url: { } } issue)
        {
            Clipboard.SetText(issue.Url);
        }
    }

    private void IssuesGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ViewModel viewModel
            && sender is DataGrid { SelectedItem: JiraIssue issue })
        {
            viewModel.SelectedIssue = issue;
            viewModel.CommitMessage = "";

            CommitTextBox.Focus();

            viewModel.CustomBranchName = issue.GitBranch;
        }
    }

    private void OnHyperlinkClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is Hyperlink { NavigateUri: { } } hyperlink)
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

    private void CopyIssueIdLink_Click(object sender, RoutedEventArgs e)
    {
        if (IssuesGrid.SelectedItem is JiraIssue issue && !string.IsNullOrWhiteSpace(issue.Key))
        {
            Clipboard.SetText(issue.Key);
        }
    }

    private async void TryCreateBranchExists_Click(object sender, RoutedEventArgs e)
    {
        if (RepositoriesComboBox.SelectionBoxItem is not GitRepository gitRepository)
        {
            return;
        }

        if (IssuesGrid.SelectedItem is not JiraIssue issue)
        {
            return;
        }

        if (DataContext is not ViewModel viewModel)
        {
            return;
        }

        var gitStatusTask = GetGitStatus(gitRepository);

        var currentBranchTask = GetCurrentBranch(gitRepository);

        await Task.WhenAll(gitStatusTask, currentBranchTask).ConfigureAwait(true);

        var gitStatus = await gitStatusTask;
        var currentBranch = await currentBranchTask;

        bool mustStash = false;

        if (gitStatus != GitStatus.Ok)
        {
            viewModel.StatusMessage = $"Git status is not ok: {gitStatus.Message}";

            var messageBoxResult = MessageBox.Show(this, "Status is not clean", "Create branch anyway", MessageBoxButton.OKCancel);

            if (messageBoxResult != MessageBoxResult.OK)
            {
                return;
            }

            mustStash = true;
        }

        if (currentBranch != gitRepository.DefaultGitBranch)
        {
            viewModel.StatusMessage =
                $"Current branch {currentBranch.Name} is not the expected branch {gitRepository.DefaultGitBranch.Name}";
            return;
        }

        if (mustStash)
        {
            await StashChanges(gitRepository);
        }

        await CreateBranch(gitRepository, new GitBranch(viewModel.CustomBranchName ?? issue.GitBranchName()));

        await PopChanges(gitRepository);
    }

    private async Task PopChanges(GitRepository gitRepository)
    {
        var stdOutBuffer = new StringBuilder();
        var result = await Cli.Wrap(GitExePath).WithArguments(new List<string> { "stash", "pop" })
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithWorkingDirectory(gitRepository.FullPath)
            .ExecuteAsync();

        if (DataContext is not ViewModel viewModel)
        {
            return;
        }

        viewModel.StatusMessage = result.ExitCode == 0 ? "Popped stashed changes" : "Failed to pop stashed changes";
    }

    private async Task StashChanges(GitRepository gitRepository)
    {
        var stdOutBuffer = new StringBuilder();
        var result = await Cli.Wrap(GitExePath).WithArguments(new List<string> { "stash" })
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithWorkingDirectory(gitRepository.FullPath)
            .ExecuteAsync();

        if (DataContext is not ViewModel viewModel)
        {
            return;
        }

        viewModel.StatusMessage = result.ExitCode == 0 ? "Stashed changes" : "Failed to stash changes";
    }

    private async Task CreateBranch(GitRepository gitRepository, GitBranch gitBranch)
    {
        var stdOutBuffer = new StringBuilder();
        var result = await Cli.Wrap(GitExePath).WithArguments(new List<string> { "checkout", "-b", gitBranch.Name })
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithWorkingDirectory(gitRepository.FullPath)
            .ExecuteAsync();

        if (DataContext is not ViewModel viewModel)
        {
            return;
        }

        viewModel.StatusMessage = result.ExitCode == 0 ? $"Created branch {gitBranch.Name}" : $"Failed to create branch {gitBranch.Name}";
    }

    private static async Task<GitBranch> GetCurrentBranch(GitRepository gitRepository)
    {
        var stdOutBuffer = new StringBuilder();
        var result = await Cli.Wrap(GitExePath).WithArguments(new List<string> { "rev-parse", "--abbrev-ref", "HEAD" })
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithWorkingDirectory(gitRepository.FullPath)
            .ExecuteAsync();

        string trimmed = stdOutBuffer.ToString().Trim();

        if (result.ExitCode != 0)
        {
            return GitBranch.Unknown(trimmed);
        }

        return trimmed switch
        {
            "develop" => GitBranch.Develop,
            "main" => GitBranch.Main,
            "master" => GitBranch.Master,
            _ => new GitBranch(trimmed)
        };
    }

    private static async Task<GitStatus> GetGitStatus(GitRepository gitRepository)
    {
        var stdOutBuffer = new StringBuilder();
        var result = await Cli.Wrap(GitExePath).WithArguments(new List<string> { "status", "--porcelain" })
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithWorkingDirectory(gitRepository.FullPath)
            .ExecuteAsync();

        return result.ExitCode == 0 && stdOutBuffer.Length == 0
            ? GitStatus.Ok
            : GitStatus.WithMessage(stdOutBuffer.ToString());
    }

    private void BranchNameCustom_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is ViewModel viewModel)
        {
            viewModel.CustomBranchName = BranchNameCustom.Text;
        }
    }
}