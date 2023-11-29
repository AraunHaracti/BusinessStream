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

public class RecordListViewModel : ViewModelBase, IDisposable
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
    
    private List<Record> _itemsFromDatabase;

    private List<Record> _itemsFilter;

    private readonly string _sql = "select " +
                                   "record.id as record_id, " +
                                   "record.description as description, " +
                                   "record.date as date, " +
                                   "record.amount as amount, " +
                                   "category.id as category_id, " +
                                   "category.name as name, " +
                                   "category.type_of_movement as type_of_movement " +
                                   "from record " +
                                   "join category " +
                                   "on record.category_id = category.id ";
    
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
    
    public Record CurrentItem { get; set; }

    private ObservableCollection<Record> _itemsOnDataGrid;
    
    public ObservableCollection<Record> ItemsOnDataGrid
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
    
    private string[] _movementTypes = { "null", "income", "expense" };
    public string[] MovementTypes => _movementTypes;

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

    private string[] _sortTypes = { "datetime", "description", "amount", "category" };
    
    public string[] SortTypes => _sortTypes;
    
    private int _sortTypeIndex = 0;

    public int SortTypeIndex
    {
        get => _sortTypeIndex;
        set
        {
            _sortTypeIndex = value;
            this.RaisePropertyChanged();
        }
    }

    private bool _isReverse = false;

    public bool IsReverse
    {
        get => _isReverse;
        set
        {
            _isReverse = value;
            this.RaisePropertyChanged();
        }
    }
    

    public RecordListViewModel()
    {
        GetAndUpdateItems();
        
        PropertyChanged += OnPropertyChanged;
    }
    
    /// <summary>
    /// Открытие нового окна на добавление записи
    /// </summary>
    public void AddItemButton()
    {
        var view = new RecordWorkWithListItemView();
        var vm = new RecordWorkWithListItemViewModel(GetAndUpdateItems);
        view.DataContext = vm;
        view.ShowDialog(_parentWindow);
    }

    /// <summary>
    /// Открытие нового окна на обновление записи. Передача в это окно выбранного объекта класса записи
    /// </summary>
    public void EditItemButton()
    {
        if (CurrentItem == null)
            return;
        var view = new RecordWorkWithListItemView();
        var vm = new RecordWorkWithListItemViewModel(GetAndUpdateItems, CurrentItem);
        view.DataContext = vm;
        view.ShowDialog(_parentWindow);
    }

    /// <summary>
    /// Удаление выбранного объекта класса записи из БД
    /// </summary>
    public void DeleteItemButton()
    {
        if (CurrentItem == null)
            return;
        DeleteItem();
    }

    /// <summary>
    /// Вызов окна подтверждения удаления записи из БД
    /// </summary>
    private async void DeleteItem()
    {
        var a = MessageBoxManager.GetMessageBoxStandard("Удаление", $"Удалить элемент '{CurrentItem.Description}'?",
            ButtonEnum.YesNo, Icon.Question);

        var b = await a.ShowAsync();

        if (ButtonResult.Yes == b)
        {
            string sql = $"delete from record where id = {CurrentItem.Id}";
        
            using (Database db = new Database())
            {
                db.SetData(sql);
            }

            GetAndUpdateItems();
        }
    }
    
    
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!Array.Exists(new string[] {nameof(SearchQuery), nameof(MovementTypeIndex), nameof(IsReverse), nameof(SortTypeIndex)}, it => it == e.PropertyName)) 
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
        Sort();
        Reverse();
        this.RaisePropertyChanged(nameof(TotalPages));
        TakeItems(TakeItemsEnum.FirstItems);
        this.RaisePropertyChanged(nameof(CurrentPage));
    }

    /// <summary>
    /// Сортировка элементов по выбранной категории выпадающего списка
    /// </summary>
    private void Sort()
    {
        _itemsFilter = SortTypes[SortTypeIndex] switch
        {
            "datetime" => _itemsFilter.OrderBy(record => record.Date).ToList(),
            "description" => _itemsFilter.OrderBy(record => record.Description).ToList(),
            "amount" => _itemsFilter.OrderBy(record => record.Amount).ToList(),
            "category" => _itemsFilter.OrderBy(record => record.Category.Name).ToList(),
            _ => _itemsFilter
        };
    }

    /// <summary>
    /// Переворачивание порядка элементов
    /// </summary>
    private void Reverse()
    {
        if (IsReverse)
            _itemsFilter.Reverse();
    }

    /// <summary>
    /// Фильтрация элементов по выбранной категории
    /// </summary>
    private void Filter()
    {
        if (_movementTypeIndex == 0) return;

        _itemsFilter = _itemsFilter.Where(it => it.Category.TypeOfMovement == _movementTypes[_movementTypeIndex]).ToList();
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
            it.Description.ToLower().Contains(SearchQuery.ToLower()) || 
            it.Date.ToString().Contains(SearchQuery) || 
            it.Amount.ToString().Contains(SearchQuery) || 
            it.Category.Name.ToLower().Contains(SearchQuery.ToLower())));
    }
    
    /// <summary>
    /// Получение данных из БД
    /// </summary>
    private void GetDataFromDatabase()
    {
        _itemsFromDatabase = new List<Record>();

        using Database db = new Database();
        
        MySqlDataReader reader = db.GetData(_sql);
            
        while (reader.Read() && reader.HasRows)
        {
            var currentItem = new Record()
            {
                Id = reader.GetInt32("record_id"),
                Description = reader.GetString("description"),
                Date = reader.GetDateTimeOffset("date"),
                Amount = reader.GetDouble("amount"),
                CategoryId = reader.GetInt32("category_id"),
                Category = new Category()
                {
                    Id = reader.GetInt32("category_id"),
                    Name = reader.GetString("name"),
                    TypeOfMovement = reader.GetString("type_of_movement")
                }
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

        ItemsOnDataGrid = new ObservableCollection<Record>(_itemsFilter.Skip((CurrentPage - 1) * _countItems).Take(_countItems));
    }
    
    public void Dispose() { }
}