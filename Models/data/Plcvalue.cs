using System;
using System.Collections.Generic;

namespace Water_Filtration.Models.data;

public partial class Plcvalue
{
    public long Id { get; set; }

    public int? SiteFk { get; set; }

    public double? Tl01 { get; set; }

    public double? Tl03 { get; set; }

    public double? Tl04 { get; set; }

    public double? Tl05 { get; set; }

    public double? Tl06 { get; set; }

    public double? Fs101 { get; set; }

    public double? Fs102 { get; set; }

    public double? Fs301 { get; set; }

    public double? Fs302 { get; set; }

    public double? Fs303 { get; set; }

    public double? Fs601 { get; set; }

    public double? Ps301 { get; set; }

    public double? Ps302 { get; set; }

    public double? Ps303 { get; set; }

    public double? Ts301 { get; set; }

    public double? Ts302 { get; set; }

    public double? Qs301 { get; set; }

    public double? Phs601 { get; set; }

    public double? Cls601 { get; set; }

    public double? Qcs601 { get; set; }

    public double? SelectedCycle { get; set; }

    public double? StepNo { get; set; }

    public double? SetTimeFiltration { get; set; }

    public double? ActTimeFiltration { get; set; }

    public double? SetTimeBackflush { get; set; }

    public double? ActTimeBackflush { get; set; }

    public double? FiltrationRunHrs { get; set; }

    public double? BackflushCount { get; set; }

    public double? Spare3 { get; set; }

    public double? Spare4 { get; set; }

    public double? Spare5 { get; set; }

    public DateTime CreatedOn { get; set; }
}
