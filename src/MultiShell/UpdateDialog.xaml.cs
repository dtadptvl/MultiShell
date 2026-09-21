using System.Windows;
using MultiShell.Services;

namespace MultiShell;

public partial class UpdateDialog : Window
{
    public UpdateDialog(UpdateInfo update)
    {
        InitializeComponent();
        VersionText.Text = $"MultiShell {update.Version} is available.";
    }

    private void OnUpdateClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
