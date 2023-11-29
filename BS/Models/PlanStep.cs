namespace BS.Models;

public partial class PlanStep
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public int? CategoryId { get; set; }

    public int Plan { get; set; }

    public double Amount { get; set; }

    public virtual Category? Category { get; set; }
    
    public virtual Plan PlanNavigation { get; set; } = null!;
}
