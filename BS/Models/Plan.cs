using System;

namespace BS.Models;

public partial class Plan
{
    public int Id { get; set; }

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }

    public double Amount { get; set; }
}
