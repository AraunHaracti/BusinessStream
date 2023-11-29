using System.Drawing;
using Avalonia.Controls;
using BS.Utils;
using BS.ViewModels.MainUserControls;
using BS.Views.MainUserControls;

namespace BS.Demos;

public class RecordDemo : IModule
{
    private RecordListView _view;
    private RecordListViewModel _viewModel;
    
    public string Name => "Записи";
    public UserControl UserInterface
    {
        get 
        {
            if (_view == null)
                CreateView();
            return _view;
        }
    }

    private void CreateView()
    {
        _view = new();
        _viewModel = new();
        _view.DataContext = _viewModel;
    }
    
    public Bitmap Picture
    {
        get; set; 
    }

    public void Deactivate()
    {
        _view = null;
        _viewModel.Dispose();
    }
}