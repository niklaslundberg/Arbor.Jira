using System.Threading.Tasks;
using Arbor.Jira.Core;
using FluentAssertions;
using Xunit;

namespace Arbor.Jira.Tests.Integration;

public class JiraServiceTests
{
    private JiraApp? _app;

    [Fact(Skip = "Configuration dependent")]
    public async Task GetIssues()
    {
        await Arrange();

        _app.Should().NotBeNull();

        var immutableArray = (await _app!.Service.GetIssues()).IssueList;

        immutableArray.IsDefault.Should().BeFalse();
    }

    private async Task Arrange() => _app = await JiraApp.CreateAsync();
}