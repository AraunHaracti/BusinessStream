namespace BS.Models;

public partial class PlanExecutionStep
{
    public int Id { get; set; }

    public int RecordId { get; set; }

    public int PlanStepId { get; set; }

    public int PlanExecutionId { get; set; }

    public virtual PlanExecution PlanExecution { get; set; } = null!;

    public virtual PlanStep PlanStep { get; set; } = null!;

    public virtual Record Record { get; set; } = null!;
}
