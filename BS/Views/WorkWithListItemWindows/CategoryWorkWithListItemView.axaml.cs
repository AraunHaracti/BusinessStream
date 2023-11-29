using Avalonia.Controls;
using Avalonia.Interactivity;
using BS.ViewModels.WorkWithListItemWindows;

namespace BS.Views.WorkWithListItemWindows;

public partial class CategoryWorkWithListItemView : Window
{
    public CategoryWorkWithListItemView()
    {
        InitializeComponent();
    }

    private void ApplyButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = (CategoryWorkWithListItemViewModel) DataContext;

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