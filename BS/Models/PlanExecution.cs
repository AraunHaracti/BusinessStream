namespace BS.Models;

public partial class PlanExecution
{
    public int Id { get; set; }

    public int Plan { get; set; }

    public double Amount { get; set; }
    
    public virtual Plan PlanNavigation { get; set; } = null!;
}
