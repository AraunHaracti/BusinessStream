using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using BS.Utils;

namespace BS.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged = delegate { };
    
    public List<IModule> Modules { get; }
    
    private IModule _SelectedModule;
    public IModule SelectedModule
    {
        get => _SelectedModule;
        set
        {
            if (value == _SelectedModule) return;
            if (_SelectedModule != null) _SelectedModule.Deactivate();
            _SelectedModule = value;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedModule)));
            PropertyChanged(this, new PropertyChangedEventArgs("UserInterface"));
        }
    }

    public UserControl UserInterface
    {
        get
        {
            if (SelectedModule == null) 
                return null;
            return SelectedModule.UserInterface;
        }
    }
    
    public MainWindowViewModel(IEnumerable<IModule> modules)
    {
        Modules = new List<IModule>(modules);
        
        if (Modules.Count > 0)
        {
            SelectedModule = Modules[0];
        }
    }
}