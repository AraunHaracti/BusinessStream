using System;
using System.Drawing;
using Avalonia.Controls;
using Avalonia.Platform;
using BS.Utils;
using BS.ViewModels.MainUserControls;
using BS.Views.MainUserControls;

namespace BS.Demos;

public class CategoryDemo : IModule
{
    private CategoryListView _view;
    private CategoryListViewModel _viewModel;
    
    public string Name => "Категории";
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