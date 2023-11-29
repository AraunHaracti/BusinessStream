using System;

namespace BS.Models;

public partial class Record
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public int CategoryId { get; set; }

    public DateTimeOffset? Date { get; set; }

    public double? Amount { get; set; }

    public virtual Category Category { get; set; } = null!;
}
