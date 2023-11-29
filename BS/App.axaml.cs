using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BS.Utils;
using BS.ViewModels;
using BS.Views;

namespace BS;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = new MainWindowViewModel(Settings.MainMenuDemos);
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm,
            };

            desktop.MainWindow.Closing += (s, args) => vm.SelectedModule.Deactivate();
        }

        base.OnFrameworkInitializationCompleted();
    }
}