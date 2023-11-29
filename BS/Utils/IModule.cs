using System.Drawing;
using Avalonia.Controls;

namespace BS.Utils;

public interface IModule
{
    string Name { get; }
    UserControl UserInterface { get; }
    Bitmap Picture { get; }
    void Deactivate();
}