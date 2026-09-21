using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using MultiShell.Terminal;

namespace MultiShell.Models;

public abstract class SidebarNodeViewModel : INotifyPropertyChanged
{
    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed class FolderGroupViewModel : SidebarNodeViewModel
{
    public required string Name { get; init; }
    public required string Folder { get; init; }
    public ObservableCollection<ShellItemViewModel> Shells { get; } = [];
}

public sealed class ShellItemViewModel : SidebarNodeViewModel
{
    public required ShellDefinition Definition { get; init; }
    public required string PresetName { get; init; }
    public required ShellSessionState State { get; init; }

    public string Folder => Definition.Folder;
    public bool IsFull => Definition.ApprovalMode == ApprovalMode.Full;
    public Visibility FullVisibility => IsFull ? Visibility.Visible : Visibility.Collapsed;
    public string StateGlyph => State == ShellSessionState.Running ? "●" : State == ShellSessionState.Starting ? "◐" : "○";
    public string StateText => State.ToString();
}
