using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BS.Models;
using BS.Utils;
using DynamicData;
using MySqlConnector;
using ReactiveUI;

namespace BS.ViewModels.WorkWithListItemWindows;

public class RecordWorkWithListItemViewModel : ViewModelBase
{
    public string Description { get; set; } = "";
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public TimeSpan Time { get; set; } = new(DateTimeOffset.Now.Ticks);
    public string Amount { get; set; } = "";
    
    private string[] _movementTypes = { "null", "income", "expense" };
    public string[] MovementTypes => _movementTypes;

    private int _movementTypeIndex = 0;
    
    public int MovementTypeIndex
    {
        get => _movementTypeIndex;
        set
        {
            _movementTypeIndex = value;

            _typeCategories = new List<Category>();
            
            if (MovementTypeIndex != 0)
                _typeCategories = _categories.Where(it => it.TypeOfMovement == MovementTypes[MovementTypeIndex]).ToList();
            
            var list = _typeCategories.Select(item => item.Name).ToList();

            CategoriesNames = list;
        }
    }
    
    private int _categoriesIndex = -1;

    public int CategoriesIndex
    {
        get => _categoriesIndex;
        set => _categoriesIndex = value;
    }

    private List<Category> _categories;

    private List<Category> _typeCategories;

    private List<string> _categoriesNames = new(); 
    
    public List<string> CategoriesNames
    {
        get => _categoriesNames;
        set
        {
            _categoriesNames = value;
            this.RaisePropertyChanged();
        }
    }
    
    private Action _action;
    
    private bool _isEdit;
    
    private Record _item;

    public RecordWorkWithListItemViewModel()
    {
        string sql = "select * from category";

        _categories = new List<Category>();
        
        using Database db = new Database();
        
        MySqlDataReader reader = db.GetData(sql);
            
        while (reader.Read() && reader.HasRows)
        {
            var currentItem = new Category()
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name"),
                TypeOfMovement = reader.GetString("type_of_movement"),
            };
            
            _categories.Add(currentItem);
        }
    }
    
    public RecordWorkWithListItemViewModel(Action action) : this()
    {
        _action = action;
        _isEdit = false;
    }
    
    public RecordWorkWithListItemViewModel(Action action, Record item) : this()
    {
        _action = action;
        _isEdit = true;
        _item = item;
        
        Description = item.Description;
        Date = item.Date.Value.Date;
        Time = item.Date.Value.TimeOfDay;
        Amount = item.Amount.Value.ToString();

        MovementTypeIndex = MovementTypes.IndexOf(item.Category.TypeOfMovement);
        CategoriesIndex = _typeCategories.IndexOf(_typeCategories.Where(it => it.Id == item.Category.Id).ToList()[0]);
    }
    
    /// <summary>
    /// Выполнение работы с данными
    /// </summary>
    /// <returns>Результат выполнения программы. Выполнена или нет.</returns>
    public bool Apply()
    {
        if (Description == "" || 
            !Regex.IsMatch(Amount.Replace(",", "."), "^(-?)(0|([1-9][0-9]*))(\\.[0-9]+)?$") ||
            CategoriesIndex == -1 ) 
            return false;

        Set();
        
        _action.Invoke();
        
        return true;
    }

    /// <summary>
    /// Взаимодействие с БД. Добавление новой записи или её обновление.
    /// </summary>
    private void Set()
    {
        string sql;
        
        if (_isEdit)
        {
            sql =
                $"update record set " +
                $"description = '{Description}', " +
                $"category_id = {_typeCategories[CategoriesIndex].Id}, " +
                $"date = '{Date.Add(Time).ToString("yy-MM-dd hh:mm:ss")}', " +
                $"amount = '{Amount.Replace(",", ".")}' " +
                $"where id = {_item.Id}";
        }
        else
        {
            sql = $"insert into record (description, category_id, date, amount) values (" +
                  $"'{Description}', " +
                  $"{_typeCategories[CategoriesIndex].Id}, " +
                  $"'{Date.Add(Time).ToString("yyy-MM-dd hh:mm:ss")}', " +
                  $"'{Amount.Replace(",", ".")}')";
        }
        
        using (Database db = new Database())
        {
            db.SetData(sql);
        }
    }
}