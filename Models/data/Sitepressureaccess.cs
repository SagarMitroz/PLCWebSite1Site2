using System;
using System.Collections.Generic;

namespace Water_Filtration.Models.data;

public partial class Sitepressureaccess
{
    public int SitepressureId { get; set; }

    public int? FkSite { get; set; }

    public int? FkPressure { get; set; }

    public bool? HasAccess { get; set; }

    public DateTime? CreatedOn { get; set; }
}
