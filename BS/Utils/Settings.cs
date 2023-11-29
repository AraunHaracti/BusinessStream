using System.Collections.Generic;
using BS.Demos;
using BS.Views.MainUserControls;
using MySqlConnector;

namespace BS.Utils;

public static class Settings
{
    public static MySqlConnectionStringBuilder DatabaseConnectionStringBuilder = new()
    {
        Server = "localhost",
        Port = 3306,
        Database = "bs",
        UserID = "root",
        Password = "password"
    };

    public static IEnumerable<IModule> MainMenuDemos = new List<IModule>()
    {
        new CategoryDemo(), new RecordDemo(),
    };
}