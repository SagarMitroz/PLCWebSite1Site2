using System;
using System.Collections.Generic;

namespace Water_Filtration.Models.data;

public partial class Filtrationcyclesummary
{
    public int Id { get; set; }

    public int? SiteFk { get; set; }

    public double? FItrationrunning { get; set; }

    public double? FiltrationRuntime { get; set; }

    public double? Standby { get; set; }

    public double? Backwash { get; set; }

    public double? Cip { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? SummaryDate { get; set; }
}
