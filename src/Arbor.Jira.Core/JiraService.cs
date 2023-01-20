using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Arbor.Jira.Core;

public class JiraService
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly JiraConfiguration _jiraConfiguration;

    public JiraService(IHttpClientFactory httpClientFactory, JiraConfiguration jiraConfiguration)
    {
        _httpClientFactory = httpClientFactory;
        _jiraConfiguration = jiraConfiguration;
    }

    public async Task<JiraIssuesResult> GetIssues(bool unresolvedOnly = true)
    {
        if (string.IsNullOrWhiteSpace(_jiraConfiguration.Url))
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
                Encoding.UTF8.GetBytes($"{_jiraConfiguration.Username}:{_jiraConfiguration.Password}")));

        try
        {
            string url =  string.Format(_jiraConfiguration.Url, unresolvedOnly ? "+AND+resolution+%3D+Unresolved" : "");

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                throw new InvalidOperationException($"The URL '{url}' is invalid");
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);

            using var response = await httpClient.SendAsync(request);

            string body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Request '{url}' The status code was {response.StatusCode}, body {body}");
            }

            var issues = JsonConvert.DeserializeObject<JiraIssueData>(body) ?? new();

            return JiraIssuesResult.Issues(issues.Issues
                .OrderByDescending(issue => issue.Key)
                .ToImmutableArray());
        }
        catch (Exception ex)
        {
            return JiraIssuesResult.Error(ex);
        }
    }
}