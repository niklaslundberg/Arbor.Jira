using System.Globalization;
using System.Windows;

namespace Arbor.Jira.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        var currentCulture = new CultureInfo("sv-SE");
        CultureInfo.CurrentCulture = currentCulture;
        CultureInfo.CurrentUICulture = currentCulture;
    }
}