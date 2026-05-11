using System;
using System.Collections.Generic;

namespace Water_Filtration.Models.data;

public partial class Plcauditlog
{
    public long Id { get; set; }

    public int TagFk { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime ChangedOn { get; set; }

    public int SiteFk { get; set; }

    public string? ChangedBy { get; set; }

    public ulong IsDelete { get; set; }
}
