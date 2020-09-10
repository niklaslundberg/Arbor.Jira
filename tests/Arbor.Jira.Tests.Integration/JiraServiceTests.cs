using System.Threading.Tasks;
using Arbor.Jira.Core;
using Xunit;

namespace Arbor.Jira.Tests.Integration
{
    public class JiraServiceTests
    {
        private JiraApp? _app;

        [Fact(Skip = "Configuration dependent")]
        public async Task GetIssues()
        {
            await Arrange();

            Assert.NotNull(_app);

            var immutableArray = (await _app!.Service.GetIssues()).IssueList;

            Assert.False(immutableArray.IsDefault);
        }

        private async Task Arrange() => _app = await JiraApp.CreateAsync();
    }
}