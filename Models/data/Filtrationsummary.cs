using System;
using System.Collections.Generic;

namespace Water_Filtration.Models.data;

public partial class Filtrationsummary
{
    public long Id { get; set; }

    public int SiteFk { get; set; }

    public string FiltrationEvent { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public int? FiltrationRunTime { get; set; }

    public DateTime EventStartTime { get; set; }

    public DateTime? EventEndTime { get; set; }

    public int? IsDelete { get; set; }
}
