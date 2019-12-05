using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Arbor.Jira.Core
{
    public class JiraService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly JiraConfiguration _jiraConfiguration;

        public JiraService(IHttpClientFactory httpClientFactory, JiraConfiguration jiraConfiguration)
        {
            _httpClientFactory = httpClientFactory;
            _jiraConfiguration = jiraConfiguration;
        }

        public async Task<ImmutableArray<JiraIssue>> GetIssues()
        {
            if (!Uri.TryCreate(_jiraConfiguration.Url, UriKind.Absolute, out Uri uri))
            {
                throw new InvalidOperationException("Invalid or missing URL");
            }

            if (string.IsNullOrWhiteSpace(_jiraConfiguration.Username))
            {
                throw new InvalidOperationException("Missing username");
            }

            if (string.IsNullOrWhiteSpace(_jiraConfiguration.Password))
            {
                throw new InvalidOperationException("Missing password");
            }

            HttpClient httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(_jiraConfiguration.Username + ":" + _jiraConfiguration.Password)));

            string url = string.Format(_jiraConfiguration.Url, _jiraConfiguration.Username);
            string json = await httpClient.GetStringAsync(url);

            var issues = JsonConvert.DeserializeObject<JiraIssueData>(json);

            return issues.Issues
                .OrderByDescending(issue => issue.Key)
                .ToImmutableArray();
        }
    }
}