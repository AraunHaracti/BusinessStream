using System;
using BS.Models;
using BS.Utils;
using DynamicData;

namespace BS.ViewModels.WorkWithListItemWindows;

public class CategoryWorkWithListItemViewModel
{
    private string _categoryName = "";

    private Category _item;
    
    public string CategoryName { get => _categoryName; set => _categoryName = value; }

    private string[] _movementTypes = { "null", "income", "expense" };
    public string[] MovementTypes => _movementTypes;

    private int _movementTypeIndex = 0;
    public int MovementTypeIndex { get => _movementTypeIndex; set => _movementTypeIndex = value; }

    private Action _action;
    
    private bool _isEdit;

    public CategoryWorkWithListItemViewModel(Action action)
    {
        _action = action;
        _isEdit = false;
    }
    
    public CategoryWorkWithListItemViewModel(Action action, Category item)
    {
        _action = action;
        _isEdit = true;
        _item = item;
        
        if (item.Name != null) CategoryName = item.Name;
        MovementTypeIndex = item.TypeOfMovement == null ? 0 : _movementTypes.IndexOf(item.TypeOfMovement);
    }

    /// <summary>
    /// Выполнение работы с данными
    /// </summary>
    /// <returns>Результат выполнения программы. Выполнена или нет.</returns>
    public bool Apply()
    {
        if (_categoryName == "" || _movementTypeIndex == 0) return false;

        Set();
        
        _action.Invoke();

        return true;
    }

    /// <summary>
    /// Взаимодействие с БД. Добавление новой категории или её обновление.
    /// </summary>
    private void Set()
    {
        string sql;
        
        if (_isEdit)
        {
            sql =
                $"update category set " +
                $"name = '{_categoryName}', " +
                $"type_of_movement = '{_movementTypes[_movementTypeIndex]}' " +
                $"where id = {_item.Id}";
        }
        else
        {
            sql = $"insert into category (name, type_of_movement) values (" +
                  $"'{_categoryName}', " +
                  $"'{_movementTypes[_movementTypeIndex]}')";
        }

        using (Database db = new Database())
        {
            db.SetData(sql);
        }
    }
}