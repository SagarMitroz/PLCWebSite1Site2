using System;
using System.Collections.Generic;

namespace Water_Filtration.Models.data;

public partial class Connectionstatus
{
    public int Id { get; set; }

    public int? SiteFk { get; set; }

    public DateTime? LastRecordTime { get; set; }
}
