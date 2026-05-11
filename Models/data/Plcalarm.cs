using System;
using System.Collections.Generic;

namespace Water_Filtration.Models.data;

public partial class Plcalarm
{
    public long Id { get; set; }

    public int TagFk { get; set; }

    public double Value { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? SiteFk { get; set; }

    public ulong IsDelete { get; set; }
}
