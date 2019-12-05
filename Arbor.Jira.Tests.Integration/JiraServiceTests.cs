using System.Collections.Immutable;
using System.Threading.Tasks;

using Arbor.Jira.Core;

using Xunit;

namespace Arbor.Jira.Tests.Integration
{
    public class JiraServiceTests
    {
        private JiraApp _app;

        [Fact(Skip = "Configuration dependent")]
        public async Task GetIssues()
        {
            await Arrange();

            ImmutableArray<JiraIssue> immutableArray = await _app.Service.GetIssues();

            Assert.False(immutableArray.IsDefault);
        }

        private async Task Arrange() => _app = await JiraApp.CreateAsync();
    }
}