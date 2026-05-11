using System;
using System.Collections.Generic;

namespace Water_Filtration.Models.data;

public partial class Waterusagesummary
{
    public int Id { get; set; }

    public double? Fs101 { get; set; }

    public double? Fs102 { get; set; }

    public double? Fs301 { get; set; }

    public double? Fs302 { get; set; }

    public double? Fs303 { get; set; }

    public double? Fs601 { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime Cronjobstarttime { get; set; }

    public DateTime Cronjobsendtime { get; set; }

    public int? SiteFk { get; set; }

    public int IsDelete { get; set; }
}
