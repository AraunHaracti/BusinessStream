namespace BS.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string TypeOfMovement { get; set; } = null!;
}
