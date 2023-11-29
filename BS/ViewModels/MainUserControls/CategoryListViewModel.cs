using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using BS.Models;
using BS.Utils;
using BS.ViewModels.WorkWithListItemWindows;
using BS.Views.WorkWithListItemWindows;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using MySqlConnector;
using ReactiveUI;

namespace BS.ViewModels.MainUserControls;

public class CategoryListViewModel : ViewModelBase, IDisposable
{
    private Window? _parentWindow
    {
        get
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }

            return null;
        }
    }
    
    private List<Category> _itemsFromDatabase;

    private List<Category> _itemsFilter;

    private readonly string _sql = "select * from category";
    
    private readonly int _countItems = 15;
    
    public int CurrentPage { get; set; } = 1;

    public int TotalPages
    {
        get
        {
            int page = (int)Math.Ceiling(_itemsFilter.Count / (double) _countItems);
            return page == 0 ? 1 : page;
        }
    }

    public Category CurrentItem { get; set; }

    private ObservableCollection<Category> _itemsOnDataGrid;
    
    public ObservableCollection<Category> ItemsOnDataGrid
    {
        get => _itemsOnDataGrid;
        set
        {
            _itemsOnDataGrid = value;
            this.RaisePropertyChanged();
        }
    }

    private string _searchQuery = "";
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            _searchQuery = value;
            this.RaisePropertyChanged();
        }
    }
    
    private string[] _movementTypes = new[] { "null", "income", "expense" };
    public string[] MovementTypes { get => _movementTypes; }

    private int _movementTypeIndex = 0;

    public int MovementTypeIndex
    {
        get => _movementTypeIndex;
        set
        {
            _movementTypeIndex = value;
            this.RaisePropertyChanged();
        }
    }

    public CategoryListViewModel()
    {
        GetAndUpdateItems();
        
        PropertyChanged += OnPropertyChanged;
    }

    /// <summary>
    /// Открытие нового окна на добавление категории
    /// </summary>
    public void AddItemButton()
    {
        var view = new CategoryWorkWithListItemView();
        var vm = new CategoryWorkWithListItemViewModel(GetAndUpdateItems);
        view.DataContext = vm;
        view.ShowDialog(_parentWindow);
    }

    /// <summary>
    /// Открытие нового окна на обновление категории. Передача в это окно выбранного объекта класса категории
    /// </summary>
    public void EditItemButton()
    {
        if (CurrentItem == null)
            return;
        var view = new CategoryWorkWithListItemView();
        var vm = new CategoryWorkWithListItemViewModel(GetAndUpdateItems, CurrentItem);
        view.DataContext = vm;
        view.ShowDialog(_parentWindow);
    }
    
    /// <summary>
    /// Удаление выбранного объекта класса категории из БД
    /// </summary>
    public void DeleteItemButton()
    {
        if (CurrentItem == null)
            return;

        int result = 0;
        
        using (Database db = new Database())
        {
            result = db.GetValue($"select count(*) from record where record.category_id = {CurrentItem.Id}");
        }

        if (!(result > 0))
        {
            DeleteItem();    
        }
    }

    /// <summary>
    /// Вызов окна подтверждения удаления категории из БД
    /// </summary>
    private async void DeleteItem()
    {
        var message = MessageBoxManager.GetMessageBoxStandard("Удаление", 
            $"Удалить элемент '{CurrentItem.Name}'?", ButtonEnum.YesNo, Icon.Question);
        

        var result = await message.ShowAsync();

        if (ButtonResult.Yes == result)
        {
            string sql = $"delete from category where id = {CurrentItem.Id}";
        
            using (Database db = new Database())
            {
                db.SetData(sql);
            }

            GetAndUpdateItems();
        }
    }
    
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!Array.Exists(new[] {nameof(SearchQuery), nameof(MovementTypeIndex)}, it => it == e.PropertyName)) 
            return;

        UpdateItems();
    }

    private void GetAndUpdateItems()
    {
        GetDataFromDatabase();
        UpdateItems();
    }

    private void UpdateItems()
    {
        Search();
        Filter();
        this.RaisePropertyChanged(nameof(TotalPages));
        TakeItems(TakeItemsEnum.FirstItems);
        this.RaisePropertyChanged(nameof(CurrentPage));
    }

    /// <summary>
    /// Фильтрация элементов по выбранной категории
    /// </summary>
    private void Filter()
    {
        if (_movementTypeIndex == 0) return;

        _itemsFilter = _itemsFilter.Where(it => it.TypeOfMovement == _movementTypes[_movementTypeIndex]).ToList();
    }
    
    /// <summary>
    /// Поиск по элементам списка записей
    /// </summary>
    private void Search()
    {
        if (SearchQuery == "")
        {
            _itemsFilter = new(_itemsFromDatabase);
            return;
        }

        _itemsFilter = new(_itemsFromDatabase.Where(it => 
            it.Name.ToLower().Contains(SearchQuery.ToLower())));
    }
    
    /// <summary>
    /// Получение данных из БД
    /// </summary>
    private void GetDataFromDatabase()
    {
        _itemsFromDatabase = new List<Category>();

        using Database db = new Database();
        
        MySqlDataReader reader = db.GetData(_sql);
            
        while (reader.Read() && reader.HasRows)
        {
            var currentItem = new Category()
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name"),
                TypeOfMovement = reader.GetString("type_of_movement"),
            };
            
            _itemsFromDatabase.Add(currentItem);
        }
    }
    
    /// <summary>
    /// Пагинация элементов списка
    /// </summary>
    /// <param name="takeItems"></param>
    public void TakeItems(TakeItemsEnum takeItems)
    {
        switch (takeItems)
        {
            default:
            case TakeItemsEnum.FirstItems:
                CurrentPage = 1;
                break;
            case TakeItemsEnum.LastItems:
                CurrentPage = TotalPages;
                break;
            case TakeItemsEnum.NextItems:
                if (CurrentPage < TotalPages)
                    CurrentPage += 1;
                break;
            case TakeItemsEnum.PreviousItems:
                if (CurrentPage > 1)
                    CurrentPage -= 1;
                break;
        }
        
        this.RaisePropertyChanged(nameof(CurrentPage));

        ItemsOnDataGrid = new ObservableCollection<Category>(_itemsFilter.Skip((CurrentPage - 1) * _countItems).Take(_countItems));
    }
    
    public void Dispose() { }
}