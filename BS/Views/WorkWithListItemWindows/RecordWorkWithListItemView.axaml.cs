using Avalonia.Controls;
using Avalonia.Interactivity;
using BS.ViewModels.WorkWithListItemWindows;

namespace BS.Views.WorkWithListItemWindows;

public partial class RecordWorkWithListItemView : Window
{
    public RecordWorkWithListItemView()
    {
        InitializeComponent();
    }

    private void ApplyButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = (RecordWorkWithListItemViewModel) DataContext;

        if (viewModel.Apply())
        {
            Close();
        }    
    }

    private void CloseWindow_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}