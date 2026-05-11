using System;
using System.Collections.Generic;

namespace Water_Filtration.Models.data;

public partial class Site
{
    public int SiteId { get; set; }

    public int? Id { get; set; }

    public string? SiteName { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? IsDelete { get; set; }

    public string? PlcSignalDate { get; set; }

    public int? PlcSignal { get; set; }

    public int? Sync { get; set; }
}
