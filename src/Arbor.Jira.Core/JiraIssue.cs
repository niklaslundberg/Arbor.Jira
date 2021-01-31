using System.Globalization;

namespace Arbor.Jira.Core
{
    public class JiraIssue
    {
        private string? _key;
        public TaskFields Fields { get; set; } = new TaskFields();

        public string FullName => $"{Key} {Fields.Summary}";

        public string? GitBranch => this.GitBranchName();

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

            if (!int.TryParse(parts[1], out int index))
            {
                return Key;
            }

            string sortOrder = parts[0] + index.ToString(CultureInfo.InvariantCulture)
                .PadLeft(totalWidth: 10, paddingChar: '0');

            return sortOrder;
        }

        public override string ToString() => FullName;
    }
}