using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Arbor.Jira.Core
{
    public class JiraIssue
    {
        private string? _key;

        public TaskFields Fields { get; set; } = new ();

        public string FullName => $"{Key} {Fields.Summary}";

        public string? GitBranch => this.GitBranchName();

        public bool Match(string value, StringComparison stringComparison)
        {
            bool MatchProperty(object? property)
            {
                return property switch
                {
                    string stringProperty => Contains(value, stringProperty, stringComparison),
                    DateTime dateProperty => Contains(value, dateProperty.ToString("yyyy-MM-dd"), stringComparison),
                    _ => false
                };
            }

            object?[] fields = {Key, GitBranch, FullName, Url, Fields.Created, Fields.Status?.Name};

            return fields.Any(MatchProperty);
        }

        private static bool Contains(string searchValue, string? valueProperty, StringComparison stringComparison)
        {
            if (string.IsNullOrWhiteSpace(valueProperty))
            {
                return false;
            }

            return valueProperty.Contains(searchValue, stringComparison);
        }

        public string? Key
        {
            get => _key;
            set
            {
                _key = value;
                SortOrder = GetSortOrder();
            }
        }

        public string? Self { get; set; }

        public string? Url => this.GetUrl();

        public string? SortOrder
        {
            get;
            private set;
        }

        public int IssueNumber { get; private set; }

        public string IssueNumberText => IssueNumber.ToString(CultureInfo.InvariantCulture);

        private string GetSortOrder()
        {
            if (string.IsNullOrEmpty(Key))
            {
                return "";
            }

            string[] parts = Key.Split("-");

            if (parts.Length != 2)
            {
                return Key;
            }

            if (!int.TryParse(parts[1], out int issueNumber))
            {
                return Key;
            }

            IssueNumber = issueNumber;

            string sortOrder = parts[0] + issueNumber
                .ToString(CultureInfo.InvariantCulture)
                .PadLeft(totalWidth: 10, paddingChar: '0');

            return sortOrder;
        }

        public override string ToString() => FullName;
    }
}